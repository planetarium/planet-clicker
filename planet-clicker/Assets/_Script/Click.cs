using UnityEngine;

namespace _Script
{
    public class Click : MonoBehaviour
    {
        public int count;

        private void Awake()
        {
            ResetCount();
        }

        public void Plus()
        {
            count++;
        }

        public void ResetCount()
        {
            count = 0;
        }
    }
}
