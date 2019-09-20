using System.Globalization;
using System.Linq;
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

        private void Awake()
        {
            AgentController.Initialize();
            var hex = string.Join("", AgentController.Agent.Address.ToHex().Take(4));
            addressText.text = $"Address: {hex}";
            _time = Agent.TxProcessInterval;
            SetTimer(_time);
        }

        private void SetTimer(float time)
        {
            timerText.text = Mathf.Ceil(time).ToString(CultureInfo.CurrentCulture);
        }

        private void ResetTimer()
        {
            SetTimer(0);
            int.TryParse(click.text.text, out var count);
            UpdateTotalCount(count);
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
                ResetTimer();
                _time = Agent.TxProcessInterval;
            }
        }

        public void UpdateTotalCount(long count)
        {
            _totalCount += count;
            countText.text = _totalCount.ToString();
        }
    }
}
