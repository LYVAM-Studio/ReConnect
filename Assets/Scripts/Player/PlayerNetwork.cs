using System;
using Mirror;
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
    }
}