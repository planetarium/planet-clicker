using Libplanet;
using Libplanet.Store;

namespace _Script.Action
{
    /// <summary>
    /// <see cref="DataModel"/> for encoding <see cref="SubCount"/> action.
    /// </summary>
    public class SubCountPlainValue : DataModel
    {
        public Address Address { get; private set; }
        public long Count { get; private set; }

        public SubCountPlainValue(Address address, long count)
        {
            Address = address;
            Count = count;
        }

        public SubCountPlainValue(Bencodex.Types.Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
