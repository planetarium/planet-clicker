using System.IO;
using UnityEditor;
using Libplanet.Unity;

namespace LibplanetUnity.Editor
{
    public static class GenesisBlockMenu
    {
        [MenuItem("Tools/Libplanet/Genesis Block/Open file location")]
        public static void OpenGenesisBlockLocation()
        {
            const string title = "Open genesis block file location";
            string path = Paths.GenesisBlockPath;

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"Genesis block file not found at {path}. " +
                    "Please create a genesis block first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Genesis Block/Create genesis block")]
        public static void CreateGenesisBlock()
        {
            const string title = "Create genesis block";
            string path = Paths.GenesisBlockPath;

            if (File.Exists(path) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Genesis block found at {path}.\n" +
                    "New genesis block will not be compatible with existing chain.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            // TODO: Allow creating a genesis block with transactions.
            Utils.CreateGenesisBlock(path);
            EditorUtility.DisplayDialog(title, "New genesis block created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Genesis Block/Delete genesis block")]
        public static void DeleteGenesisBlock()
        {
            const string title = "Delete genesis block";
            string path = Paths.GenesisBlockPath;

            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Genesis block found at {path}.\n" +
                    "A genesis block is required to run a blockchain node.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {

                    File.Delete(path);
                    EditorUtility.DisplayDialog(title, "Genesis block deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Genesis block not found.", "Close");
            }
        }
    }
}
