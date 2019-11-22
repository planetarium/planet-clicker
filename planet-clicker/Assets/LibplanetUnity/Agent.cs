using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Libplanet;
using Libplanet.Crypto;
using Libplanet.Net;
using NetMQ;
using UnityEngine;
using LibplanetUnity.Helper;
using LibplanetUnity.Action;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Store;
using Libplanet.Tx;

namespace LibplanetUnity
{
    public class Agent : MonoSingleton<Agent>
    {
        private static readonly TimeSpan BlockInterval = TimeSpan.FromSeconds(10);

        private const int SwarmDialTimeout = 5000;

        private const int SwarmLinger = 1 * 1000;

        public const string PlayerPrefsKeyOfAgentPrivateKey = "private_key_agent";
#if UNITY_EDITOR
        private const string AgentStoreDirName = "planetarium_dev";
#else
        private const string AgentStoreDirName = "planetarium";
#endif

        private static readonly string CommandLineOptionsJsonPath = Path.Combine(Application.streamingAssetsPath, "clo.json");
        private const string PeersFileName = "peers.dat";
        private const string IceServersFileName = "ice_servers.dat";

        private static readonly string DefaultStoragePath =
            Path.Combine(Application.persistentDataPath, AgentStoreDirName);

        private static IEnumerator _miner;
        private static IEnumerator _swarmRunner;

        private ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();
        public long BlockIndex => _blocks?.Tip?.Index ?? 0;
        private PrivateKey PrivateKey { get; set; }
        public Address Address { get; set; }

        public event EventHandler BootstrapStarted;
        public event EventHandler PreloadStarted;
        public event EventHandler<PreloadState> PreloadProcessed;
        public event EventHandler PreloadEnded;

        public bool SyncSucceed { get; private set; }

        private BlockChain<PolymorphicAction<ActionBase>> _blocks;

        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        protected DefaultStore _store;

        private ImmutableList<Peer> _seedPeers;

        private IImmutableSet<Address> _trustedPeers;

        private CancellationTokenSource _cancellationTokenSource;

        public static void Initialize()
        {
            instance.InitAgent();
        }

        private void InitAgent()
        {
            var options = GetOptions(CommandLineOptionsJsonPath);
            var privateKey = GetPrivateKey(options);
            var peers = GetPeers(options);
            var iceServers = GetIceServers();
            var host = GetHost(options);
            int? port = options.Port;
            var storagePath = options.StoragePath ?? DefaultStoragePath;

            Init(privateKey, storagePath, peers, iceServers, host, port);

            _miner = options.NoMiner ? null : CoMiner();

            StartSystemCoroutines();
            StartNullableCoroutine(_miner);
        }

        public void Init(PrivateKey privateKey, string path, IEnumerable<Peer> peers,
            IEnumerable<IceServer> iceServers, string host, int? port)
        {
            Debug.Log(path);
            var policy = GetPolicy();
            PrivateKey = privateKey;
            Address = privateKey.PublicKey.ToAddress();
            _store = new DefaultStore(path, flush: false);
            _blocks = new BlockChain<PolymorphicAction<ActionBase>>(policy, _store);
            _swarm = new Swarm<PolymorphicAction<ActionBase>>(
                _blocks,
                privateKey,
                appProtocolVersion: 1,
                host: host,
                listenPort: port,
                iceServers: iceServers,
                differentVersionPeerEncountered: DifferentAppProtocolVersionPeerEncountered);

            _seedPeers = peers.Where(peer => peer.PublicKey != privateKey.PublicKey).ToImmutableList();
            // Init SyncSucceed
            SyncSucceed = true;

            // FIXME: Trusted peers should be configurable
            _trustedPeers = _seedPeers.Select(peer => peer.Address).ToImmutableHashSet();
            _cancellationTokenSource = new CancellationTokenSource();

        }

        public static Options GetOptions(string jsonPath)
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

        public IValue GetState(Address address)
        {
            AddressStateMap states = _blocks.GetState(address);
            states.TryGetValue(address, out var value);
            return value;
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

        private static IEnumerable<Peer> GetPeers(Options options)
        {
            return options.Peers?.Any() ?? false
                ? options.Peers.Select(LoadPeer)
                : LoadConfigLines(PeersFileName).Select(LoadPeer);
        }

        private static IEnumerable<IceServer> GetIceServers()
        {
            return LoadIceServers();
        }

        private static string GetHost(Options options)
        {
            return options.Host;
        }

        private static BoundPeer LoadPeer(string peerInfo)
        {
            string[] tokens = peerInfo.Split(',');
            var pubKey = new PublicKey(ByteUtil.ParseHex(tokens[0]));
            string host = tokens[1];
            int port = int.Parse(tokens[2]);

            return new BoundPeer(pubKey, new DnsEndPoint(host, port), 0);
        }

        private static IEnumerable<string> LoadConfigLines(string fileName)
        {
            string userPath = Path.Combine(
                Application.persistentDataPath,
                fileName
            );
            string content;
            
            if (File.Exists(userPath))
            {
                content = File.ReadAllText(userPath);
            }
            else 
            {
                string assetName = Path.GetFileNameWithoutExtension(fileName);
                content = Resources.Load<TextAsset>($"Config/{assetName}").text;
            }

            foreach (var line in Regex.Split(content, "\n|\r|\r\n"))
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    yield return line;
                }
            }
        }

        private static IEnumerable<IceServer> LoadIceServers()
        {
            foreach (string line in LoadConfigLines(IceServersFileName)) 
            {
                var uri = new Uri(line);
                string[] userInfo = uri.UserInfo.Split(':');

                yield return new IceServer(new[] {uri}, userInfo[0], userInfo[1]);
            }
        }

        #region Mono

        protected override void OnDestroy()
        {
            NetMQConfig.Cleanup(false);

            base.OnDestroy();
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

        private IBlockPolicy<PolymorphicAction<ActionBase>> GetPolicy()
        {
# if UNITY_EDITOR
            return new DebugPolicy();
# else
            return new BlockPolicy<PolymorphicAction<ActionBase>>(
                null,
                BlockInterval,
                100000,
                2048
            );
#endif
        }

        public IEnumerator CoSwarmRunner()
        {
            BootstrapStarted?.Invoke(this, null);
            var bootstrapTask = Task.Run(async () =>
            {
                try
                {
                    await _swarm.BootstrapAsync(
                        _seedPeers,
                        5000,
                        5000,
                        _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Debug.LogFormat("Exception occurred during bootstrap {0}", e);
                }
            });

            yield return new WaitUntil(() => bootstrapTask.IsCompleted);

            PreloadStarted?.Invoke(this, null);
            Debug.Log("PreloadingStarted event was invoked");

            DateTimeOffset started = DateTimeOffset.UtcNow;
            long existingBlocks = _blocks?.Tip?.Index ?? 0;
            Debug.Log("Preloading starts");

            var swarmPreloadTask = Task.Run(async () =>
            {
                await _swarm.PreloadAsync(
                    TimeSpan.FromMilliseconds(SwarmDialTimeout),
                    new Progress<PreloadState>(state =>
                        PreloadProcessed?.Invoke(this, state)
                    ),
                    trustedStateValidators: _trustedPeers,
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


            PreloadEnded?.Invoke(this, null);

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

        private void DifferentAppProtocolVersionPeerEncountered(object sender, DifferentProtocolVersionEventArgs e)
        {
            Debug.LogWarningFormat("Different Version Encountered Expected: {0} Actual : {1}",
                e.ExpectedVersion, e.ActualVersion);
            SyncSucceed = false;
        }

        private IEnumerator CoProcessActions()
        {
            while(true)
            {
                if (_actions.TryDequeue(out System.Action action))
                {
                    action();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        public static bool WantsToQuit()
        {
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        public void MakeTransaction(IEnumerable<ActionBase> gameActions)
        {
            var actions = gameActions.Select(gameAction => (PolymorphicAction<ActionBase>) gameAction).ToList();
            Task.Run(() => MakeTransaction(actions, true));
        }

        private Transaction<PolymorphicAction<ActionBase>> MakeTransaction(
                    IEnumerable<PolymorphicAction<ActionBase>> actions, bool broadcast)
        {
            var polymorphicActions = actions.ToArray();
            Debug.LogFormat("Make Transaction with Actions: `{0}`",
                string.Join(",", polymorphicActions.Select(i => i.InnerAction)));
            return _blocks.MakeTransaction(PrivateKey, polymorphicActions);
        }

        public IEnumerator CoMiner()
        {
            while (true)
            {
                var txs = new HashSet<Transaction<PolymorphicAction<ActionBase>>>();

                var task = Task.Run(async () =>
                {
                    var block = await _blocks.MineBlock(Address);

                    if (_swarm?.Running ?? false)
                    {
                        _swarm.BroadcastBlocks(new[] {block});
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
                    _blocks.UnstageTransactions(invalidTxs);

                    foreach (var retryAction in retryActions)
                    {
                        MakeTransaction(retryAction, true);
                    }
                }
            }
        }
    }
}
