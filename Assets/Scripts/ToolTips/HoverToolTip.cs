using System;
using System.Collections;
using Mirror;
using Reconnect.MouseEvents;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.ToolTips
{
    public class HoverToolTip : NetworkBehaviour, ICursorHandle
    {
        bool ICursorHandle.IsPointerDown { get; set; }
        bool ICursorHandle.IsPointerOver { get; set; }

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

       void ICursorHandle.OnCursorEnter()
        {
            Show();
        }

        void ICursorHandle.OnCursorExit()
        {
            Hide();
        }

        void ICursorHandle.OnCursorDrag()
        {
            Hide();
        }
        
        void ICursorHandle.OnCursorEndDrag()
        {
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