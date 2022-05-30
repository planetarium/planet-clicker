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

            public bool noMiner;

            public string[] peers = new string[]{ };

            public string storagePath;

            public string appProtocolVersion;

            public string[] trustedAppProtocolVersionSigners = new string[] { };

            [Option("logging", Required = false, HelpText = "Turn on Libplanet logging.")]
            public bool Logging { get => logging; set => logging = value; }

            [Option("no-miner", Required = false, HelpText = "Do not mine block.")]
            public bool NoMiner { get => noMiner; set => noMiner = value; }

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
