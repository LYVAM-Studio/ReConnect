using System;
using Mirror;
using Reconnect.Physics;
using Reconnect.Utils;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Reconnect.Player
{
    public class PlayerNetwork : NetworkBehaviour
    {
        protected PhysicsScript Physics;

        public bool isLocked;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        protected CharacterController CharacterController;
        protected CinemachineCamera FreeLookCamera;
        protected Transform LookAtObject;

        protected PlayerInput PlayerInput;

        public virtual void Awake()
        {
            isLocked = false;

            //Locking the cursor to the middle of the screen and making it invisible
            Cursor.lockState = CursorLockMode.Locked;

            // //allow all players to run this
            // SceneScript = FindFirstObjectByType<SceneScript>(); // Changed the deprecated FindObjectOfType

            // get the FreeLookCamera game object
            FreeLookCamera = FindFirstObjectByType<CinemachineCamera>() ??
                             throw new ArgumentException(
                                 "There is no freeLook Camera in this Scene");
            // get the object to look at
            LookAtObject = gameObject.FindComponentsInChildrenWithName<Transform>("LookAt")[0] ??
                           throw new ArgumentException(
                               "There is no LookAt named gameObject in the children of the current GameObject");
            PlayerInput = GetComponent<PlayerInput>();

            CharacterController = GetComponent<CharacterController>();

            if (!TryGetComponent(out Physics))
                throw new ArgumentException("No Physics Script found on the player !");
        }

        // [Command]
        // public void CmdSendPlayerMessage()
        // {
        //     if (SceneScript)
        //         SceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        // }

        [Command]
        public void CmdSetupPlayer(string name, Color col)
        {
            //player info sent to server, then server updates sync vars which handles it on all clients
            playerName = name;
            playerColor = col;
            // SceneScript.statusText = $"{playerName} joined.";
        }

        private void OnNameChanged(string old, string @new)
        {
            //not implemented
        }

        private void OnColorChanged(Color old, Color @new)
        {
            //not implemented
        }

        public override void OnStartLocalPlayer()
        {
            // SceneScript.playerNetwork = this;

            FreeLookCamera.Follow = transform;
            FreeLookCamera.LookAt = LookAtObject;

            var name = "Player" + Random.Range(100, 999);
            var color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            var playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = true;
        }
    }
}