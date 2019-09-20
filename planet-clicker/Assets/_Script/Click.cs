using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class Click : MonoBehaviour
    {
        public int _count;
        public Text text;

        public void Plus()
        {
            _count++;
            text.text = _count.ToString();
        }

        public void ResetCount()
        {
            _count = 0;
            text.text = _count.ToString();
        }
    }
}
