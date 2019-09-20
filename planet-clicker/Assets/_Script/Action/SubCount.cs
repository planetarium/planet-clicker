using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Libplanet;
using Libplanet.Action;
using UnityEngine;

namespace _Script
{
    [ActionType("decrease_count")]
    public class SubCount : ActionBase
    {
        private Address _address;
        private long _count;

        public SubCount()
        {
        }

        public SubCount(Address address, long count)
        {
            _address = address;
            _count = count;
        }

        public override IImmutableDictionary<string, object> PlainValue =>
            new Dictionary<string, object>
            {
                ["address"] = _address.ToHex(),
                ["count"] = _count.ToString(),
            }.ToImmutableDictionary();

        public override void LoadPlainValue(IImmutableDictionary<string, object> plainValue)
        {
            _address = new Address((string)plainValue["address"]);
            _count = long.Parse(plainValue["count"].ToString());
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            if (ctx.Rehearsal)
            {
                return states.SetState(_address, MarkChanged);
            }

            var currentCount = (long?)states.GetState(_address) ?? 0;
            var nextCount = Math.Max(currentCount - _count, 0);

            Debug.Log($"decrease_count: CurrentCount: {currentCount}, NextCount: {nextCount}");

            return states.SetState(_address, nextCount);
        }
    }
}
