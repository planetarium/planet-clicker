using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Unity;
using _Script.State;
using _Script.Data;
using UnityEngine;

namespace _Script.Action
{
    // Action Type 명 지정
    [ActionType("add_count")]
    public class AddCount : ActionBase
    {
        // 내부적으로 count 값을 관리하기 위한 변수
        private CountData _count;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _count = new CountData(count);
        }

        // 직렬화 진행
        public override IValue PlainValue => _count.Encode();

        // 역직렬화 진행
        public override void LoadPlainValue(IValue plainValue)
        {
            _count = new CountData((Bencodex.Types.Dictionary)plainValue);
        }

        // 액션 실행에 대한 정의
        public override IAccountStateDelta Execute(IActionContext context)
        {
            // 이전 상태를 가져옵니다.
            var states = context.PreviousStates;

            // 이전 상태의 count 값을 가져옵니다.
            CountState prevState;
            prevState = states.GetState(context.Signer) is Bencodex.Types.Dictionary bdict
                ? new CountState(new CountData(bdict))
                : new CountState(new CountData(0));

            // +n된 count로 설정하기 위해서 더해줍니다. 
            prevState.SumCount(_count.count);

            Debug.Log($"add_count: CurrentCount: {prevState.Count}");

            // state를 업데이트합니다.
            return states.SetState(context.Signer, prevState.Serialize());
        }
    }
}

