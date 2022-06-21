using System;
using Scripts.States;
using Libplanet;
using Libplanet.Action;
using Libplanet.Unity;
using UnityEngine;

namespace Scripts.Actions
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

        public override Bencodex.Types.IValue PlainValue => _plainValue.Encode();

        public override void LoadPlainValue(Bencodex.Types.IValue plainValue)
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

            CountState countState = states.GetState(_plainValue.Address) is Bencodex.Types.Dictionary countStateEncoded
                ? new CountState(countStateEncoded)
                : new CountState(0L);

            long currentCount = countState.Count;
            countState.SubCount(_plainValue.Count);
            long nextCount = countState.Count;

            Debug.Log($"sub_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(RankingState.Address) is Bencodex.Types.Dictionary rankingStateEncoded
                ? new RankingState(rankingStateEncoded)
                : new RankingState();

            rankingState.Update(_plainValue.Address, countState.Count);
            states = states.SetState(RankingState.Address, rankingState.Encode());
            return states.SetState(_plainValue.Address, countState.Encode());
        }
    }
}
