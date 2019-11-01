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

namespace _Script
{
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

        public class CountUpdated : UnityEvent<long>
        {
        }

        public class RankUpdated: UnityEvent<RankingState>
        {
        }

        public static CountUpdated OnCountUpdated = new CountUpdated();

        public static RankUpdated OnRankUpdated = new RankUpdated();

        private void Awake()
        {
            Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Agent.Initialize();
            var agent = Agent.instance;
            var hex = agent.Address.ToHex().Substring(0, 4);
            addressText.text = $"Address: {hex}";
            _time = TxProcessInterval;
            SetTimer(_time);
            _levelTable = new Table<Level>();
            _levelTable.Load(Resources.Load<TextAsset>("level").text);

            OnCountUpdated.AddListener(count =>
            {
                agent.RunOnMainThread(() =>
                {
                    UpdateTotalCount(count);
                });
            });
            OnRankUpdated.AddListener(rs =>
            {
                agent.RunOnMainThread(() =>
                {
                    StartCoroutine(UpdateRankingBoard(rs));
                });
            });

            OnCountUpdated.Invoke((long?) agent.GetState(Agent.instance.Address) ?? 0);
            OnRankUpdated.Invoke((RankingState) agent.GetState(RankingState.Address) ?? new RankingState());
        }

        private void SetTimer(float time)
        {
            timerText.text = Mathf.Ceil(time).ToString(CultureInfo.CurrentCulture);
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
                if (click._count > 0)
                {
                    var action = new AddCount(click._count);
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

        private void UpdateTotalCount(long count)
        {
            _totalCount = count;
            var selected = _levelTable.Values.FirstOrDefault(i => i.exp > _totalCount) ?? _levelTable.Values.Last();
            click.Set(selected.id);
            countText.text = _totalCount.ToString();
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
                var row = go.GetComponent<RankingRow>();
                var rank = i + 1;
                row.Set(rank, rankingInfo);
                if (rankingInfo.Address == Agent.instance.Address)
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
