using System;
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
        bool ICursorHandle.IsPointerDown { get; set; }
        bool ICursorHandle.IsPointerOver { get; set; }
        
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
                {
                    Debug.LogException(new UnauthorizedActionFromClientException("Client cannot set Pole1"));
                    return;
                }
                _pole1 = value;
            }
        }

        public Vector2Int Pole2
        {
            get => _pole2;
            set
            {
                if (!isServer)
                {
                    Debug.LogException(new UnauthorizedActionFromClientException("Client cannot set Pole2"));
                    return;
                }
                _pole2 = value;
            }
        }

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
                {
                    Debug.LogException(new UnauthorizedActionFromClientException("Client cannot set IsHorizontal"));
                    return;
                }
                transform.localEulerAngles = value ? new Vector3(0, 0, 90) : Vector3.zero;
                _isHorizontal = value;
            }
        }

        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;
        
        // The last position occupied by this component
        [NonSerialized]
        public Vector3 LastLocalPosition;
        
        // Whether this was rotated or not on its last position
        [SyncVar]
        public bool wasHorizontal;

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
                {
                    Debug.LogException( new UnauthorizedActionFromClientException("Client cannot set IsLocked"));
                    return;
                }
                _isLocked = value;
                if (_isLocked) _outline.enabled = false;
            }
        }
        public Uid InnerUid { get; set; }
        
        // Control map
        private PlayerControls _controls;
        
        // Dragging status of the dipole
        [SyncVar]
        public bool isBeingDragged;
        
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
            LastLocalPosition = transform.localPosition;
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
            if (!_isLocked && !Breadboard.OnWireCreation)
                _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }
        
        void ICursorHandle.OnCursorDown()
        {
            if (Breadboard.IsCircuitOn)
            {
                Breadboard.KnockOutOnEdit();
                return;
            }
            
            if (_isLocked) return;

            isBeingDragged = true;
            _deltaCursor = transform.position - Breadboard.breadboardHolder.GetFlattenedCursorPos();
        }
        
        void ICursorHandle.OnCursorUp()
        {
            if (_isLocked) return;
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException(
                    "No PlayerNetwork component has been found on the local player");
            playerNetwork.CmdDipoleEndDrag(netIdentity);
            isBeingDragged = false;
        }

        public void SetLocalPosition(Vector3 value) => transform.localPosition = value;
        public void SetPosition(Vector3 value) => transform.position = value;

        void ICursorHandle.OnCursorDrag()
        {
            if (_isLocked) return;
            if (!isBeingDragged) return;
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
            if (isBeingDragged)
            {
                if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                    throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
                playerNetwork.CmdSetHorizontalDipole(netIdentity, !IsHorizontal);
            }
        }

        // Returns to last pos when the player exists the breadboard holder
        public void OnBreadBoardExit(NetworkConnection clientConnection)
        {
            SetLocalPosition(LastLocalPosition);
            IsHorizontal = wasHorizontal;
            if (!clientConnection.identity.TryGetComponent(out PlayerNetwork playerNetwork))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No PlayerNetwork component has been found on the client player"));
                return;
            }
            playerNetwork.TargetForceHideTooltip(netIdentity);
            playerNetwork.TargetStopDragging(netIdentity);
        }
    }
}