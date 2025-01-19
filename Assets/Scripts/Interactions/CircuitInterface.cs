using System;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class CircuitInterface : MonoBehaviour, IInteractable
    {
        public GameObject circuitInterface;
        private bool _isActive;

        private void Start()
        {
            // By default, the interface is closed
            _isActive = false;
            circuitInterface.SetActive(false);
        }
        

        public void Interact()
        {
            Debug.Log("Interacted");
            _isActive = !_isActive;
            circuitInterface.SetActive(_isActive);
        }

        // This object can always be interacted with.
        public bool CanInteract() => true;
        
        
        // Makes the cursor free to be moved and displayed on the screen
        private void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        // Makes the cursor locked at the center of the screen and hidden
        private void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}