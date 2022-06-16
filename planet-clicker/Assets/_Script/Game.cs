using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _Script.Action;
using _Script.Data;
using _Script.State;
using Libplanet.Unity;
using Libplanet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Libplanet.Blockchain.Renderers;
using Libplanet.Action;

namespace _Script
{
    public class CountUpdated : UnityEvent<CountState>
    {
    }

    public class RankUpdated : UnityEvent<RankingState>
    {
    }

    public class Game : MonoBehaviour
    {
        public const float TxProcessInterval = 3.0f;

        public Text timerText;
        public Text countText;
        public Text addressText;
        public Text rankingText;
        public Click click;
        public ScrollRect rankingBoard;
        public RankingRow rankingRow;
        private float _time;
        private long _totalCount = 0;
        private Table<Level> _levelTable;
        private Dictionary<Address, int> _attacks = new Dictionary<Address, int>();

        private CountUpdated _onCountUpdated;
        private RankUpdated _onRankUpdated;
        private IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> _renderers;

        public void Awake()
        {
            Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            _onCountUpdated = new CountUpdated();
            _onRankUpdated = new RankUpdated();
            _renderers = new List<IRenderer<PolymorphicAction<ActionBase>>>()
            {
                new AnonymousActionRenderer<PolymorphicAction<ActionBase>>()
                {
                    ActionRenderer = (action, context, nextStates) =>
                    {
                        // Renders only when the count has updated.
                        if (nextStates.GetState(context.Signer) is Bencodex.Types.Dictionary countStateEncoded)
                        {
                            CountState countState = new CountState(countStateEncoded);
                            Agent.Instance.RunOnMainThread(() => _onCountUpdated.Invoke(countState));
                        }

                        // Renders only when the ranking has changed.
                        if (nextStates.GetState(RankingState.Address) is Bencodex.Types.Dictionary rankingStateEncoded)
                        {
                            RankingState rankingState = new RankingState(rankingStateEncoded);
                            Agent.Instance.RunOnMainThread(() => _onRankUpdated.Invoke(rankingState));
                        }
                    }
                }
            };

            Agent.Initialize(_renderers);
            Agent agent = Agent.Instance;
            string hex = agent.Address.ToHex().Substring(0, 4);
            addressText.text = $"My Address: {hex}";

            _time = TxProcessInterval;
            SetTimer(_time);

            _levelTable = new Table<Level>();
            _levelTable.Load(Resources.Load<TextAsset>("level").text);

            _onCountUpdated.AddListener(UpdateTotalCount);
            _onRankUpdated.AddListener(rankingState => StartCoroutine(UpdateRankingBoard(rankingState)));
        }

        public void Start()
        {
            Agent agent = Agent.Instance;
            Bencodex.Types.IValue initialCountState = agent.GetState(Agent.Instance.Address);
            Bencodex.Types.IValue initialRankingState = agent.GetState(RankingState.Address);
            if (initialCountState is Bencodex.Types.Dictionary countStateEncoded)
            {
                CountState countState = new CountState(countStateEncoded);
                _onCountUpdated.Invoke(countState);
            }

            if (initialRankingState is Bencodex.Types.Dictionary rankingStateEncoded)
            {
                _onRankUpdated.Invoke(new RankingState(rankingStateEncoded));
            }
        }

        private void SetTimer(float time)
        {
            timerText.text = $"Remain Time: {time:F1} sec";
        }

        private void ResetTimer()
        {
            SetTimer(0.0f);
            click.ResetCount();
        }

        private void FixedUpdate()
        {
            if (_time > 0)
            {
                _time -= Time.deltaTime;
                SetTimer(_time);
            }
            else
            {
                _time = TxProcessInterval;
                List<ActionBase> actions = new List<ActionBase>();
                if (click.count > 0)
                {
                    var action = new AddCount(click.count);
                    actions.Add(action);
                }

                foreach ((Address address, int count) in _attacks)
                {
                    actions.Add(new SubCount(address, count));
                }

                if (actions.Any())
                {
                    Agent.Instance.MakeTransaction(actions);
                }
                _attacks = new Dictionary<Address, int>();

                ResetTimer();
            }
        }

        private void UpdateTotalCount(CountState countState)
        {
            _totalCount = countState.Count;
            Level selected = _levelTable.Values.FirstOrDefault(i => i.Exp > _totalCount) ?? _levelTable.Values.Last();
            click.Set(selected.Id);
            countText.text = $"Total Count: {_totalCount.ToString()}";
        }

        private IEnumerator UpdateRankingBoard(RankingState rankingState)
        {
            foreach (Transform child in rankingBoard.content.transform)
            {
                Destroy(child.gameObject);
            }
            yield return new WaitForEndOfFrame();

            rankingRow.gameObject.SetActive(true);
            var ranking = rankingState.GetRanking().ToList();
            for (var i = 0; i < ranking.Count; i++)
            {
                var rankingInfo = ranking[i];
                var go = Instantiate(rankingRow, rankingBoard.content.transform);
                var bg = go.GetComponent<Image>();
                if (i % 2 == 1)
                {
                    bg.enabled = false;
                }
                var row = go.GetComponent<RankingRow>();
                var rank = i + 1;
                row.Set(rank, rankingInfo);
                if (rankingInfo.Address == Agent.Instance.Address)
                {
                    rankingText.text = $"My Ranking: {rank}";
                }
            }

            rankingRow.gameObject.SetActive(false);
            yield return null;
        }

        public void Attack(RankingRow row)
        {
            Address address = row.address;

            if (_attacks.TryGetValue(address, out _))
            {
                _attacks[address] += 1;
            }
            else
            {
                _attacks[address] = 0;
            }
        }
    }
}
