using System;
using Electronics.Breadboards;
using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Physics;
using Reconnect.Utils;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Reconnect.Player
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField]
        protected Transform lookAtObject;

        public bool isLocked;

        protected PhysicsScript Physics;
        protected CharacterController CharacterController;
        protected PlayerControls PlayerControls;

        public virtual void Awake()
        {
            PlayerControls = new PlayerControls();
                
            isLocked = false;

            if (!TryGetComponent(out CharacterController))
                throw new ComponentNotFoundException("No CharacterController component has been found on the player.");
            if (!TryGetComponent(out Physics))
                throw new ComponentNotFoundException("No PhysicsScript component has been found on the player.");
        }

        public override void OnStartLocalPlayer()
        {
            FreeLookCamera.VirtualCamera.Follow = transform;
            FreeLookCamera.VirtualCamera.LookAt = lookAtObject;
        }
        
        [Command]
        public void CmdExecuteCircuit(NetworkIdentity bbHolderIdentity)
        {
            Debug.Log($"Command received by server");
            if (!bbHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
                throw new ComponentNotFoundException("No BreadboardHolder component has been found on the identity provided");
            RpcHandleCircuitResult(BbSolver.Instance.ExecuteCircuit(breadboardHolder.breadboard), bbHolderIdentity);
        }
        
        [Command]
        public void CmdExecuteTargetAction(int targetId)
        {
            Debug.Log($"Command do action received by server");
            Lamp target = UniqueIdDictionary.Instance.Get<Lamp>(targetId);
            Debug.Log($"before lamp is On : {target.LightBulb.isOn}");
            target.DoAction();
            Debug.Log($"after lamp is On : {target.LightBulb.isOn}");
        }
        
        [ClientRpc]
        private void RpcHandleCircuitResult(bool succeeded, NetworkIdentity bbHolderIdentity)
        {
            Debug.Log($"RPC received | execution {succeeded}");
            if (!bbHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
                throw new ComponentNotFoundException("No BreadboardHolder component has been found on the identity provided");
            if (succeeded)
                CmdExecuteTargetAction(breadboardHolder.breadboard.TargetID);
            else
                breadboardHolder.breadboardSwitch.OnFailedExercise();
        }

        [Command]
        public void CmdRequestSetPoles(NetworkIdentity dipoleIdentity, Vector2Int pole1, Vector2Int pole2)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
                throw new ComponentNotFoundException("No Dipole component has been found on the identity provided");
            dipole.Pole1 = pole1;
            dipole.Pole2 = pole2;
        }
    }
}