using System;
using System.Collections.Immutable;
using Libplanet.Action;

namespace _Script
{
    [Serializable]
    public abstract class ActionBase : IAction
    {
        protected const string MarkChanged = "";

        public abstract IImmutableDictionary<string, object> PlainValue { get; }
        public abstract void LoadPlainValue(IImmutableDictionary<string, object> plainValue);
        public abstract IAccountStateDelta Execute(IActionContext ctx);

        public void Render(IActionContext context, IAccountStateDelta nextStates)
        {
        }

        public void Unrender(IActionContext context, IAccountStateDelta nextStates)
        {
        }

    }
}
