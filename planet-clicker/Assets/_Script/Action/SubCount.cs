using System;
using _Script.State;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Store;
using LibplanetUnity;
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
            _data = (SubCountData)DataModel.Decode<SubCountData>((Bencodex.Types.Dictionary)plainValue);
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;

            if (ctx.Rehearsal)
            {
                states = states.SetState(rankingAddress, MarkChanged);
                return states.SetState(_data.address, MarkChanged);
            }

            states.TryGetState(_data.address, out Bencodex.Types.Integer currentCount);
            var nextCount = Math.Max(currentCount - _data.count, 0);

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState;
            rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary bdict)
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(_data.address, nextCount);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(_data.address, (Bencodex.Types.Integer)nextCount);
        }
    }
}
