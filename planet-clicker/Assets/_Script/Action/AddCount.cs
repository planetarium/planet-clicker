using _Script.State;
using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity;
using Libplanet.Unity;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("add_count")]
    public class AddCount : ActionBase
    {
        private long _count;
        private static readonly Bencodex.Types.Boolean MarkChanged = true;

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

        public override IAccountStateDelta Execute(IActionContext context)
        {
            var states = context.PreviousStates;
            var rankingAddress = RankingState.Address;
            Bencodex.Types.Integer currentCount = states.GetState(context.Signer) is Bencodex.Types.Integer bint
                ? bint
                : 0;
            var nextCount = currentCount + _count;

            Debug.Log($"add_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(rankingAddress) is Bencodex.Types.Dictionary bdict
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(context.Signer, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(context.Signer, (Bencodex.Types.Integer)nextCount);
        }
    }
}
