using _Script.State;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Store;
using LibplanetUnity;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("add_count")]
    public class AddCount : ActionBase
    {
        private AddCountData _data;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _data = new AddCountData(count);
        }

        public override IValue PlainValue => _data.Encode();

        public override void LoadPlainValue(IValue plainValue)
        {
            _data = new AddCountData((Bencodex.Types.Dictionary)plainValue);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            CountState countState = states.TryGetState(ctx.Signer, out Bencodex.Types.Integer encodedCount)
                ? new CountState(encodedCount)
                : new CountState();

            var currentCount = countState.Count;
            countState.AddCount(_data.count);
            var nextCount = countState.Count;

            Debug.Log($"add_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            var rankingState = states.TryGetState(RankingState.Address, out Bencodex.Types.Dictionary bdict)
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(ctx.Signer, countState.Count);
            states = states.SetState(RankingState.Address, rankingState.Serialize());
            return states.SetState(ctx.Signer, countState.Encode());
        }
    }
}
