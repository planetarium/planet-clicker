using System.IO;
using UnityEditor;

namespace Libplanet.Unity.Editor
{
    public static class BlockChainMenu
    {
        [MenuItem("Tools/Libplanet/Blockchain/Open blockchain directory location")]
        public static void OpenBlockchainLocation()
        {
            const string title = "Open blockchain directory location";
            string path = Paths.StorePath;

            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"Blockchain directory not found at {path}. " +
                    "Please create a blockchain first by running a node.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Blockchain/Delete blockchain")]
        public static void DeleteBlockChain()
        {
            const string title = "Delete blockchain";
            DirectoryInfo directory = new DirectoryInfo(Paths.StorePath);
            if (directory.Exists)
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Blockchain found at {directory}.\n" +
                    "Local blockchain data will be removed.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {
                    directory.Delete(recursive: true);
                    EditorUtility.DisplayDialog(title, "Blockchain deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Blockchain not found.", "Close");
            }
        }
    }
}
