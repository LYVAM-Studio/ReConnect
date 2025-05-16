using Electronics.Breadboards;
using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Physics;
using Reconnect.Utils;
using UnityEngine;

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
            bool succeeded = BbSolver.ExecuteCircuit(breadboardHolder.breadboard);
            if (succeeded)
            {
                Debug.Log("Success : DoAction");
                ElecComponent target = UidDictionary.Get<ElecComponent>(breadboardHolder.breadboard.TargetUid);
                target.DoAction();
            }
            else
            {
                Debug.Log("Failure : OnFailedExercise");
                breadboardHolder.breadboardSwitch.OnFailedExercise();
            }
        }
        
        [Command]
        public void CmdRequestUndoTargetAction(Uid targetId)
        {
            Debug.Log("Command undo action received by server");
            ElecComponent target = UidDictionary.Get<ElecComponent>(targetId);
            target.UndoAction();
        }

        [Command]
        public void CmdRequestSetPoles(NetworkIdentity dipoleIdentity, Vector2Int pole1, Vector2Int pole2)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
                throw new ComponentNotFoundException("No Dipole component has been found on the identity provided");
            dipole.Pole1 = pole1;
            dipole.Pole2 = pole2;
        }
        
        [Command]
        public void CmdSetDipolePosition(NetworkIdentity dipoleIdentity, Vector3 targetPos)
        {
            dipoleIdentity.transform.position = targetPos;
        }
        
        [Command]
        public void CmdSetDipoleLocalPosition(NetworkIdentity dipoleIdentity, Vector3 targetPos)
        {
            dipoleIdentity.transform.localPosition = targetPos;
        }
    }
}