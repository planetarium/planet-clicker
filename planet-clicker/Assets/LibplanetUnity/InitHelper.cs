using System.IO;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Node;
using Libplanet.Unity;
using Libplanet.Store;

namespace LibplanetUnity
{
    public static class InitHelper
    {
        public static SwarmConfig GetSwarmConfig(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"File not found for {nameof(SwarmConfig)}.", path);
            }

            return SwarmConfig.FromJson(File.ReadAllText(path));
        }

        public static Block<PolymorphicAction<ActionBase>> GetGenesisBlock(string path)
        {
            if (!File.Exists(Paths.GenesisBlockPath))
            {
                throw new FileNotFoundException(
                    $"File not found for genesis block.", path);
            }

            return NodeUtils<PolymorphicAction<ActionBase>>.LoadGenesisBlock(path);
        }

        public static PrivateKey GetPrivateKey(string path)
        {
            if (!File.Exists(path))
            {
                Utils.CreatePrivateKey(path);
            }

            return NodeUtils<PolymorphicAction<ActionBase>>.LoadPrivateKey(path);
        }

        public static (IStore store, IStateStore stateStore) GetStore(string path)
        {
            return NodeUtils<PolymorphicAction<ActionBase>>.LoadStore(path);
        }
    }
}
