using System;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Libplanet;

namespace _Script.State
{
    public class RankingInfo
    {
        public Address Address;
        public long Count;

        public RankingInfo(Address address, long count)
        {
            Address = address;
            Count = count;
        }
    }

    [Serializable]
    public class RankingState : State
    {
        public static readonly Address Address = new Address(new byte[]
            {
                0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1
            }
        );

        private readonly Dictionary<Address, long> _map;

        public RankingState() : base(Address)
        {
            _map = new Dictionary<Address, long>();
        }

        public RankingState(Bencodex.Types.Dictionary bdict) : base(Address)
        {
            _map = bdict.ToDictionary(
                pair => new Address((Bencodex.Types.Binary)pair.Key),
                pair => (long)(Bencodex.Types.Integer)pair.Value
            );
        }

        public void Update(Address address, long count)
        {
            _map[address] = count;
        }

        public IEnumerable<RankingInfo> GetRanking()
        {
            return _map
                .Select(pair => new RankingInfo(pair.Key, pair.Value))
                .OrderByDescending(info => info.Count);
        }

        public IValue Serialize()
        {
            return new Bencodex.Types.Dictionary(
                _map.Select(pair => new KeyValuePair<IKey, IValue>(
                    new Bencodex.Types.Binary(pair.Key.ToByteArray()),
                    new Bencodex.Types.Integer(pair.Value)
                ))
            );
        }
    }
}
