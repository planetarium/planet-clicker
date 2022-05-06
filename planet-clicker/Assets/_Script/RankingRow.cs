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
            address = player.Address;
            rankingText.text = ranking.ToString();
            addressText.text = player.Address.ToHex().Substring(0, 4).ToString();
            countText.text = player.Count.ToString();
        }
    }
}
