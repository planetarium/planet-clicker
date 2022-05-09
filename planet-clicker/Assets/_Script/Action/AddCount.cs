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
            _data = (AddCountData)DataModel.Decode<AddCountData>((Bencodex.Types.Dictionary)plainValue);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;
            states.TryGetState(ctx.Signer, out Bencodex.Types.Integer currentCount);
            var nextCount = currentCount + _data.count;

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
