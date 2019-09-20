using System;
using System.Collections.Generic;
using System.Linq;

namespace _Script.Helper
{
    [Serializable]
    public class Options
    {
        // JSON 직렬화를 위해 필드와 속성을 둘 다 기술합니다.
        public string privateKey;

        public string host;

        public int port;

        public bool noMiner;

        public string[] peers = new string[]{ };

        public string storagePath;

        public bool autoPlay;

        public string PrivateKey { get => privateKey; set => privateKey = value; }

        public string Host { get => host; set => host = value; }

        public int? Port { get => port == 0 ? default(int?) : port; set => port = value ?? 0; }

        public bool NoMiner { get => noMiner; set => noMiner = value; }

        public IEnumerable<string> Peers { get => peers; set => peers = value.ToArray(); }

        public string StoragePath { get => storagePath; set => storagePath = value; }
    }
}
