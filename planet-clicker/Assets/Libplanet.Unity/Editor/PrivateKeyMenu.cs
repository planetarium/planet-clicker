using System.IO;
using UnityEditor;

namespace Libplanet.Unity.Editor
{
    public static class PrivateKeyMenu
    {
        [MenuItem("Tools/Libplanet/Private key/Open file location")]
        public static void OpenPrivateKeyLocation()
        {
            const string title = "Open private key file location";
            string path = Paths.PrivateKeyPath;

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"Private key file not found at {path}. " +
                    "Please create a private key first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Private key/Create private key")]
        public static void CreatePrivateKey()
        {
            const string title = "Create private key";
            string path = Paths.PrivateKeyPath;

            if (File.Exists(path) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Private key found at {path}.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Utils.CreatePrivateKey(path);
            EditorUtility.DisplayDialog(title, "New private key created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Private key/Delete private key")]
        public static void DeletePrivateKey()
        {
            const string title = "Delete private key";
            string path = Paths.PrivateKeyPath;

            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Are you sure you want to delete private key found at {path}?",
                    "Ok",
                    "Cancel"))
                {
                    File.Delete(path);
                    EditorUtility.DisplayDialog(title, "Private key deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Private key not found.", "Close");
            }
        }
    }
}
