using System;
using Reconnect.Electronics.Graphs;
using Reconnect.MouseEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Electronics.Breadboards
{
    public class Dipole : MonoBehaviour, IDipole, ICursorHandle
    {
        public Breadboard Breadboard { get; set; }
        public Vector2Int Pole1 { get; set; }
        public Vector2Int Pole2 { get; set; }
        bool ICursorHandle.IsPointerDown { get; set; }
        public Vector3 MainPoleAnchor => _isHorizontal ? new Vector3(-0.5f, 0, 0) : new Vector3(0, 0.5f, 0);

        // Whether this object is rotated or not
        private bool _isHorizontal;
        
        // Whether this object is rotated or not
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set
            {
                transform.localEulerAngles = value ? new Vector3(0, 0, 90) : Vector3.zero;
                _isHorizontal = value;
            }
        }

        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;
        
        // The last position occupied by this component
        private Vector3 _lastLocalPosition;
        
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
        
        // Control map
        private PlayerControls _controls;
        
        // Dragging status of the dipole
        private bool _isBeingDragged;
        
        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
            
            _controls = new PlayerControls();

            _controls.Breadboard.Rotate.performed += OnRotate;
        }

        private void Start()
        {
            _lastLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }
        
        private void OnDestroy()
        {
            _controls.Breadboard.Rotate.performed -= OnRotate;
        }
        
        void ICursorHandle.OnCursorEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }
        
        void ICursorHandle.OnCursorDown()
        {
            if (_isLocked) return;
            _isBeingDragged = true;
            _lastLocalPosition = transform.localPosition;
            _wasHorizontal = _isHorizontal;
            
            _deltaCursor = transform.position - Breadboard.breadboardHolder.GetFlattenedCursorPos();
        }
        
        void ICursorHandle.OnCursorUp()
        {
            if (_isLocked) return;
            EndDrag();
        }

        private void RollbackPosition()
        {
            // Restore the last valid position and rotation
            transform.localPosition = _lastLocalPosition;
            IsHorizontal = _wasHorizontal;
        }

        private void EndDrag()
        {
            _isBeingDragged = false;
            
            if (Breadboard.TryGetClosestValidPos(this, out var closest, out var newPole1, out var newPole2))
            {
                transform.localPosition = closest;
                Pole1 = newPole1;
                Pole2 = newPole2;
            }
            else
            {
                RollbackPosition();
            }
        }

        void ICursorHandle.OnCursorDrag()
        {
            if (_isLocked) return;
            transform.position = Vector3.MoveTowards(
                Breadboard.breadboardHolder.GetFlattenedCursorPos() + _deltaCursor,
                Breadboard.breadboardHolder.cam.transform.position,
                Breadboard.transform.lossyScale.x * 0.2f);
        }
        
        private void OnRotate(InputAction.CallbackContext context)
        {
            if (_isBeingDragged)
                IsHorizontal ^= true; // Toggles the rotation
        }

        private void Update()
        {
            if (!Breadboard.breadboardHolder.IsActive) // returns to last pos when the player exists the breadboard holder
                RollbackPosition();
        }
    }
}