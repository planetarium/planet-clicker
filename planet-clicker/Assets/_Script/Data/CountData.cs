using Bencodex.Types;
using Libplanet.Store;

namespace _Script.Data
{
    public class CountData : DataModel
    {
        // NOTE: Explicitly set to lower case for compatibility.
        public long count { get; private set; }

        public CountData(long c)
        {
            count = c;
        }

        public void UpdateCount(long count)
        {
            this.count = count;
        }

        public CountData(Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
