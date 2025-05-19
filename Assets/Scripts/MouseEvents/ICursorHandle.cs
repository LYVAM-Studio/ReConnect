using UnityEngine.EventSystems;

namespace Reconnect.MouseEvents
{
    public interface ICursorHandle :
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        protected bool IsPointerDown { get;set; }
        protected bool IsPointerOver { get;set; }

        public void OnCursorEnter() { }
        public void OnCursorExit() { }
        public void OnCursorDown() { }
        public void OnCursorUp() { }
        public void OnCursorClick() { }
        public void OnCursorDrag() { }
        public void OnCursorBeginDrag() { }
        public void OnCursorEndDrag() { }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsPointerOver = true;
            OnCursorEnter();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsPointerOver = false;
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

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (IsPointerOver)
                OnCursorBeginDrag();
        }
        
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (IsPointerOver)
                OnCursorEndDrag();
        }
    }
}