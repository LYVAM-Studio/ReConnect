using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.MouseEvents
{
    public interface ICursorHandle :
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler,
        IPointerClickHandler
    {
        protected bool IsPointerDown { get;set; }

        public void OnCursorEnter() { }
        public void OnCursorExit() { }
        public void OnCursorDown() { }
        public void OnCursorUp() { }
        public void OnCursorClick() { }
        public void OnCursorDrag() { }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnCursorEnter();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnCursorExit();
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            IsPointerDown = true;
            OnCursorDown();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (IsPointerDown)
            {
                OnCursorUp();
                IsPointerDown = false;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            OnCursorDrag();
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            OnCursorClick();
        }

    }
}