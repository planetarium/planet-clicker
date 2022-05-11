using _Script.State;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("sub_count")]
    public class SubCount : ActionBase
    {
        private SubCountData _data;

        public SubCount()
        {
        }

        public SubCount(Address address, long count)
        {
            _data = new SubCountData(address, count);
        }

        public override IValue PlainValue => _data.Encode();

        public override void LoadPlainValue(IValue plainValue)
        {
            _data = new SubCountData((Dictionary)plainValue);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;

            if (ctx.Rehearsal)
            {
                states = states.SetState(RankingState.Address, MarkChanged);
                return states.SetState(_data.address, MarkChanged);
            }

            CountState countState = states.TryGetState(_data.address, out Integer encodedCount)
                ? new CountState(encodedCount)
                : new CountState();
            var currentCount = countState.Count;
            countState.SubCount(_data.count);
            var nextCount = countState.Count;

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState;
            rankingState = states.TryGetState(RankingState.Address, out Dictionary encodedRanking)
                ? new RankingState(encodedRanking)
                : new RankingState();

            rankingState.Update(_data.address, countState.Count);
            states = states.SetState(RankingState.Address, rankingState.Encode());
            return states.SetState(_data.address, countState.Encode());
        }
    }
}
