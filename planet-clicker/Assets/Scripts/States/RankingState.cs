using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Libplanet;
using Libplanet.Store;

namespace Scripts.States
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

    public class RankingState : DataModel
    {
        // Fields are ignored when encoding.
        public static readonly Address Address = new Address(new byte[]
            {
                0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1
            }
        );

        public ImmutableDictionary<Address, long> Map { get; private set; }

        public RankingState()
            : base()
        {
            Map = ImmutableDictionary<Address, long>.Empty;
        }

        // NOTE: Just to make a point. A pair of string and long works fine,
        // but so should an IKey and IValue pair, which in my opinion,
        // should take precedence over a string and long pair.
        // Also, Add() should return Bencodex.Types.Dictionary not
        // an ImmutableDictionary<K, V>.
        public RankingState(Bencodex.Types.Dictionary encoded)
            : base((Bencodex.Types.Dictionary)Bencodex.Types.Dictionary.Empty.Add(
                (Bencodex.Types.IKey)new Bencodex.Types.Text(nameof(Map)),
                (Bencodex.Types.IValue)encoded))
        {
        }

        public new Bencodex.Types.IValue Encode() => base.Encode()[nameof(Map)];

        public void Update(Address address, long count)
        {
            Map = Map.SetItem(address, count);
        }

        public IEnumerable<RankingInfo> GetRanking()
        {
            return Map
                .Select(kv => new RankingInfo(kv.Key, kv.Value))
                .OrderByDescending(info => info.Count);
        }
    }
}
