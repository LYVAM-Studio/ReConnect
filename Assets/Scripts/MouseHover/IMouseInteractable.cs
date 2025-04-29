namespace Reconnect.MouseHover
{
    public interface IMouseInteractable
    {
        public void OnHoverEnter() { }
        public void OnHoverExit() { }
        public void OnHover() { }
        public void OnClicked() { }
        public void OnReleased() { }
        public void OnDragged() { }
        public void OnClickedAsButton() { }
    }
}