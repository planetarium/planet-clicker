using System;
using Bencodex.Types;
using Libplanet.Store;

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

        public CountState(IValue encoded)
            : base(Dictionary.Empty.Add(nameof(Count), encoded))
        {
        }

        public new IValue Encode() => base.Encode()[nameof(Count)];

        public void AddCount(long count)
        {
            Count = Count + count;
        }

        public void SubCount(long count)
        {
            Count = Math.Max(0, Count - count);
        }
    }
}
