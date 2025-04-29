using UnityEngine;
using Reconnect.Electronics.Breadboards;
using Reconnect.MouseHover;


namespace Reconnect.Electronics.Components
{
    public class WireScript : MonoBehaviour, IDipole, IMouseInteractable
    {
        public Breadboard Breadboard { get; set; }

        public Vector2Int Pole1 { get; set; }
        public Vector2Int Pole2 { get; set; }

        private Outline _outline;

        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set 
            {
                _isLocked = value;
                if (_isLocked) _outline.enabled = false;
            }
        }

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        public void OnHoverEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        public void OnHoverExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUpAsButton()
        {
            if (!_isLocked)
                Breadboard.DeleteWire(this);
        }
    }
}