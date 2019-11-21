using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using UnityEngine;

namespace LibplanetUnity.Action
{
    public static class AccountStateDeltaExtensions
    {
        public static bool TryGetState<T>(this IAccountStateDelta states, Address address, out T result)
            where T : IValue
        {
            IValue raw = states.GetState(address);
            if (raw is T v)
            {
                result = v;
                return true;
            }

            result = default;
            return false;
        }
    }
}
