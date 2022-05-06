using _Script.State;
using Libplanet;
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
        public Address address;

        public void Set(int ranking, PlayerState player)
        {
            address = player.address;
            rankingText.text = ranking.ToString();
            addressText.text = player.address.ToHex().Substring(0, 4).ToString();
            countText.text = player.Count.ToString();
        }
    }
}
