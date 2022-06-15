using Bencodex.Types;
using Libplanet.Store;

namespace _Script.Data
{
    public class CountData : DataModel
    {
        public long count { get; private set; }

        public CountData(long c)
        {
            count = c;
        }

        public CountData(IValue encoded)
            : base((Bencodex.Types.Dictionary)encoded)
        {
        }
    }
}
