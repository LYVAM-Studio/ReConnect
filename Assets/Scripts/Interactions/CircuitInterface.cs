using Mirror;
using Player;
using Reconnect.Player;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class CircuitInterface : MonoBehaviour, IInteractable
    {
        [Tooltip("The interface that will be opened and closed when the player interacts.")]
        public GameObject circuitInterface;
        
        private bool _isActive;

        private CinemachineInputAxisController _cinemachineInputAxisController;
        
        public void Awake()
        {
            _cinemachineInputAxisController = GameObject.FindGameObjectWithTag("freeLookCamera")
                .GetComponent<CinemachineInputAxisController>();
        }
        
        private void Start()
        {
            // By default, the interface is closed
            _isActive = false;
            circuitInterface.SetActive(false);
        }
        
        public void Interact(GameObject player)
        {
            Debug.Log("Interacted");
            // Toggle state
            _isActive = !_isActive;
            // Update 
            circuitInterface.SetActive(_isActive);
            ToggleCursor();
            ToggleCameraInput();
            TogglePlayerLock(player);
        }

        // This object can always be interacted with.
        public bool CanInteract() => true;
        
        // Shows the cursor if true, otherwise hides it.
        private void ToggleCursor()
        {
            Cursor.visible = _isActive;
            Cursor.lockState = _isActive ? CursorLockMode.Confined : CursorLockMode.Locked;
        }

        private void ToggleCameraInput()
        {
            _cinemachineInputAxisController.enabled = !_isActive;
        }

        private void TogglePlayerLock(GameObject player)
        {
            player.GetComponent<PlayerNetwork>().isLocked = _isActive;
        }

    }
}