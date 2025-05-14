using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Reconnect.ToolTips
{
    public class HoverToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        [TextArea(1, 3)]
        public string text;

        public Size toolTipSize;

        public float timeBeforeAppearing = 0.5f;

        private Coroutine _coroutine;
        private bool _isShown;
        
       private void Awake()
       {
           ToolTipManager.Instance.CreateToolTip(GetHashCode(), text, toolTipSize);
       }
       
       private void Update()
       {
           if (_isShown)
               ToolTipManager.Instance.SetPositionToMouse(GetHashCode());
       }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            Hide();
        }

        private void Show()
        {
            if (_coroutine is not null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ShowCoroutine());
        }

        private void Hide()
        {
            if (_coroutine is not null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            
            if (_isShown)
            {
                ToolTipManager.Instance.HideToolTip(GetHashCode());
            }
            
            _isShown = false;
        }
        
        private IEnumerator ShowCoroutine()
        {
            yield return new WaitForSeconds(timeBeforeAppearing);
            ToolTipManager.Instance.ShowToolTip(GetHashCode());
            ToolTipManager.Instance.SetPositionToMouse(GetHashCode());
            _isShown = true;
        }
    }
}