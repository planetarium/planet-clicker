using Libplanet.Store;
using Bencodex.Types;
using _Script.Data;

namespace _Script.State
{
    public class CountState : DataModel
    {
        public long Count { get; private set; }

        public CountState()
            : base()
        {
            Count = 0;
        }

        public CountState(long count)
            : base()
        {
            Count = count;
        }

        public CountState(IValue encoded)
            : base((Bencodex.Types.Dictionary)encoded)
        {
        }

        public new IValue Encode() => base.Encode();

        public CountState Update(CountData data)
        {
            return new CountState(Count + data.count);
        }
    }
}
