using System.Collections;
using System.Globalization;
using System.Linq;
using _Script.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class Game : MonoBehaviour
    {
        public Text timerText;
        public Text countText;
        public Text addressText;
        public Click click;
        private float _time;
        private long _totalCount = 0;
        private Table<Level> _levelTable;

        private void Awake()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            AgentController.Initialize();
            var hex = AgentController.Agent.Address.ToHex().Substring(0, 4);
            addressText.text = $"Address: {hex}";
            _time = Agent.TxProcessInterval;
            SetTimer(_time);
            _levelTable = new Table<Level>();
            _levelTable.Load(Resources.Load<TextAsset>("level").text);
            StartCoroutine(GetTotalCount());
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
                _time = Agent.TxProcessInterval;
                if (click._count > 0)
                {
                    var action = new AddCount(click._count);
                    AgentController.Agent.MakeTransaction(action);
                }

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

        private IEnumerator GetTotalCount()
        {
            while (true)
            {
                var count = (long?) AgentController.Agent.GetState(AgentController.Agent.Address) ?? 0;
                if (count > _totalCount)
                {
                    UpdateTotalCount(count);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
