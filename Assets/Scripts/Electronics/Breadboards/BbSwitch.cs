using System;
using Electronics.Breadboards;
using Mirror;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Menu;
using Reconnect.MouseEvents;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class BbSwitch : NetworkBehaviour, ICursorHandle
    {
        bool ICursorHandle.IsPointerDown { get; set; }
        bool ICursorHandle.IsPointerOver { get; set; }
        
        private Animator _animator;
        public Breadboard breadboard;

        // The component responsible for the outlines
        private Outline _outline;

        private int _isOnHash;
        public bool IsOn
        {
            get => _animator.GetBool(_isOnHash);
            set => _animator.SetBool(_isOnHash, value);
        }
        [SyncVar]
        public NetworkIdentity lastPlayerExecuting;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator is null)
                throw new ComponentNotFoundException("No Animator component has been found on the switch");
            _isOnHash = Animator.StringToHash("isON");
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("No Outline component has been found on the switch.");
            _outline.enabled = false;
            if (!_animator.TryGetComponent(out BbSwitchAnimation childrenAnimationScript))
                throw new ComponentNotFoundException(
                    "no BbSwitchAnimation component has been found in the switch prefab children");
            childrenAnimationScript.bbSwitch = this;
        }

        void ICursorHandle.OnCursorEnter()
        {
            _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }

        void ICursorHandle.OnCursorClick()
        {
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
            playerNetwork.CmdSetSwitchAnimation(netIdentity, !IsOn);
        }

        public void OnSwitchStartUp()
        {
            if (!isClient) return;
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
            playerNetwork.CmdRequestUndoTargetAction(breadboard.TargetUid);
        }
        
        public void OnSwitchIdleDown()
        {
            if (!isServer)
                return;
            ExecuteCircuit();
        }
        
        private void ExecuteCircuit()
        {
            if (lastPlayerExecuting is null)
                throw new UnreachableCaseException("The Breadboard Switch cannot be down without anyone clicking it");
            if (!lastPlayerExecuting.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException(
                    "No PlayerNetwork found on the localPlayer gameObject");
            
            BreadboardResult result = BbSolver.ExecuteCircuit(breadboard);
            switch (result)
            {
                case BreadboardResult.ShortCircuit:
                    playerNetwork.TargetKnockOut("You have been electrocuted because you have short-circuited the whole circuit.");
                    IsOn = false;
                    break;
                case BreadboardResult.Success :
                    ElecComponent target = UidDictionary.Get<ElecComponent>(breadboard.TargetUid);
                    target.DoAction();
                    break;
                case BreadboardResult.Failure :
                    IsOn = false;
                    break;
            }
        }
    }
}