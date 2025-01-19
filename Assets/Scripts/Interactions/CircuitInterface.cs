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
            _isActive = false;
            circuitInterface.SetActive(false);
        }


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

        public void Interact()
        {
            Debug.Log("Interacted");
            if (_isActive)
            {
                _isActive = false;
                circuitInterface.SetActive(false);
                HideCursor();
            }
            else
            {
                _isActive = true;
                circuitInterface.SetActive(true);
                ShowCursor();
            }
        }

        public bool CanInteract()
        {
            return true;
        }
    }
}