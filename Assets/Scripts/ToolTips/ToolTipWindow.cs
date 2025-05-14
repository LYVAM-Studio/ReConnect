using TMPro;
using UnityEngine;

namespace Reconnect.ToolTips
{
    public class ToolTipWindow : MonoBehaviour
    {
        public GameObject canvas;
        public RectTransform position;
        public TMP_Text text;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}