using _Script.State;
using System;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Unity;
using UnityEngine;

namespace _Script.Action
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

        public override IValue PlainValue => _plainValue.Encode();

        public override void LoadPlainValue(IValue plainValue)
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
            Bencodex.Types.Integer currentCount = states.GetState(context.Signer) is Bencodex.Types.Integer bint
                ? bint
                : 0;
            var nextCount = currentCount + _plainValue.Count;

            Debug.Log($"add_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            RankingState rankingState = states.GetState(RankingState.Address) is Bencodex.Types.Dictionary bdict
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(context.Signer, nextCount);
            states = states.SetState(RankingState.Address, rankingState.Encode());
            return states.SetState(context.Signer, (Bencodex.Types.Integer)nextCount);
        }
    }
}
