using System;
using Bencodex.Types;
using Libplanet.Store;

namespace _Script.State
{
    public class CountState : DataModel
    {
        public long Count { get; private set; }

        public CountState(long count)
            : base()
        {
            Count = count;
        }

        public CountState(Bencodex.Types.Dictionary encoded)
            : base(encoded)
        {
        }

        public new IValue Encode() => base.Encode();

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
