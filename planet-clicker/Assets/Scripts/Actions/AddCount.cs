using System;
using Libplanet.Action;
using Libplanet.Unity;
using Scripts.States;
using UnityEngine;

namespace Scripts.Actions
{
    [ActionType("add_count")]
    public class AddCount : ActionBase
    {
        private AddCountPlainValue _plainValue;
        private static readonly Bencodex.Types.Boolean MarkChanged = true;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _plainValue = new AddCountPlainValue(count);
        }

        public override Bencodex.Types.IValue PlainValue => _plainValue.Encode();

        public override void LoadPlainValue(Bencodex.Types.IValue plainValue)
        {
            if (plainValue is Bencodex.Types.Dictionary bdict)
            {
                _plainValue = new AddCountPlainValue(bdict);
            }
            else
            {
                throw new ArgumentException($"Invalid plain value type: {plainValue.GetType()}");
            }
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            IAccountStateDelta states = context.PreviousStates;
            CountState countState = states.GetState(context.Signer) is Bencodex.Types.Dictionary countStateEncoded
                ? new CountState(countStateEncoded)
                : new CountState(0L);

            long currentCount = countState.Count;
            countState.AddCount(_plainValue.Count);
            long nextCount = countState.Count;
            Debug.Log($"add_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(RankingState.Address) is Bencodex.Types.Dictionary rankingStateEncoded
                ? new RankingState(rankingStateEncoded)
                : new RankingState();

            rankingState.Update(context.Signer, countState.Count);
            states = states.SetState(RankingState.Address, rankingState.Encode());
            return states.SetState(context.Signer, countState.Encode());
        }
    }
}
