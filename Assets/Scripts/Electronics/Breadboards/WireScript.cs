using System;
using Mirror;
using UnityEngine;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.MouseEvents;
using Reconnect.Player;
using Reconnect.Utils;

namespace Reconnect.Electronics.Components
{
    public class WireScript : ComponentSync, IDipole, ICursorHandle
    {
        bool ICursorHandle.IsPointerDown { get; set; }
        
        public Breadboard Breadboard => breadboardNetIdentity != null
            ? breadboardNetIdentity.GetComponent<BreadboardHolder>().breadboard
            : null;

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
        
        private new void Awake()
        {
            base.Awake();
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("No Outline component found on this WireScript.");
            
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
            {
                if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                    throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
                playerNetwork.CmdRequestDeleteWire(netIdentity);
            }
        }
        
        public void DeleteWire()
        {
            Breadboard.Wires.Remove(this);
            NetworkServer.Destroy(gameObject);
        }
    }
}