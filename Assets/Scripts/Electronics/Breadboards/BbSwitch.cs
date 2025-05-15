using System;
using Electronics.Breadboards;
using Mirror;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;
using Reconnect.MouseEvents;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Electronics.Breadboards
{
    public class BbSwitch : NetworkBehaviour, ICursorHandle
    {
        private Animator _animator;
        public Breadboard breadboard;
        bool ICursorHandle.IsPointerDown { get; set; }

        // The component responsible for the outlines
        private Outline _outline;

        private int _isOnHash;

        private bool IsOn
        {
            get => _animator.GetBool(_isOnHash);
            set => _animator.SetBool(_isOnHash, value);
        }

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
            Debug.Log($"BB switch bb is {breadboard}");
        }

        void ICursorHandle.OnCursorEnter()
        {
            _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }

        private void ToggleAnimation() => IsOn = !IsOn;
        
        void ICursorHandle.OnCursorClick()
        {
            ToggleAnimation();
        }

        public void OnSwitchStartUp()
        {
            UniqueIdDictionary.Instance.Get<ElecComponent>(breadboard.TargetID).UndoAction();
        }

        public void OnFailedExercise()
        {
            ToggleAnimation(); // automatic shutdown of the switch
            // TODO: handle KO of the player
        }
        
        public void OnSwitchIdleDown()
        {
            /*if (!ExecuteCircuit())
                OnFailedExercise();*/
            Debug.Log("End of animation");
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                throw new ComponentNotFoundException("No PlayerNetwork found on the local player");
            playerNetwork.CmdExecuteCircuit(netIdentity);
        }
    }
}