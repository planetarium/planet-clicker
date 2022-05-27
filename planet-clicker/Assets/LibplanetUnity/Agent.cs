using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Blockchain.Renderers;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Store;
using Libplanet.Tx;
using LibplanetUnity.Action;
using LibplanetUnity.Helper;
using NetMQ;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Libplanet.Node;
using UnityEngine;

namespace LibplanetUnity
{
    public class Agent : MonoSingleton<Agent>
    {
        private static readonly TimeSpan BlockInterval = TimeSpan.FromSeconds(10);

        private const int SwarmDialTimeout = 5000;

        private const string PlayerPrefsKeyOfAgentPrivateKey = "private_key_agent";

        private const string AgentStoreDirName = "planetarium";

        private static readonly string CommandLineOptionsJsonPath = Path.Combine(Application.streamingAssetsPath, "clo.json");

        public static readonly string GenesisBlockPath = Path.Combine(Application.streamingAssetsPath, "genesis");

        public static readonly string DefaultStoragePath =
            Path.Combine(Application.persistentDataPath, AgentStoreDirName);

        private static IEnumerator _miner;

        private static IEnumerator _swarmRunner;

        private readonly ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();

        private PrivateKey PrivateKey { get; set; }

        private BlockChain<PolymorphicAction<ActionBase>> _blocks;

        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        private IStore _store;

        private IStateStore _stateStore;

        private ImmutableList<Peer> _seedPeers;

        private CancellationTokenSource _cancellationTokenSource;

        public Address Address { get; private set; }

        public IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> Renderers { get; private set; }

        public static void Initialize(IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            instance.InitAgent(renderers);
        }

        public static void CreateGenesisBlock()
        {
            Block<PolymorphicAction<ActionBase>> genesisBlock =
                NodeUtils<PolymorphicAction<ActionBase>>.CreateGenesisBlock();
            NodeUtils<PolymorphicAction<ActionBase>>.SaveGenesisBlock(GenesisBlockPath, genesisBlock);
        }

        public IValue GetState(Address address)
        {
            return _blocks.GetState(address);
        }

        public void MakeTransaction(IEnumerable<ActionBase> gameActions)
        {
            var actions = gameActions.Select(gameAction => (PolymorphicAction<ActionBase>)gameAction).ToList();
            Task.Run(() => MakeTransaction(actions, true));
        }

        private void InitAgent(IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            var options = GetOptions(CommandLineOptionsJsonPath);
            var privateKey = GetPrivateKey(options);
            var peers = options.Peers.Select(LoadPeer).ToImmutableList();
            var iceServers = options.IceServers.Select(LoadIceServer).ToImmutableList();
            var host = options.Host;
            int? port = options.Port;
            var storagePath = options.StoragePath ?? DefaultStoragePath;
            var appProtocolVersion = options.AppProtocolVersion is null
                ? default
                : AppProtocolVersion.FromToken(options.AppProtocolVersion);
            var trustedAppProtocolVersionSigners = options.TrustedAppProtocolVersionSigners
                .Select(s => new PublicKey(ByteUtil.ParseHex(s)));

            if (options.Logging)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
            }

            Init(
                privateKey,
                storagePath,
                peers,
                iceServers,
                host,
                port,
                appProtocolVersion,
                trustedAppProtocolVersionSigners,
                renderers
                );

            _miner = options.NoMiner ? null : CoMiner();

            StartSystemCoroutines();
            StartNullableCoroutine(_miner);
        }

        private void Init(
            PrivateKey privateKey,
            string path,
            IEnumerable<Peer> peers,
            IEnumerable<IceServer> iceServers,
            string host,
            int? port,
            AppProtocolVersion appProtocolVersion,
            IEnumerable<PublicKey> trustedAppProtocolVersionSigners,
            IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> renderers)
        {
            var policy = new BlockPolicy<PolymorphicAction<ActionBase>>(
                null,
                BlockInterval,
                2048,
                2048);
            var stagePolicy = new VolatileStagePolicy<PolymorphicAction<ActionBase>>();
            PrivateKey = privateKey;
            Address = privateKey.PublicKey.ToAddress();
            // TODO: Use RocksDBStore instead:
            (_store, _stateStore) = NodeUtils<PolymorphicAction<ActionBase>>.LoadStore(path);
            Block<PolymorphicAction<ActionBase>> genesis =
                NodeUtils<PolymorphicAction<ActionBase>>.LoadGenesisBlock(GenesisBlockPath);
            _blocks = new BlockChain<PolymorphicAction<ActionBase>>(
                policy,
                stagePolicy,
                _store,
                _stateStore,
                genesis,
                renderers
            );

            if (!(host is null) || iceServers.Any())
            {
                _swarm = new Swarm<PolymorphicAction<ActionBase>>(
                    _blocks,
                    privateKey,
                    appProtocolVersion: appProtocolVersion,
                    host: host,
                    listenPort: port,
                    iceServers: iceServers,
                    trustedAppProtocolVersionSigners: trustedAppProtocolVersionSigners);

                _seedPeers = peers.Where(peer => peer.PublicKey != privateKey.PublicKey).ToImmutableList();
                _seedPeers.Select(peer => peer.Address).ToImmutableHashSet();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private static Options GetOptions(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                return JsonUtility.FromJson<Options>(
                    File.ReadAllText(jsonPath)
                );
            }
            else
            {
                return CommnadLineParser.GetCommandLineOptions() ?? new Options();
            }
        }

        public void RunOnMainThread(System.Action action)
        {
            _actions.Enqueue(action);
        }

        private static PrivateKey GetPrivateKey(Options options)
        {
            PrivateKey privateKey;
            var privateKeyHex = options.PrivateKey ?? PlayerPrefs.GetString(PlayerPrefsKeyOfAgentPrivateKey, "");

            if (string.IsNullOrEmpty(privateKeyHex))
            {
                privateKey = new PrivateKey();
                PlayerPrefs.SetString(PlayerPrefsKeyOfAgentPrivateKey, ByteUtil.Hex(privateKey.ByteArray));
            }
            else
            {
                privateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyHex));
            }

            return privateKey;
        }

        private static BoundPeer LoadPeer(string peerInfo)
        {
            string[] tokens = peerInfo.Split(',');
            var pubKey = new PublicKey(ByteUtil.ParseHex(tokens[0]));
            string host = tokens[1];
            var port = int.Parse(tokens[2]);

            return new BoundPeer(pubKey, new DnsEndPoint(host, port));
        }

        private static IceServer LoadIceServer(string iceServerInfo)
        {
            var uri = new Uri(iceServerInfo);
            string[] userInfo = uri.UserInfo.Split(':');

            return new IceServer(new[] { uri }, userInfo[0], userInfo[1]);
        }

        #region Mono

        protected override void OnDestroy()
        {
            NetMQConfig.Cleanup(false);

            base.OnDestroy();
            _store.Dispose();
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
                    await _swarm.BootstrapAsync(
                        _seedPeers,
                        TimeSpan.FromMilliseconds(5000),
                        cancellationToken: _cancellationTokenSource.Token
                    );
                }
                catch (Exception e)
                {
                    Debug.LogFormat("Exception occurred during bootstrap {0}", e);
                }
            });

            yield return new WaitUntil(() => bootstrapTask.IsCompleted);

            Debug.Log("PreloadingStarted event was invoked");

            DateTimeOffset started = DateTimeOffset.UtcNow;
            long existingBlocks = _blocks?.Tip?.Index ?? 0;
            Debug.Log("Preloading starts");

            var swarmPreloadTask = Task.Run(async () =>
            {
                await _swarm.PreloadAsync(
                    TimeSpan.FromMilliseconds(SwarmDialTimeout),
                    null,
                    false,
                    cancellationToken: _cancellationTokenSource.Token
                );
            });

            yield return new WaitUntil(() => swarmPreloadTask.IsCompleted);
            DateTimeOffset ended = DateTimeOffset.UtcNow;

            if (swarmPreloadTask.Exception is Exception exc)
            {
                Debug.LogErrorFormat(
                    "Preloading terminated with an exception: {0}",
                    exc
                );
                throw exc;
            }

            var index = _blocks?.Tip?.Index ?? 0;
            Debug.LogFormat(
                "Preloading finished; elapsed time: {0}; blocks: {1}",
                ended - started,
                index - existingBlocks
            );

            var swarmStartTask = Task.Run(async () =>
            {
                try
                {
                    await _swarm.StartAsync();
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
            return _blocks.MakeTransaction(PrivateKey, polymorphicActions);
        }

        private IEnumerator CoMiner()
        {
            while (true)
            {
                var txs = new HashSet<Transaction<PolymorphicAction<ActionBase>>>();

                var task = Task.Run(async () =>
                {
                    var block = await _blocks.MineBlock(PrivateKey);

                    if (_swarm?.Running ?? false)
                    {
                        _swarm.BroadcastBlock(block);
                    }

                    return block;
                });
                yield return new WaitUntil(() => task.IsCompleted);

                if (!task.IsCanceled && !task.IsFaulted)
                {
                    var block = task.Result;
                    Debug.Log($"created block index: {block.Index}, difficulty: {block.Difficulty}");
                }
                else
                {
                    var invalidTxs = txs;
                    var retryActions = new HashSet<IImmutableList<PolymorphicAction<ActionBase>>>();

                    if (task.IsFaulted)
                    {
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            if (ex is InvalidTxNonceException invalidTxNonceException)
                            {
                                var invalidNonceTx = _store.GetTransaction<PolymorphicAction<ActionBase>>(invalidTxNonceException.TxId);

                                if (invalidNonceTx.Signer == Address)
                                {
                                    Debug.Log($"Tx[{invalidTxNonceException.TxId}] nonce is invalid. Retry it.");
                                    retryActions.Add(invalidNonceTx.Actions);
                                }
                            }

                            if (ex is InvalidTxException invalidTxException)
                            {
                                Debug.Log($"Tx[{invalidTxException.TxId}] is invalid. mark to unstage.");
                                invalidTxs.Add(_store.GetTransaction<PolymorphicAction<ActionBase>>(invalidTxException.TxId));
                            }

                            Debug.LogException(ex);
                        }
                    }

                    foreach (var invalidTx in invalidTxs)
                    {
                        _blocks.UnstageTransaction(invalidTx);
                    }

                    foreach (var retryAction in retryActions)
                    {
                        MakeTransaction(retryAction, true);
                    }
                }
            }
        }
    }
}
