using System;
using Mirror;
using Scene;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerNetwork : NetworkBehaviour
    {
        public bool isLocked;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        protected CharacterController CharacterController;
        protected CinemachineCamera FreeLookCamera;
        protected Transform LookAtObject;

        protected PlayerInput PlayerInput;

        // imported components
        protected SceneScript SceneScript;

        public virtual void Awake()
        {
            isLocked = false;

            //Locking the cursor to the middle of the screen and making it invisible
            Cursor.lockState = CursorLockMode.Locked;

            //allow all players to run this
            SceneScript = FindFirstObjectByType<SceneScript>(); // Changed the deprecated FindObjectOfType

            // get the FreeLookCamera game object
            FreeLookCamera = FindFirstObjectByType<CinemachineCamera>() ??
                             throw new ArgumentException(
                                 "There is no freeLook Camera in this Scene");
            // get the object to look at
            LookAtObject = gameObject.FindComponentsInChildrenWithName<Transform>("LookAt")[0] ??
                           throw new ArgumentException(
                               "There is no LookAt named gameObject in the children of the current GameObject");
            PlayerInput = GetComponent<PlayerInput>();

            if (!isLocalPlayer) PlayerInput.actions["UnlockCursor"].started += OnEscape;

            CharacterController = GetComponent<CharacterController>();
        }

        public virtual void OnDestroy()
        {
            // It's a good practice to unsubscribe from actions when the object is destroyed
            if (!isLocalPlayer) PlayerInput.actions["UnlockCursor"].started -= OnEscape;
        }

        // TODO : this is a debug capability
        private void OnEscape(InputAction.CallbackContext context)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        [Command]
        public void CmdSendPlayerMessage()
        {
            if (SceneScript)
                SceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        }

        [Command]
        public void CmdSetupPlayer(string name, Color col)
        {
            //player info sent to server, then server updates sync vars which handles it on all clients
            playerName = name;
            playerColor = col;
            SceneScript.statusText = $"{playerName} joined.";
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
            SceneScript.playerNetwork = this;

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