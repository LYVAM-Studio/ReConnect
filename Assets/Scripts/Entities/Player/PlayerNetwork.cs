using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Game;
using Reconnect.Menu;
using Reconnect.Physics;
using Reconnect.ToolTips;
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
        public void CmdSetSwitchAnimation(NetworkIdentity bbHolderIdentity, bool value)
        {
            if (!bbHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No BreadboardHolder component has been found on the identity provided"));
                return;
            }
            breadboardHolder.breadboardSwitch.IsOn = value;
            breadboardHolder.breadboardSwitch.lastPlayerExecuting = connectionToClient.identity; // the player who sent the command
        }
        
        [Command]
        public void CmdRequestUndoTargetAction(Uid targetId)
        {
            ElecComponent target = UidDictionary.Get<ElecComponent>(targetId);
            target.UndoAction();
        }
        
        [Command]
        public void CmdSetDipolePosition(NetworkIdentity dipoleIdentity, Vector3 targetPos)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No Dipole component has been found on the identity provided"));
                return;
            }
            dipole.SetPosition(targetPos);
        }
        
        [Command]
        public void CmdSetHorizontalDipole(NetworkIdentity dipoleIdentity, bool value)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            dipole.IsHorizontal = value;
        }

        [Command]
        public void CmdDipoleEndDrag(NetworkIdentity dipoleIdentity)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            if (dipole.Breadboard.TryGetClosestValidPos(dipole, out var closest, out var newPole1, out var newPole2))
            {
                dipole.SetLocalPosition(closest);
                dipole.LastLocalPosition = closest;
                dipole.Pole1 = newPole1;
                dipole.Pole2 = newPole2;
                dipole.wasHorizontal = dipole.IsHorizontal;
            }
            else
            {
                dipole.SetLocalPosition(dipole.LastLocalPosition);
                dipole.IsHorizontal = dipole.wasHorizontal;
            }
        }
        
        [TargetRpc]
        public void TargetForceHideTooltip(NetworkIdentity dipoleIdentity)
        {
            if (dipoleIdentity.TryGetComponent(out HoverToolTip tooltip))
                tooltip.ForceHideUntilEndDrag();
        }
        
        [Command]
        public void CmdRequestCreateWire(NetworkIdentity breadboardHolderIdentity, Vector2Int sourcePoint, 
            Vector2Int destinationPoint, string wireName, bool isWireLocked)
        {
            if (!breadboardHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            if (breadboardHolder.breadboardSwitch.IsOn)
            {
                TargetKnockOut(
                    "You have been electrocuted because you tried to edit the circuit while it was still powered on.");
            }
            else
            {
                breadboardHolder.breadboard.CreateWire(sourcePoint, destinationPoint, wireName, isWireLocked);
            }
        }
        
        [Command]
        public void CmdRequestCreateResistor(NetworkIdentity breadboardHolderIdentity, Vector2Int sourcePoint, 
            Vector2Int destinationPoint, string resistorName, uint resistance, float tolerance, bool isResistorLocked)
        {
            if (!breadboardHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            breadboardHolder.breadboard.CreateResistor(sourcePoint, destinationPoint, resistorName, resistance, tolerance, isResistorLocked);
        }
        
        [Command]
        public void CmdRequestCreateLamp(NetworkIdentity breadboardHolderIdentity, Vector2Int sourcePoint, 
            Vector2Int destinationPoint, string lampName, uint resistance, bool isLampLocked)
        {
            if (!breadboardHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            breadboardHolder.breadboard.CreateLamp(sourcePoint, destinationPoint, lampName, resistance, isLampLocked);
        }
        
        [Command]
        public void CmdRequestDeleteWire(NetworkIdentity wireIdentity)
        {
            if (!wireIdentity.TryGetComponent(out WireScript wire))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No wireScript has been found on the network identity"));
                return;
            }
            wire.DeleteWire();
        }

        [Command]
        public void CmdOnBreadboardExit(NetworkIdentity breadboardHolderIdentity)
        {
            if (!breadboardHolderIdentity.TryGetComponent(out BreadboardHolder breadboardHolder))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }
            breadboardHolder.breadboard.Dipoles.ForEach(d => d.OnBreadBoardExit(connectionToClient));
        }
        
        [TargetRpc]
        public void TargetStopDragging(NetworkIdentity dipoleIdentity)
        {
            if (!dipoleIdentity.TryGetComponent(out Dipole dipole))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component Dipole found on the identity provided"));
                return;
            }

            dipole.isBeingDragged = false;
        }

        [Command]
        public void CmdKnockOutPlayer(string reason) => TargetKnockOut(reason);
        
        [TargetRpc]
        public void TargetKnockOut(string reason)
        {
            if (!TryGetComponent(out PlayerMovementsNetwork playerMovements))
            {
                Debug.LogException(
                    new ComponentNotFoundException("No component PlayerNetwork has been found on the local player"));
                return;
            }
            if (MenuManager.Instance.CurrentMenuState is MenuState.BreadBoard)
                MenuManager.Instance.BackToPreviousMenu();
            MenuManager.Instance.SetKnockOutReason(reason);
            playerMovements.KnockOut();
        }
        
        [Command]
        public void CmdGetPlayerLevel()
        {
            TargetSetPlayerLevel(GameManager.Level);
        }

        [TargetRpc]
        private void TargetSetPlayerLevel(uint level)
        {
            uint old = GameManager.Level;
            GameManager.Level = level;
            GameManager.OnLevelChange(old, level);
        }
        
        [Command]
        public void CmdSetPlayersLevel(uint level)
        {
            RpcExitBreadboardMenu();

            uint old = GameManager.Level;
            GameManager.Level = level;
            if (isClient)
                GameManager.OnLevelChange(old, GameManager.Level);
            GameManager.Instance.LevelTrigger(level - 1, this);
            RpcSetPlayerLevel(level);
        }

        [ClientRpc]
        private void RpcSetPlayerLevel(uint level)
        {
            if (isServer) return;
            uint old = GameManager.Level;
            GameManager.Level = level;
            GameManager.OnLevelChange(old, level);
        }

        [ClientRpc]
        public void RpcExitBreadboardMenu()
        {
            MenuManager.Instance.CorruptHistory();
        }

        [ClientRpc]
        public void RpcSetActiveGameObject(NetworkIdentity objectIdentity, bool value)
        {
            objectIdentity.gameObject.SetActive(value);
        }
        
        [ClientRpc]
        public void RpcSetEnabledAnimation(NetworkIdentity objectIdentity, bool value)
        {
            if (!objectIdentity.TryGetComponent(out Animation objectAnimation))
            {
                Debug.LogException(new ComponentNotFoundException($"No Animation component found on this object {objectIdentity.name}"));
                return;
            }

            objectAnimation.enabled = value;
        }
        
        [ClientRpc]
        public void RpcSetEnabledAudio(NetworkIdentity objectIdentity, bool value)
        {
            if (!objectIdentity.TryGetComponent(out AudioSource objectAudio))
            {
                Debug.LogException(new ComponentNotFoundException($"No AudioSource component found on this object {objectIdentity.name}"));
                return;
            }

            objectAudio.enabled = value;
        }
        
        [ClientRpc]
        public void RpcSetEnabledLight(NetworkIdentity objectIdentity, bool value)
        {
            if (!objectIdentity.TryGetComponent(out Light objectLight))
            {
                Debug.LogException(new ComponentNotFoundException($"No Light component found on this object {objectIdentity.name}"));
                return;
            }

            objectLight.enabled = value;
        }
    }
}