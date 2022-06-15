using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Libplanet.Crypto;

namespace Libplanet.Unity.Editor
{
    public static class PrivateKeyMenu
    {
        [MenuItem("Tools/Libplanet/Private key/Open private key file location")]
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

        [MenuItem("Tools/Libplanet/Private key/Open temp private keys file location")]
        public static void OpenTempPrivateKeysLocation()
        {
            const string title = "Open temp private keys file location";
            string path = Paths.TempPrivateKeysPath;

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"Temp private keys file not found at {path}. " +
                    "Please create a temp private key first.",
                    "Close");
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("Tools/Libplanet/Private key/Delete temp private keys")]
        public static void DeleteTempPrivateKeys()
        {
            const string title = "Delete private key";
            string path = Paths.TempPrivateKeysPath;

            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog(
                    title,
                    $"Are you sure you want to delete all temp private keys found at {path}?",
                    "Ok",
                    "Cancel"))
                {
                    File.Delete(path);
                    EditorUtility.DisplayDialog(title, "Temp private keys deleted.", "Close");
                }
            }
            else
            {
                EditorUtility.DisplayDialog(title, "Temp private keys not found.", "Close");
            }
        }
    }

    public class CreateTempPrivateKeyWindow : EditorWindow
    {
        public string privateKeyString = "";

        public string memo = "";

        public string saveText = "";

        public bool valid = false;

        [MenuItem("Tools/Libplanet/Private key/Create temp private key")]
        public static void Init()
        {
            const string title = "Create temp private key";

            EditorWindow window = EditorWindow.GetWindowWithRect(
                typeof(CreateTempPrivateKeyWindow),
                new Rect(0, 0, 800, 200),
                true,
                title);
            window.Show();
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("Private key information", EditorStyles.boldLabel);
            privateKeyString = EditorGUILayout.TextField("Private key string", privateKeyString);
            memo = EditorGUILayout.TextField("Memo", memo);

            if (GUILayout.Button("Generate random private key"))
            {
                privateKeyString = ByteUtil.Hex(new PrivateKey().ByteArray);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Private key with memo", EditorStyles.boldLabel);
            (saveText, valid) = GetSaveText();
            EditorGUILayout.SelectableLabel(saveText);

            if (GUILayout.Button("Save"))
            {
                if (valid)
                {
                    string path = Paths.TempPrivateKeysPath;
                    using (StreamWriter stream = new StreamWriter(path, true, Encoding.UTF8))
                    {
                        stream.WriteLine(saveText);
                    }
                    EditorUtility.DisplayDialog("Save dialog", $"Private key added to {path}.", "Close");
                }
                else
                {
                    EditorUtility.DisplayDialog("Save dialog", "Entry is not valid.", "Close");
                }
            }
        }

        private (string, bool) GetSaveText()
        {
            if (memo.Length > 0)
            {
                try
                {
                    PrivateKey _ = new PrivateKey(ByteUtil.ParseHex(privateKeyString));
                    return ($"{privateKeyString},{memo}", true);
                }
                catch (Exception)
                {
                    return ("Invalid private key string", false);
                }
            }
            else
            {
                return ("Please write a memo to identify the private key", false);
            }
        }
    }
}
