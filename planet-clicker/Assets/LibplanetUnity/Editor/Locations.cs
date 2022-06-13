using System.IO;
using UnityEditor;
using Libplanet.Unity;

namespace LibplanetUnity.Editor
{
    public static class Locations
    {
        [MenuItem("Tools/Libplanet/Locations/Private key")]
        public static void OpenPrivateKeyLocation()
        {
            string path = Paths.PrivateKeyPath;
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    "Open private key location",
                    $"Private key file not found at {path}. " +
                    "Please create a private key first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Locations/Blockchain")]
        public static void OpenBlockchainLocation()
        {
            string path = Paths.StorePath;
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                EditorUtility.DisplayDialog(
                    "Open blockchain location",
                    $"Blockchain directory not found at {path}. " +
                    "Please create a blockchain first by running a node.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Locations/Genesis block")]
        public static void OpenGenesisBlockLocation()
        {
            string path = Paths.GenesisBlockPath;
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    "Open genesis block location",
                    $"Genesis block file not found at {path}. " +
                    "Please create a genesis block first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Locations/Swarm config")]
        public static void OpenSwarmConfigLocation()
        {
            string path = Paths.SwarmConfigPath;
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    "Open genesis block location",
                    $"Swarm config file not found at {path}. " +
                    "Please create a swarm config first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }
    }
}
