using _Script.State;
using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("add_count")]
    public class AddCount : ActionBase
    {
        private long _count;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _count = count;
        }

        public override IValue PlainValue =>
            Bencodex.Types.Dictionary.Empty.SetItem("count", _count);

        public override void LoadPlainValue(IValue plainValue)
        {
            var serialized = (Bencodex.Types.Dictionary)plainValue;
            _count = (long)((Integer)serialized["count"]).Value;
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;

            var playerState = states.TryGetState(ctx.Signer, out Bencodex.Types.Dictionary playerBDict)
                ? new PlayerState(ctx.Signer, playerBDict)
                : new PlayerState(ctx.Signer);

            Debug.Log($"add_count: CurrentCount: {playerState.Count}, SumCount: {_count}");
            playerState.SumCount((int) _count);

            var rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary rankingBDict)
                ? new RankingState(rankingBDict)
                : new RankingState();

            rankingState.Update(ctx.Signer, playerState.Count);
            states = states.SetState(rankingAddress, rankingState.Serialize());
            return states.SetState(ctx.Signer, playerState.Serialize());
        }
    }
}
