using System.IO;
using UnityEditor;

namespace LibplanetUnity.Editor
{
    public static class LibplanetEditor
    {
        [MenuItem("Tools/Libplanet/Delete chain")]
        public static void DeleteChain()
        {
            DeleteAll(Agent.DefaultStoragePath);
        }

        [MenuItem("Tools/Libplanet/Create genesis")]
        public static void CreateGenesisBlock()
        {
            if (
                File.Exists(Agent.GenesisBlockPath) &&
                !EditorUtility.DisplayDialog(
                    "Create genesis",
                    "Previous genesis block has been found. Do you want to overwrite it?",
                    "Ok",
                    "Cancel"
                )
            )
            {
                return;
            }

            Agent.CreateGenesisBlock();
            EditorUtility.DisplayDialog("Create genesis", "New genesis block was created.", "Close");
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
