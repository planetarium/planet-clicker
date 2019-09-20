using System.Collections.Generic;
using System.Collections.Immutable;
using _Script.State;
using Libplanet.Action;
using UnityEngine;

namespace _Script
{
    [ActionType("store_count")]
    public class AddCount : ActionBase
    {
        private long _count;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _count = count;
        }

        public override IImmutableDictionary<string, object> PlainValue =>
            new Dictionary<string, object>
            {
                ["count"] = _count.ToString(),
            }.ToImmutableDictionary();

        public override void LoadPlainValue(IImmutableDictionary<string, object> plainValue)
        {
            _count = long.Parse(plainValue["count"].ToString());
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;
            if (ctx.Rehearsal)
            {
                states = states.SetState(rankingAddress, MarkChanged);
                return states.SetState(ctx.Signer, MarkChanged);
            }

            var currentCount = (long?)states.GetState(ctx.Signer)?? 0;
            var nextCount = currentCount + _count;

            Debug.Log($"store_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            var rankingState = (RankingState) states.GetState(rankingAddress) ?? new RankingState();
            rankingState.Update(ctx.Signer, nextCount);
            states = states.SetState(rankingAddress, rankingState);
            return states.SetState(ctx.Signer, nextCount);
        }
    }
}
