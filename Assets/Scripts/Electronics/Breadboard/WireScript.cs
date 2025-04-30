using UnityEngine;
using Reconnect.Electronics.Breadboards;
using Reconnect.MouseEvents;

namespace Reconnect.Electronics.Components
{
    public class WireScript : MonoBehaviour, IDipole, ICursorHandle
    {
        bool ICursorHandle.IsPointerDown { get; set; }
        
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

        void ICursorHandle.OnCursorEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }

        void ICursorHandle.OnCursorClick()
        {
            if (!_isLocked)
                Breadboard.DeleteWire(this);
        }
    }
}