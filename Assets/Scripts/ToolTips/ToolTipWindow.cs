using TMPro;
using UnityEngine;

namespace Reconnect.ToolTips
{
    public class ToolTipWindow : MonoBehaviour
    {
        [SerializeField]
        private Size size;
        public TMP_Text text;
        
        public Size Size => size;
    }
}