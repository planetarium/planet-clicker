using System.IO;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using LibplanetUnity.Action;
using UnityEditor;
using UnityEngine;

namespace LibplanetUnity.Editor
{
    public static class LibplanetEditor
    {
        [MenuItem("Tools/Libplanet/Delete All(Editor)")]
        public static void DeleteAllEditor()
        {
            var path = Path.Combine(Application.persistentDataPath, "planetarium_dev.ldb");
            DeleteAll(path);
        }
        
        [MenuItem("Tools/Libplanet/Delete All(Player)")]
        public static void DeleteAllPlayer()
        {
            var path = Path.Combine(Application.persistentDataPath, "planetarium.ldb");
            DeleteAll(path);
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
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                return;
            }
            info.Delete();
        }
    }   
}
