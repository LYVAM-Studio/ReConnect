using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.ToolTips
{
    public class HoverToolTip : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        [TextArea(1, 3)]
        [SyncVar(hook = nameof(OnTextChanged))]
        private string text;

        public float width;
        public float height;

        public float timeBeforeAppearing = 0.5f;
        
        public string Text
        {
            get => text;
            set
            {
                text = value;
                ToolTipManager.Instance.SetText(_id, value);
            }
        }

        private void OnTextChanged(string _, string newValue)
        {
            ToolTipManager.Instance.SetText(_id, newValue); // update text clients
        }

        private int _id;
        private Coroutine _coroutine;
        private bool _isShown;
        private bool _forceHide;
       private void Awake()
       {
           _id = GetHashCode();
           ToolTipManager.Instance.CreateToolTip(_id);
           ToolTipManager.Instance.SetText(_id, text);
           ToolTipManager.Instance.SetSize(_id, width, height);
       }
       
       private void Update()
       {
           if (_isShown)
               ToolTipManager.Instance.SetPositionToMouse(_id);
       }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.LogError("Pointer enter");
            Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.LogError("Pointer Exit");
            Hide();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.LogError("Pointer drag");
            Hide();
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.LogWarning($"EndDragCallback hide={_forceHide}");
            if (!_forceHide)
                Show();
            _forceHide = false;
        }

        public void ForceHideUntilEndDrag() => _forceHide = true;
        
        private void Show()
        {
            if (_coroutine is not null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ShowCoroutine());
        }

        public void Hide()
        {
            if (_coroutine is not null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            
            if (_isShown)
            {
                ToolTipManager.Instance.HideToolTip(_id);
            }
            
            _isShown = false;
        }
        
        private IEnumerator ShowCoroutine()
        {
            yield return new WaitForSeconds(timeBeforeAppearing);
            ToolTipManager.Instance.ShowToolTip(_id);
            ToolTipManager.Instance.SetPositionToMouse(_id);
            _isShown = true;
        }
    }
}