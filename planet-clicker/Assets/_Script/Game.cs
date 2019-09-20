using System;
using System.Globalization;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class Game : MonoBehaviour
    {
        public Text timerText;
        public Click click;
        private float _time;

        private void Awake()
        {
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
    }
}
