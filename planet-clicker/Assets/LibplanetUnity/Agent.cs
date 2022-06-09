using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Renderers;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Store;
using Libplanet.Tx;
using Libplanet.Unity.Miner;
using Libplanet.Unity;
using NetMQ;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Libplanet.Node;
using UnityEngine;
using UnityEditor;

namespace LibplanetUnity
{
    public class Agent : MonoSingleton<Agent>
    {
        private static BaseMiner<PolymorphicAction<ActionBase>> _miner;

        private static IEnumerator _swarmRunner;

        private readonly ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();

        private PrivateKey PrivateKey { get; set; }

        private NodeConfig<PolymorphicAction<ActionBase>> _nodeConfig;

        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        private BlockChain<PolymorphicAction<ActionBase>> _blockChain;

        private CancellationTokenSource _cancellationTokenSource;

        public Address Address { get; private set; }

        public static void Initialize(IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            Instance.InitAgent(renderers);
        }

        public IValue GetState(Address address)
        {
            return _blockChain.GetState(address);
        }

        public void MakeTransaction(IEnumerable<ActionBase> gameActions)
        {
            var actions = gameActions.Select(gameAction => (PolymorphicAction<ActionBase>)gameAction).ToList();
            Task.Run(() => MakeTransaction(actions, true));
        }

        private void InitAgent(IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            var storagePath = Paths.StorePath;

            // TODO: Provide a way to configure APV.
            AppProtocolVersion appProtocolVersion = default;
            IEnumerable<PublicKey> trustedAppProtocolVersionSigners = new List<PublicKey>();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Init(
                storagePath,
                appProtocolVersion,
                trustedAppProtocolVersionSigners,
                renderers);

            _miner = new SoloMiner<PolymorphicAction<ActionBase>>(
                _blockChain,
                PrivateKey,
                new MineHandler());

            StartSystemCoroutines();
            StartNullableCoroutine(_miner.CoStart());
        }

        private void Init(
            string storagePath,
            AppProtocolVersion appProtocolVersion,
            IEnumerable<PublicKey> trustedAppProtocolVersionSigners,
            IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            SwarmConfig swarmConfig = InitHelper.GetSwarmConfig(Paths.SwarmConfigPath);
            Block<PolymorphicAction<ActionBase>> genesis = InitHelper.GetGenesisBlock(Paths.GenesisBlockPath);
            (IStore store, IStateStore stateStore) = InitHelper.GetStore(Paths.StorePath);
            // NOTE: Agent private key doesn't necessarily have to match swarm private key.
            PrivateKey = InitHelper.GetPrivateKey(Paths.PrivateKeyPath);
            Address = PrivateKey.PublicKey.ToAddress();

            _nodeConfig = new NodeConfig<PolymorphicAction<ActionBase>>(
                new PrivateKey(),
                new NetworkConfig<PolymorphicAction<ActionBase>>(
                    NodeUtils<PolymorphicAction<ActionBase>>.DefaultBlockPolicy,
                    NodeUtils<PolymorphicAction<ActionBase>>.DefaultStagePolicy,
                    genesis),
                swarmConfig,
                store,
                stateStore,
                renderers);
            _swarm = _nodeConfig.GetSwarm();
            _blockChain = _swarm.BlockChain;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void RunOnMainThread(System.Action action)
        {
            _actions.Enqueue(action);
        }

        #region Mono
        protected override void OnDestroy()
        {
            NetMQConfig.Cleanup(false);

            base.OnDestroy();
            _swarm?.Dispose();
        }
        #endregion

        private void StartSystemCoroutines()
        {
            _swarmRunner = CoSwarmRunner();

            StartNullableCoroutine(_swarmRunner);
            StartCoroutine(CoProcessActions());
        }

        private Coroutine StartNullableCoroutine(IEnumerator routine)
        {
            return ReferenceEquals(routine, null) ? null : StartCoroutine(routine);
        }

        private IEnumerator CoSwarmRunner()
        {
            if (_swarm is null)
            {
                yield break;
            }

            var bootstrapTask = Task.Run(async () =>
            {
                try
                {
                    // FIXME: Swarm<T> should handle the filtering internally.
                    await _swarm.BootstrapAsync(
                        cancellationToken: _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Debug.LogFormat("Exception occurred during bootstrap {0}", e);
                }
            });

            yield return new WaitUntil(() => bootstrapTask.IsCompleted);

            Debug.Log("PreloadingStarted event was invoked");

            DateTimeOffset started = DateTimeOffset.UtcNow;
            long existingBlocks = _blockChain?.Tip?.Index ?? 0;
            Debug.Log("Starting preload...");

            var swarmPreloadTask = Task.Run(async () =>
            {
                await _swarm.PreloadAsync(
                    progress: null,
                    render: false,
                    cancellationToken: _cancellationTokenSource.Token);
            });

            yield return new WaitUntil(() => swarmPreloadTask.IsCompleted);
            DateTimeOffset ended = DateTimeOffset.UtcNow;

            if (swarmPreloadTask.Exception is Exception exc)
            {
                Debug.LogErrorFormat(
                    "Preload terminated with an exception: {0}",
                    exc
                );
                throw exc;
            }

            var index = _blockChain?.Tip?.Index ?? 0;
            Debug.LogFormat(
                "Preload finished; elapsed time: {0}; blocks: {1}",
                ended - started,
                index - existingBlocks
            );

            var swarmStartTask = Task.Run(async () =>
            {
                try
                {
                    await _swarm.StartAsync(cancellationToken: _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat(
                        "Swarm terminated with an exception: {0}",
                        e
                    );
                    throw;
                }
            });

            Task.Run(async () =>
            {
                await _swarm.WaitForRunningAsync();

                Debug.LogFormat(
                    "The address of this node: {0},{1},{2}",
                    ByteUtil.Hex(PrivateKey.PublicKey.Format(true)),
                    _swarm.EndPoint.Host,
                    _swarm.EndPoint.Port
                );
            });

            yield return new WaitUntil(() => swarmStartTask.IsCompleted);
        }

        private IEnumerator CoProcessActions()
        {
            while (true)
            {
                if (_actions.TryDequeue(out System.Action action))
                {
                    action();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private static bool WantsToQuit()
        {
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        private Transaction<PolymorphicAction<ActionBase>> MakeTransaction(
                    IEnumerable<PolymorphicAction<ActionBase>> actions, bool broadcast)
        {
            var polymorphicActions = actions.ToArray();
            Debug.LogFormat("Make Transaction with Actions: `{0}`",
                string.Join(",", polymorphicActions.Select(i => i.InnerAction)));
            return _blockChain.MakeTransaction(PrivateKey, polymorphicActions);
        }

        public class MineHandler : IMineListener<PolymorphicAction<ActionBase>>
        {
            public void Failure(Task mineTask)
            {
                var txs = new HashSet<Transaction<PolymorphicAction<ActionBase>>>();
                var invalidTxs = txs;
                var retryActions = new HashSet<IImmutableList<PolymorphicAction<ActionBase>>>();

                if (mineTask.IsFaulted)
                {
                    foreach (var ex in mineTask.Exception.InnerExceptions)
                    {
                        if (ex is InvalidTxNonceException invalidTxNonceException)
                        {
                            var invalidNonceTx = Agent.Instance._blockChain.GetTransaction(invalidTxNonceException.TxId);

                            if (invalidNonceTx.Signer == Agent.Instance.PrivateKey.PublicKey.ToAddress())
                            {
                                Debug.Log($"Tx[{invalidTxNonceException.TxId}] nonce is invalid. Retry it.");
                                retryActions.Add(invalidNonceTx.Actions);
                            }
                        }

                        if (ex is InvalidTxException invalidTxException)
                        {
                            Debug.Log($"Tx[{invalidTxException.TxId}] is invalid. mark to unstage.");
                            invalidTxs.Add(Agent.Instance._blockChain.GetTransaction(invalidTxException.TxId));
                        }

                        Debug.LogException(ex);
                    }
                }

                foreach (var invalidTx in invalidTxs)
                {
                    Agent.Instance._blockChain.UnstageTransaction(invalidTx);
                }

                foreach (var retryAction in retryActions)
                {
                    Agent.Instance.MakeTransaction(retryAction, true);
                }
            }
            public void Success(Block<PolymorphicAction<ActionBase>> block)
            {
                Debug.Log($"created block index: {block.Index}, difficulty: {block.Difficulty}");
            }
        }
    }
}
