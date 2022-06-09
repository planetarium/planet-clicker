using System.IO;
using UnityEngine;

namespace LibplanetUnity
{
    public static class Paths
    {
        public static readonly string GenesisBlockPath =
            Path.Combine(Application.streamingAssetsPath, "genesis");

        public static readonly string SwarmConfigPath =
            Path.Combine(Application.streamingAssetsPath, "swarm_config.json");

        public static readonly string StorePath =
            Path.Combine(Application.persistentDataPath, "store");

        public static readonly string PrivateKeyPath =
            Path.Combine(Application.persistentDataPath, "private_key");
    }
}
