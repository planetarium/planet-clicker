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
            _address = new Address(((Bencodex.Types.Binary)serialized["address"]));
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

            var playerState = states.TryGetState(ctx.Signer, out Bencodex.Types.Dictionary playerBDict)
                ? new PlayerState(ctx.Signer, playerBDict)
                : new PlayerState(ctx.Signer);

            Debug.Log($"sub_count: CurrentCount: {playerState.Count}, SubCount: {_count}");
            playerState.SubCount((int) _count);

            RankingState rankingState;
            rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary rankingBDict)
                ? new RankingState(rankingBDict)
                : new RankingState();

            rankingState.Update(_address, playerState.Count);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(_address, playerState.Serialize());
        }
    }
}
