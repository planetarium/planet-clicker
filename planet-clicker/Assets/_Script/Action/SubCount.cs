using System;
using _Script.State;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Unity;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("sub_count")]
    public class SubCount : ActionBase
    {
        private Address _address;
        private long _count;
        private static readonly Bencodex.Types.Boolean MarkChanged = true;

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
            _address = new Address(((Bencodex.Types.Binary)serialized["address"]));
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            var states = context.PreviousStates;
            var rankingAddress = RankingState.Address;

            if (context.Rehearsal)
            {
                states = states.SetState(rankingAddress, MarkChanged);
                return states.SetState(_address, MarkChanged);
            }

            Bencodex.Types.Integer currentCount = states.GetState(context.Signer) is Bencodex.Types.Integer bint
                ? bint
                : 0;
            var nextCount = Math.Max(currentCount - _count, 0);

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(rankingAddress) is Bencodex.Types.Dictionary bdict
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(_address, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(_address, (Bencodex.Types.Integer)nextCount);
        }
    }
}
