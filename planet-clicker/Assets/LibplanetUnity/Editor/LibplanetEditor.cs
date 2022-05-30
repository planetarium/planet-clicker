using System.IO;
using UnityEditor;

namespace LibplanetUnity.Editor
{
    public static class LibplanetEditor
    {
        [MenuItem("Tools/Libplanet/Delete blockchain")]
        public static void DeleteChain()
        {
            var dir = new DirectoryInfo(Agent.DefaultStoragePath);
            if (dir.Exists)
            {
                if (EditorUtility.DisplayDialog(
                    "Delete blockchain",
                    $"Blockchain found at {dir}.\n" +
                    $"Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {
                    dir.Delete(recursive: true);
                    EditorUtility.DisplayDialog("Delete blockchain", "Blockchain deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Delete blockchain", "Blockchain not found.", "Close");
            }
        }

        [MenuItem("Tools/Libplanet/Create swarm config")]
        public static void CreateSwarmConfig()
        {
            const string title = "Create swarm config";
            if (File.Exists(Agent.SwarmConfigPath) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Swarm config found at {Agent.SwarmConfigPath}.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Agent.CreateSwarmConfig();
            EditorUtility.DisplayDialog(title, "New swarm config created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Delete swarm config")]
        public static void DeleteSwarmConfig()
        {
            const string title = "Delete swarm config";
            if (File.Exists(Agent.SwarmConfigPath))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Swarm config found at {Agent.SwarmConfigPath}.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {

                    File.Delete(Agent.SwarmConfigPath);
                    EditorUtility.DisplayDialog(title, "Swarm config deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Genesis block not found.", "Close");
            }
        }

        [MenuItem("Tools/Libplanet/Create genesis block")]
        public static void CreateGenesisBlock()
        {
            const string title = "Create genesis block";
            if (File.Exists(Agent.GenesisBlockPath) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Genesis block found at {Agent.GenesisBlockPath}.\n" +
                    "New genesis block will not be compatible with existing chain.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Agent.CreateGenesisBlock();
            EditorUtility.DisplayDialog(title, "New genesis block created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Delete genesis block")]
        public static void DeleteGenesisBlock()
        {
            const string title = "Delete genesis block";
            if (File.Exists(Agent.GenesisBlockPath))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Genesis block found at {Agent.GenesisBlockPath}.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {

                    File.Delete(Agent.GenesisBlockPath);
                    EditorUtility.DisplayDialog(title, "Genesis block deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Genesis block not found.", "Close");
            }
        }

        [MenuItem("Tools/Libplanet/Create private key")]
        public static void CreatePrivateKey()
        {
            const string title = "Create private key";
            if (File.Exists(Agent.DefaultPrivateKeyPath) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Private key found at {Agent.DefaultPrivateKeyPath}.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Agent.CreatePrivateKey();
            EditorUtility.DisplayDialog(title, "New private key created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Delete private key")]
        public static void DeletePrivateKey()
        {
            const string title = "Delete private key";
            if (File.Exists(Agent.DefaultPrivateKeyPath))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Are you sure you want to delete private key found at {Agent.DefaultPrivateKeyPath}?",
                    "Ok",
                    "Cancel"))
                {
                    File.Delete(Agent.DefaultPrivateKeyPath);
                    EditorUtility.DisplayDialog(title, "Private key deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Private key not found.", "Close");
            }
        }

        private static void DeleteAll(string path)
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                dir.Delete(recursive: true);
            }
        }
    }
}
