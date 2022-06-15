using System;
using _Script.Data;
using Bencodex.Types;

namespace _Script.State
{
    [Serializable]
    public class CountState
    {
        public CountData Count;

        public CountState(CountData count)
        { 
            Count = count;
        }

        public CountState(Dictionary encoded)
        {
            Count = new CountData(encoded);
        }

        public void SumCount(long count)
        {
            Count.UpdateCount(Count.count + count);
        }

        public IValue Serialize()
        {
            return Count.Encode();
        }
    }
}
