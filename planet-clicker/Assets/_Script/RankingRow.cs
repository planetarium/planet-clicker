using _Script.State;
using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class RankingRow : MonoBehaviour
    {
        public Text rankingText;
        public Text addressText;
        public Text countText;
        public Button attackButton;

        public void Set(int ranking, RankingInfo info)
        {
            rankingText.text = ranking.ToString();
            addressText.text = info.Address.ToHex().Substring(0, 4).ToString();
            countText.text = info.Count.ToString();
        }
    }
}
