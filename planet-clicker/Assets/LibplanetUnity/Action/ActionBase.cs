using System;
using System.Collections.Immutable;
using Libplanet.Action;

namespace LibplanetUnity.Action
{
    [Serializable]
    public abstract class ActionBase : IAction
    {
        protected const string MarkChanged = "";

        public abstract IImmutableDictionary<string, object> PlainValue { get; }
        public abstract void LoadPlainValue(IImmutableDictionary<string, object> plainValue);
        public abstract IAccountStateDelta Execute(IActionContext ctx);
        public abstract void Render(IActionContext context, IAccountStateDelta nextStates);
        public abstract void Unrender(IActionContext context, IAccountStateDelta nextStates);
    }
}
