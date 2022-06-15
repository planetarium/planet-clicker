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
        private SubCountPlainValue _plainValue;
        private static readonly Bencodex.Types.Boolean MarkChanged = true;

        public SubCount()
        {
        }

        public SubCount(Address address, long count)
        {
            _plainValue = new SubCountPlainValue(address, count);
        }

        public override IValue PlainValue => _plainValue.Encode();

        public override void LoadPlainValue(IValue plainValue)
        {
            if (plainValue is Bencodex.Types.Dictionary bdict)
            {
                _plainValue = new SubCountPlainValue(bdict);
            }
            else
            {
                throw new ArgumentException($"Invalid plain value type: {plainValue.GetType()}");
            }
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            IAccountStateDelta states = context.PreviousStates;

            if (context.Rehearsal)
            {
                states = states.SetState(RankingState.Address, MarkChanged);
                return states.SetState(_plainValue.Address, MarkChanged);
            }

            Bencodex.Types.Integer currentCount = states.GetState(_plainValue.Address) is Bencodex.Types.Integer bint
                ? bint
                : 0;
            var nextCount = Math.Max(currentCount - _plainValue.Count, 0);

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(RankingState.Address) is Bencodex.Types.Dictionary bdict
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(_plainValue.Address, nextCount);
            states = states.SetState(RankingState.Address, rankingState.Encode());
            return states.SetState(_plainValue.Address, (Bencodex.Types.Integer)nextCount);
        }
    }
}
