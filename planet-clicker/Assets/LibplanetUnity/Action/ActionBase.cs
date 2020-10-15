using System;
using Bencodex.Types;
using Libplanet.Action;

namespace LibplanetUnity.Action
{
    [Serializable]
    public abstract class ActionBase : IAction
    {
        protected static readonly Bencodex.Types.Boolean MarkChanged = true;

        public abstract IValue PlainValue { get; }
        public abstract void LoadPlainValue(IValue plainValue);
        public abstract IAccountStateDelta Execute(IActionContext ctx);
    }
}
