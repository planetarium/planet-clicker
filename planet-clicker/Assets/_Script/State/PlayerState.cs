using System;
using System.Text;
using System.Collections.Generic;
using Bencodex.Types;
using Libplanet;

namespace _Script.State
{
    [Serializable]
    public class PlayerState : State
    {
        public long Count;

        public PlayerState(Address address) : base(address)
        {
            Count = 0;
        }

        public PlayerState(Address address, long count) : base(address)
        {
            Count = count;
        }

        public PlayerState(Address address, Bencodex.Types.Dictionary bdict) : base(address)
        {
            bdict.TryGetValue(new Bencodex.Types.Binary("count", Encoding.UTF8), out var count);
            Count = (Bencodex.Types.Integer)count;
        }

        public IValue Serialize()
        {
            return new Bencodex.Types.Dictionary(
                new Dictionary<IKey, IValue>()
                {
                    {new Bencodex.Types.Binary("count", Encoding.UTF8), new Bencodex.Types.Integer(Count)},
                }
            );
        }

        public void SumCount(int value)
        {
            Count = Count + value;
        }

        public void SubCount(int value)
        {
            Count = Math.Max(Count - value, 0);
        }
    }
}