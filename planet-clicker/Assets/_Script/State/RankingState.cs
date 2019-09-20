using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
