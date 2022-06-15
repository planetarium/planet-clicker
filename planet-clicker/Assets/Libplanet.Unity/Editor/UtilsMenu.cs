using System;
using UnityEditor;
using UnityEngine;
using Libplanet.Crypto;

namespace Libplanet.Unity.Editor
{
    public class GenerateBoundPeerStringWindow : EditorWindow
    {
        string privateKeyString = "";
        string host = "";
        int port = 0;
        string boundPeerString = "";

        [MenuItem("Tools/Libplanet/Utils/Generate bound peer string")]
        static void Init()
        {
            const string title = "Generate bound peer string";

            var window = EditorWindow.GetWindowWithRect(
                typeof(GenerateBoundPeerStringWindow),
                new Rect(0, 0, 800, 200),
                true,
                title);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Bound peer information", EditorStyles.boldLabel);
            privateKeyString = EditorGUILayout.TextField("Private key string", privateKeyString);
            host = EditorGUILayout.TextField("Host", host);
            port = EditorGUILayout.IntField("Port", port);

            // Zero port is excluded.
            if (port < 1 || port > 65535)
            {
                boundPeerString = "Invalid port number";
            }
            else if (host.Length < 1)
            {
                boundPeerString = "Invalid host";
            }
            else
            {
                try
                {
                    PrivateKey privateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyString));
                    string publicKeyString = ByteUtil.Hex(privateKey.PublicKey.Format(true));
                    boundPeerString = $"{publicKeyString},{host},{port}";
                }
                catch (Exception)
                {
                    boundPeerString = "Invalid private key string";
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Generated bound peer string", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(boundPeerString);
        }
    }
}
