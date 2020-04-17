using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LibplanetUnity.Helper
{
    [Serializable]
        public class Options
        {
            public bool logging;

            public string privateKey;

            public string host;

            public int port;

            public bool noMiner;

            public string[] peers = new string[]{ };

            public string[] iceServers = new string[]{ };

            public string storagePath;

            public string appProtocolVersion;

            public string[] trustedAppProtocolVersionSigners = new string[] { };

            [Option("logging", Required = false, HelpText = "Turn on Libplanet logging.")]
            public bool Logging { get => logging; set => logging = value; }

            [Option("private-key", Required = false, HelpText = "The private key to use.")]
            public string PrivateKey { get => privateKey; set => privateKey = value; }

            [Option("host", Required = false, HelpText = "The host name to use.")]
            public string Host { get => host; set => host = value; }

            [Option("port", Required = false, HelpText = "The source port to use.")]
            public int? Port { get => port == 0 ? default(int?) : port; set => port = value ?? 0; }

            [Option("no-miner", Required = false, HelpText = "Do not mine block.")]
            public bool NoMiner { get => noMiner; set => noMiner = value; }

            [Option("peer", Required = false, HelpText = "Peers to add. (Usage: --peer peerA peerB ...)")]
            public IEnumerable<string> Peers { get => peers; set => peers = value.ToArray(); }

            [Option("ice-servers", Required = false, HelpText = "STUN/TURN servers to use. (Usage: --ice-servers serverA serverB ...)")]
            public IEnumerable<string> IceServers
            {
                get => iceServers;
                set
                {
                    iceServers = value.ToArray();
                }
            }

            [Option("storage-path", Required = false, HelpText = "The path to store game data.")]
            public string StoragePath { get => storagePath; set => storagePath = value; }

            [Option("app-protocol-version", Required = false, HelpText = "App protocol version token.")]
            public string AppProtocolVersion { get => appProtocolVersion; set => appProtocolVersion = value; }

            [Option("trusted-app-protocol-version-signer",
                Required = false,
                HelpText = "Trustworthy signers who claim new app protocol versions")]
            public IEnumerable<string> TrustedAppProtocolVersionSigners
            {
                get => trustedAppProtocolVersionSigners;
                set => trustedAppProtocolVersionSigners = value.ToArray();
            }
        }

    public static class CommnadLineParser
    {
        public static Options GetCommandLineOptions()
        {
            string[] args = Environment.GetCommandLineArgs();

            var parser = new Parser(with => with.IgnoreUnknownArguments = true);

            ParserResult<Options> result = parser.ParseArguments<Options>(args);

            if (result.Tag == ParserResultType.Parsed)
            {
                return ((Parsed<Options>) result).Value;
            }

            result.WithNotParsed(
                errors =>
                    Debug.Log(HelpText.AutoBuild(result)
                    ));

            return null;
        }
    }
}
