using Bencodex.Types;
using Libplanet.Store;

namespace _Script.Action
{
    public class AddCountData : DataModel
    {
        // NOTE: Explicitly set to lower case for compatibility.
        public long count { get; private set; }

        public AddCountData(long c)
        {
            count = c;
        }

        public AddCountData(Bencodex.Types.Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
