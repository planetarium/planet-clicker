using UnityEngine;
using UnityEngine.UI;

namespace _Script
{
    public class Click : MonoBehaviour
    {
        public int _count;
        public Text text;
        public Image image;

        private void Awake()
        {
            Set(1);
            ResetCount();
        }

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

        public void Set(int id)
        {
            image.sprite = null;
            var sprite = Resources.Load<Sprite>($"Images/0{id}") ?? Resources.Load<Sprite>($"Images/01");
            image.sprite = sprite;
            image.SetNativeSize();
        }
    }
}
