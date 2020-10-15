using System;
using _Script.State;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using LibplanetUnity;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("sub_count")]
    public class SubCount : ActionBase
    {
        private Address _address;
        private long _count;

        public SubCount()
        {
        }

        public SubCount(Address address, long count)
        {
            _address = address;
            _count = count;
        }

        public override IValue PlainValue =>
            Bencodex.Types.Dictionary.Empty
            .SetItem("count", _count)
            .SetItem("address", _address.ToByteArray());

        public override void LoadPlainValue(IValue plainValue)
        {
            var serialized = (Bencodex.Types.Dictionary)plainValue;
            _count = (long)((Integer)serialized["count"]).Value;
            _address = new Address(((Bencodex.Types.Binary)serialized["address"]).Value);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;

            if (ctx.Rehearsal)
            {
                states = states.SetState(rankingAddress, MarkChanged);
                return states.SetState(_address, MarkChanged);
            }

            states.TryGetState(_address, out Bencodex.Types.Integer currentCount);
            var nextCount = Math.Max(currentCount - _count, 0);

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState;
            rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary bdict)
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(_address, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(_address, (Bencodex.Types.Integer)nextCount);
        }
    }
}
