using _Script.State;
using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("add_count")]
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
            _count = (long)((Integer)serialized["count"]).Value;
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;
            states.TryGetState(ctx.Signer, out Bencodex.Types.Integer currentCount);
            var nextCount = currentCount + _count;

            Debug.Log($"add_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            var rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary bdict)
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(ctx.Signer, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(ctx.Signer, (Bencodex.Types.Integer)nextCount);
        }
    }
}
