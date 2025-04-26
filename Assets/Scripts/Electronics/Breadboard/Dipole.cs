using Reconnect.Electronics.Graphs;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class Dipole : MonoBehaviour, IDipole
    {
        public Breadboard Breadboard { get; set; }
        public Vector2Int Pole1 { get; set; }
        public Vector2Int Pole2 { get; set; }
        public Vector3 MainPoleAnchor => _isHorizontal ? new Vector3(-0.5f, 0, 0) : new Vector3(0, 0.5f, 0);

        // Whether this object is rotated or not
        private bool _isHorizontal;
        
        // Whether this object is rotated or not
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set
            {
                transform.eulerAngles = value ? new Vector3(0, 0, 90) : Vector3.zero;
                _isHorizontal = value;
            }
        }

        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;
        
        // The last position occupied by this component
        private Vector3 _lastPosition;
        
        // Whether this was rotated or not on its last position
        private bool _wasHorizontal;

        // The component responsible for the outlines
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
        
        public Vertex Inner { get; set; }
        
        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }
        
        private void OnMouseDown()
        {
            if (_isLocked) return;
            _lastPosition = transform.position;
            _wasHorizontal = _isHorizontal;
            _deltaCursor = transform.position - Breadboard.breadboardHolder.GetFlattenedCursorPos();
        }

        private void OnMouseDrag()
        {
            if (_isLocked) return;
            transform.position = Breadboard.breadboardHolder.GetFlattenedCursorPos() + _deltaCursor;
            if (Input.GetKeyDown(KeyCode.R)) // todo: use new input system
            {
                // Toggles the rotation
                IsHorizontal ^= true;
            }
        }

        private void OnMouseEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUp()
        {
            if (_isLocked) return;
            if (Breadboard.TryGetClosestValidPos(this, out var validPos, out var newPole1, out var newPole2))
            {
                transform.position = Breadboard.transform.rotation * validPos;
                Pole1 = newPole1;
                Pole2 = newPole2;
            }
            else
            {
                // Restore the last valid position and rotation
                transform.position = _lastPosition;
                IsHorizontal = _wasHorizontal;
            }
        }
    }
}