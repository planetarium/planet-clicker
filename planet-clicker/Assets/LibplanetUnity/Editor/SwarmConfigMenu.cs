using System.IO;
using UnityEditor;
using Libplanet.Unity;

namespace LibplanetUnity.Editor
{
    public static class SwarmConfigMenu
    {
        [MenuItem("Tools/Libplanet/Swarm config/Open file location")]
        public static void OpenSwarmConfigLocation()
        {
            const string title = "Open swarm config file location";
            string path = Paths.SwarmConfigPath;

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"Swarm config file not found at {path}. " +
                    "Please create a swarm config first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Swarm config/Create swarm config")]
        public static void CreateSwarmConfig()
        {
            const string title = "Create swarm config";
            string path = Paths.SwarmConfigPath;

            if (File.Exists(path) &&
                !EditorUtility.DisplayDialog(
                    title,
                    $"Swarm config found at {path}.\n" +
                    "Do you want to overwrite it?",
                    "Ok",
                    "Cancel"))
            {
                return;
            }

            Utils.CreateSwarmConfig(path);
            EditorUtility.DisplayDialog(title, "New swarm config created.", "Close");
        }

        [MenuItem("Tools/Libplanet/Swarm config/Delete swarm config")]
        public static void DeleteSwarmConfig()
        {
            const string title = "Delete swarm config";
            string path = Paths.SwarmConfigPath;

            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Swarm config found at {path}.\n" +
                    "A swarm config is required to run a blockchain node.\n" +
                    "Do you want to delete it?",
                    "Ok",
                    "Cancel"))
                {

                    File.Delete(path);
                    EditorUtility.DisplayDialog(title, "Swarm config deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Swarm config not found.", "Close");
            }
        }
    }
}
