using System.Collections.Generic;
using System.Linq;
using _Script.Action;
using _Script.State;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Libplanet.Blockchain.Renderers;
using Libplanet.Action;
using Libplanet.Unity;

namespace _Script
{
    public class Game : MonoBehaviour
    {
        // 3초 타이머를 설정할 때 필요한 Interval 값입니다.
        private const float TxProcessInterval = 3.0f;

        // UI와 연결하기 위한 변수입니다.
        public Text countText;
        public Text addressText;
        
        // Count를 임시로 저장해둘 객체입니다.
        public Click click;

        // 3초를 계산하기 위한 변수입니다.
        private float _time;
        // 실행된 이후부터 종료될 때 까지 Count를 저장해둘 변수입니다.
        private long _totalCount = 0;

        // Listener를 등록하기 위한 객체입니다.
        public class CountUpdated : UnityEvent<Bencodex.Types.Dictionary>
        {
        }

        public static CountUpdated OnCountUpdated = new CountUpdated();

        private void Awake()
        {
            // 해상도와 로그 설정
            Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            // Libplant.Unity를 초기화하기 위한 작업입니다.
            // 초기화할 때 renderer를 넣어줘야하는데 renderer는 액션이 실행되었을 때 실행되는 객체입니다. 
            Agent.Initialize(
                new[]
                {
                    new AnonymousActionRenderer<PolymorphicAction<ActionBase>>()
                    {
                        ActionRenderer = (action, ctx, nextStates) =>
                        {
                            // 실행되어서 업데이트된 Count를 가져옵니다.
                            if (nextStates.GetState(ctx.Signer) is Bencodex.Types.Dictionary bdict)
                            {
                                Agent.Instance.RunOnMainThread(() =>
                                {
                                    // 위에 선언해둔 Listener에 Event를 전달합니다.
                                    OnCountUpdated.Invoke(bdict);
                                });
                            }
                        }
                    }
                }
            );
            var agent = Agent.Instance;

            // UI에 Address 값을 업데이트합니다.
            var hex = agent.Address.ToHex().Substring(0, 4);
            addressText.text = $"My Address: {hex}";

            // 3초로 초기화 합니다.
            _time = TxProcessInterval;

            // Listener를 등록합니다.
            OnCountUpdated.AddListener(UpdateTotalCount);

            // 초기에 BlockChain에서 데이터를 가져와 초기화 시켜줍니다.
            var initialState = agent.GetState(Agent.Instance.Address);
            if (initialState is Bencodex.Types.Dictionary bdict)
            {
                OnCountUpdated.Invoke(bdict);
            }
        }

        // 유니티가 프레임마다 불러주는 메소드입니다.
        private void FixedUpdate()
        {
            if (_time > 0)
            {
                // 아직 3초가 지나지 않았다면 0초가 될 때 까지 계속 - 시킵니다.
                _time -= Time.deltaTime;
            }
            else
            {
                // 타이머를 다시 3초로 초기화 시켜줍니다.
                _time = TxProcessInterval;

                // 액션은 여러개 실행될 수 있어 List로 관리합니다.
                List<ActionBase> actions = new List<ActionBase>();
                if (click.count > 0)
                {
                    // 액션을 생성하여 List에 추가합니다.
                    var action = new AddCount(click.count);
                    actions.Add(action);
                }

                if (actions.Any())
                {
                    // BlockChain에 Transaction을 생성합니다.
                    Agent.Instance.MakeTransaction(actions);
                }

                // 액션이 실행되었으니 초기화해줍니다.
                click.ResetCount();
            }
        }

        // 리스너에 등록되는 Update 메소드입니다.
        private void UpdateTotalCount(Bencodex.Types.Dictionary bdict)
        {
            CountState state = new CountState(bdict);
            _totalCount = state.Count;
            countText.text = $"Total Count: {_totalCount.ToString()}";
        }
    }
}
