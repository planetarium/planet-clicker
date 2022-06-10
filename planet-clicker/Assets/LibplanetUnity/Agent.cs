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
        private BaseMiner<PolymorphicAction<ActionBase>> _miner;

        private SwarmRunner _swarmRunner;

        private readonly ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();

        private PrivateKey PrivateKey { get; set; }

        private NodeConfig<PolymorphicAction<ActionBase>> _nodeConfig;

        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        private BlockChain<PolymorphicAction<ActionBase>> _blockChain;

        public Address Address { get; private set; }

        public static void Initialize(
            IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers,
            BaseMiner<PolymorphicAction<ActionBase>> miner = null)
        {
            Instance.InitAgent(renderers, miner);
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

        private void InitAgent(
            IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers,
            BaseMiner<PolymorphicAction<ActionBase>> miner)
        {
            var storagePath = Paths.StorePath;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Init(storagePath, renderers, miner);

            StartCoroutines();
        }

        private void Init(
            string storagePath,
            IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers,
            BaseMiner<PolymorphicAction<ActionBase>> miner)
        {
            SwarmConfig swarmConfig = InitHelper.GetSwarmConfig(Paths.SwarmConfigPath);
            Block<PolymorphicAction<ActionBase>> genesis = InitHelper.GetGenesisBlock(Paths.GenesisBlockPath);
            (IStore store, IStateStore stateStore) = InitHelper.GetStore(Paths.StorePath);
            // NOTE: Agent private key doesn't necessarily have to match swarm private key.
            PrivateKey = InitHelper.GetPrivateKey(Paths.PrivateKeyPath);

            _nodeConfig = new NodeConfig<PolymorphicAction<ActionBase>>(
                PrivateKey,
                new NetworkConfig<PolymorphicAction<ActionBase>>(
                    NodeUtils<PolymorphicAction<ActionBase>>.DefaultBlockPolicy,
                    NodeUtils<PolymorphicAction<ActionBase>>.DefaultStagePolicy,
                    genesis),
                swarmConfig,
                store,
                stateStore,
                renderers);
            _nodeConfig.SwarmConfig.InitConfig.Host = "localhost";
            _swarm = _nodeConfig.GetSwarm();
            _blockChain = _swarm.BlockChain;

            // NOTE: Agent private key doesn't necessarily have to match swarm private key.
            Address = PrivateKey.PublicKey.ToAddress();

            _swarmRunner = new SwarmRunner(_swarm, PrivateKey);
            if (miner is null)
            {
                _miner = new SoloMiner<PolymorphicAction<ActionBase>>(
                    _blockChain,
                    PrivateKey,
                    new DefaultMineHandler(_blockChain, PrivateKey));
            } else 
            {
                _miner = miner;
            }
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

        private void StartCoroutines()
        {
            StartCoroutine(_swarmRunner.CoSwarmRunner());
            StartCoroutine(CoProcessActions());
            StartNullableCoroutine(_miner.CoStart());
        }

        private Coroutine StartNullableCoroutine(IEnumerator routine)
        {
            return ReferenceEquals(routine, null) ? null : StartCoroutine(routine);
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
    }
}
