using Scripts.States;
using Libplanet;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts
{
    public class RankingRow : MonoBehaviour
    {
        public Text rankingText;
        public Text addressText;
        public Text countText;
        public Button attackButton;
        public Address address;

        public void Set(int ranking, RankingInfo info)
        {
            address = info.Address;
            rankingText.text = ranking.ToString();
            addressText.text = info.Address.ToHex().Substring(0, 4).ToString();
            countText.text = info.Count.ToString();
        }
    }
}
