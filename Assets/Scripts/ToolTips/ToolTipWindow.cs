using System;
using Reconnect.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.ToolTips
{
    public class ToolTipWindow : MonoBehaviour
    {
        [NonSerialized] public Image background;
        [NonSerialized] public TMP_Text text;
        
        private void Awake()
        {
            if (!TryGetComponent(out background))
                throw new ComponentNotFoundException("No Image component has been found on the ToolTip window prefab.");

            text = GetComponentInChildren<TMP_Text>();
            if (text is null)
                throw new ComponentNotFoundException("No TMP_Text component has been found in the children of the tooltip window prefab.");
        }

        public Vector2 Size
        {
            get => background.rectTransform.sizeDelta;
            set => background.rectTransform.sizeDelta = value;
        }
    }
}