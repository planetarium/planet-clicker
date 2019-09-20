using System.Collections.Generic;
using System.Collections.Immutable;
using Libplanet.Action;

namespace _Script
{
    [ActionType("store_count")]
    public class AddCount : ActionBase
    {
        private long _count;

        public AddCount(int reward)
        {
            _count = _count;
        }

        public override IImmutableDictionary<string, object> PlainValue =>
            new Dictionary<string, object>
            {
                ["count"] = _count,
            }.ToImmutableDictionary();

        public override void LoadPlainValue(IImmutableDictionary<string, object> plainValue)
        {
            _count = (long)plainValue["count"];
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            if (ctx.Rehearsal)
            {
                return states.SetState(ctx.Signer, MarkChanged);
            }

            var currentCount = (long?)states.GetState(ctx.Signer)?? 0;
            var nextCount = currentCount + _count;

            return states.SetState(ctx.Signer, nextCount);
        }
    }
}
