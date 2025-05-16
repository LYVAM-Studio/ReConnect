using Mirror;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.MouseEvents;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Electronics.Breadboards
{
    public class Dipole : ComponentSync, IDipole, ICursorHandle
    {
        public Breadboard Breadboard => breadboardNetIdentity != null
            ? breadboardNetIdentity.GetComponent<BreadboardHolder>().breadboard
            : null;
        
        [SyncVar]
        private Vector2Int _pole1;

        [SyncVar]
        private Vector2Int _pole2;

        public Vector2Int Pole1
        {
            get => _pole1;
            set
            {
                if (!isServer)
                    throw new UnauthorizedActionFromClientException("Client cannot set Pole1");
                _pole1 = value;
            }
        }

        public Vector2Int Pole2
        {
            get => _pole2;
            set
            {
                if (!isServer)
                    throw new UnauthorizedActionFromClientException("Client cannot set Pole2");
                _pole2 = value;
            }
        }

        bool ICursorHandle.IsPointerDown { get; set; }
        public Vector3 MainPoleAnchor => _isHorizontal ? new Vector3(-0.5f, 0, 0) : new Vector3(0, 0.5f, 0);
        
        // Whether this object is rotated or not
        [SyncVar]
        private bool _isHorizontal;
        
        // Whether this object is rotated or not
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set
            {
                if (!isServer)
                    throw new UnauthorizedActionFromClientException("Client cannot set IsHorizontal");
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

        [SyncVar]
        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set 
            {
                if (!isServer)
                    throw new UnauthorizedActionFromClientException("Client cannot set IsLocked");
                _isLocked = value;
                if (_isLocked) _outline.enabled = false;
            }
        }
        
        public Uid InnerUid { get; set; }
        
        // Control map
        private PlayerControls _controls;
        
        // Dragging status of the dipole
        private bool _isBeingDragged;
        
        private new void Awake()
        {
            base.Awake();
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("No Outline component found on this Dipole.");
            
            _outline.enabled = false;
            _controls = new PlayerControls();

            _controls.Breadboard.Rotate.performed += OnRotate;
        }

        private new void Start()
        {
            base.Start();
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
            Debug.Log(Breadboard);
            if (!_isLocked && !Breadboard.OnWireCreation)
                _outline.enabled = true;
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
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException(
                    "No PlayerNetwork component has been found on the local player");
            // Restore the last valid position and rotation
            playerNetwork.CmdSetDipoleLocalPosition(netIdentity, _lastLocalPosition);
            IsHorizontal = _wasHorizontal;
        }

        private void EndDrag()
        {
            _isBeingDragged = false;
            
            if (Breadboard.TryGetClosestValidPos(this, out var closest, out var newPole1, out var newPole2))
            {
                if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                    throw new ComponentNotFoundException(
                        "No PlayerNetwork component has been found on the local player");
                playerNetwork.CmdSetDipoleLocalPosition(netIdentity, closest);
                playerNetwork.CmdRequestSetPoles(netIdentity, newPole1, newPole2);
            }
            else
            {
                RollbackPosition();
            }
        }

        void ICursorHandle.OnCursorDrag()
        {
            Debug.Log("Cursor drag");
            if (_isLocked) return;
            Debug.Log("moves");
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
            playerNetwork.CmdSetDipolePosition(netIdentity,
                Vector3.MoveTowards(
                Breadboard.breadboardHolder.GetFlattenedCursorPos() + _deltaCursor,
                Breadboard.breadboardHolder.cam.transform.position,
                Breadboard.transform.lossyScale.x * 0.2f));
        }
        
        private void OnRotate(InputAction.CallbackContext context)
        {
            if (_isBeingDragged)
                IsHorizontal ^= true; // Toggles the rotation
        }

        // Returns to last pos when the player exists the breadboard holder
        public void OnBreadBoardExit()
        {
            if (_isBeingDragged)
                RollbackPosition();
        }
    }
}