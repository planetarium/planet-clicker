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

        [MenuItem("Tools/Libplanet/Create genesis")]
        public static void CreateGenesisBlock()
        {
            if (File.Exists(Agent.GenesisBlockPath) &&
                !EditorUtility.DisplayDialog(
                    "Create genesis",
                    $"Genesis block found at {Agent.GenesisBlockPath}.\n" +
                    "New genesis block will not be compatible with existing chain.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Agent.CreateGenesisBlock();
            EditorUtility.DisplayDialog("Create genesis", "New genesis block created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Delete genesis")]
        public static void DeleteGenesisBlock()
        {
            if (File.Exists(Agent.GenesisBlockPath))
            {
                if (EditorUtility.DisplayDialog(
                    "Delete genesis",
                    $"Genesis block found at {Agent.GenesisBlockPath}.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {

                    File.Delete(Agent.GenesisBlockPath);
                    EditorUtility.DisplayDialog("Delete genesis", "Genesis block deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Delete genesis", "Genesis block not found.", "Close");
            }
        }

        [MenuItem("Tools/Libplanet/Create private key")]
        public static void CreatePrivateKey()
        {
            if (File.Exists(Agent.DefaultPrivateKeyPath) &&
                !EditorUtility.DisplayDialog(
                    "Create private key",
                    $"Private key found at {Agent.DefaultPrivateKeyPath}.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Agent.CreatePrivateKey();
            EditorUtility.DisplayDialog("Create private key", "New private key created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Delete private key")]
        public static void DeletePrivateKey()
        {
            if (File.Exists(Agent.DefaultPrivateKeyPath))
            {
                if (EditorUtility.DisplayDialog(
                    "Delete private key",
                    $"Are you sure you want to delete private key found at {Agent.DefaultPrivateKeyPath}?",
                    "Ok",
                    "Cancel"))
                {
                    File.Delete(Agent.DefaultPrivateKeyPath);
                    EditorUtility.DisplayDialog("Delete private key", "Private key deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Delete private key", "Private key not found.", "Close");
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
