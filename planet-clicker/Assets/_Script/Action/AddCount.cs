using System.Collections.Generic;
using System.Collections.Immutable;
using _Script.State;
using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
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

        public override IValue PlainValue =>
            Bencodex.Types.Dictionary.Empty.SetItem("count", _count);

        public override void LoadPlainValue(IValue plainValue)
        {
            var serialized = (Bencodex.Types.Dictionary)plainValue;
            _count = (long) ((Integer) serialized["count"]).Value;
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;
            states.TryGetState(ctx.Signer, out Bencodex.Types.Integer currentCount);
            var nextCount = currentCount + _count;
            
            Debug.Log($"store_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState;
            if (states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary bdict))
            {
                rankingState = new RankingState(bdict);
            }
            else 
            {
                rankingState = new RankingState();
            }
            rankingState.Update(ctx.Signer, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(ctx.Signer, (Bencodex.Types.Integer)nextCount);
        }

        public override void Render(IActionContext ctx, IAccountStateDelta nextStates)
        {
            var agent = Agent.instance;
            var count = (long)((Integer)nextStates.GetState(ctx.Signer));
            var rankingState = new RankingState(
                (Bencodex.Types.Dictionary)nextStates.GetState(RankingState.Address)
            );

            agent.RunOnMainThread(() =>
            {
                Game.OnCountUpdated.Invoke(count);
            });
            agent.RunOnMainThread(() =>
            {
                Game.OnRankUpdated.Invoke(rankingState);
            });
        }

        public override void Unrender(IActionContext context, IAccountStateDelta nextStates)
        {
        }
    }
}
