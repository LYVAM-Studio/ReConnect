using Unity.Cinemachine;
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
        
        public void Interact()
        {
            Debug.Log("Interacted");
            // Toggle state
            _isActive = !_isActive;
            // Update 
            circuitInterface.SetActive(_isActive);
            ToggleCursor();
            ToggleCameraInput();
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

    }
}