using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class Click : MonoBehaviour
    {
        private int _count;
        public Text text;

        public void Plus()
        {
            _count++;
            text.text = _count.ToString();
        }
    }
}
