using System;
using Libplanet.Store;

namespace Scripts.States
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
