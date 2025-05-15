using System;
using Reconnect.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.ToolTips
{
    public class ToolTipWindow : MonoBehaviour
    {
        [NonSerialized] private Image _background;
        [NonSerialized] private TMP_Text _textComponent;

        public string Text
        {
            get => _textComponent.text;
            set => _textComponent.text = value;
        }
        
        public Vector2 Size
        {
            get => _background.rectTransform.sizeDelta;
            set => _background.rectTransform.sizeDelta = value;
        }
        
        private void Awake()
        {
            if (!TryGetComponent(out _background))
                throw new ComponentNotFoundException("No Image component has been found on the ToolTip window prefab.");

            _textComponent = GetComponentInChildren<TMP_Text>();
            if (_textComponent is null)
                throw new ComponentNotFoundException("No TMP_Text component has been found in the children of the tooltip window prefab.");
        }
    }
}