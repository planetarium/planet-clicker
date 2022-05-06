using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _Script.Action;
using _Script.Data;
using _Script.State;
using LibplanetUnity;
using LibplanetUnity.Action;
using Libplanet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Libplanet.Blockchain.Renderers;
using Libplanet.Action;

namespace _Script
{
    public class Game : MonoBehaviour
    {
        private const float TxProcessInterval = 3.0f;

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

        public class PlayerUpdated : UnityEvent<PlayerState>
        {
        }

        public class RankUpdated: UnityEvent<RankingState>
        {
        }

        public static PlayerUpdated OnPlayerUpdated = new PlayerUpdated();

        public static RankUpdated OnRankUpdated = new RankUpdated();

        private void Awake()
        {
            Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            Agent.Initialize(
                new[]
                {
                    new AnonymousActionRenderer<PolymorphicAction<ActionBase>>()
                    {
                        ActionRenderer = (action, ctx, nextStates) =>
                        {
                            // Renders only when the count has updated.
                            if (nextStates.GetState(ctx.Signer) is Bencodex.Types.Dictionary playerBDict)
                            {
                                var playerState = new PlayerState(ctx.Signer, playerBDict);
                                Agent.instance.RunOnMainThread(() =>
                                {
                                    OnPlayerUpdated.Invoke(playerState);
                                });
                            }

                            // Renders only when the ranking has changed.
                            if (nextStates.GetState(RankingState.Address) is Bencodex.Types.Dictionary rawRank)
                            {
                                var rankingState = new RankingState(rawRank);
                                Agent.instance.RunOnMainThread(() =>
                                {
                                    OnRankUpdated.Invoke(rankingState);
                                });
                            }
                        }
                    }
                }
            );
            var agent = Agent.instance;
            var hex = agent.Address.ToHex().Substring(0, 4);
            addressText.text = $"My Address: {hex}";

            _time = TxProcessInterval;
            SetTimer(_time);

            _levelTable = new Table<Level>();
            _levelTable.Load(Resources.Load<TextAsset>("level").text);

            OnPlayerUpdated.AddListener(UpdateTotalCount);
            OnRankUpdated.AddListener(rs =>
            {
                StartCoroutine(UpdateRankingBoard(rs));
            });

            var initialCount = agent.GetState(Agent.instance.Address);
            var initialRanking = agent.GetState(RankingState.Address);
            if (initialCount is Bencodex.Types.Dictionary playerDBict)
            {
                OnPlayerUpdated.Invoke(new PlayerState(agent.Address, playerDBict));
            }

            if (initialRanking is Bencodex.Types.Dictionary rankingBDict)
            {
                OnRankUpdated.Invoke(new RankingState(rankingBDict));
            }
        }

        private void SetTimer(float time)
        {
            timerText.text = $"Remain Time: {Mathf.Ceil(time).ToString(CultureInfo.CurrentCulture)} sec";
        }

        private void ResetTimer()
        {
            SetTimer(0);
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
                var actions = new List<ActionBase>();
                if (click.count > 0)
                {
                    var action = new AddCount(click.count);
                    actions.Add(action);
                }

                actions.AddRange(_attacks.Select(pair => new SubCount(pair.Key, pair.Value)));
                if (actions.Any())
                {
                    Agent.instance.MakeTransaction(actions);
                }
                _attacks = new Dictionary<Address, int>();

                ResetTimer();
            }
        }

        private void UpdateTotalCount(PlayerState playerState)
        {
            _totalCount = playerState.Count;
            var selected = _levelTable.Values.FirstOrDefault(i => i.exp > _totalCount) ?? _levelTable.Values.Last();
            click.Set(selected.id);
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
                if (rankingInfo.address == Agent.instance.Address)
                {
                    rankingText.text = $"My Ranking: {rank}";
                }
            }

            rankingRow.gameObject.SetActive(false);
            yield return null;
        }

        public void Attack(RankingRow row)
        {
            var address = row.address;

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
