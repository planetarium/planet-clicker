using Bencodex.Types;
using Libplanet;
using Libplanet.Store;

namespace _Script.Action
{
    public class SubCountData : DataModel
    {
        // NOTE: Explicitly set to lower case for compatibility.
        public Address address { get; private set; }
        public long count { get; private set; }

        public SubCountData(Address a, long c)
        {
            address = a;
            count = c;
        }

        public SubCountData(Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
