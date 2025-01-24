using System;
using UnityEngine;

namespace Reconnect.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        private Outline _glowScript;
        protected void Start()
        {
            _glowScript = GetComponent<Outline>();
            _glowScript.enabled = false;
            _glowScript.OutlineWidth = 2.5f;
        }

        public abstract void Interact(GameObject player);
        public abstract bool CanInteract();
        
        // This method is called by the player when this interactable enters its range.
        public void OnEnterPlayerRange()
        {
            _glowScript.enabled = true;
        }

        // This method is called by the player when this interactable exits its range.
        public void OnExitPlayerRange()
        {
            _glowScript.enabled = false;
        }

        // This method is called from the player interaction detector script to inform this interactable that it is no longer the nearest.
        public void ResetNearest()
        {
            // Make it glow less
            _glowScript.OutlineWidth = 2.5f;
        }

        // This method is called from the player interaction detector script to inform this interactable that it is the nearest.
        public void SetNearest()
        {
            // Make it glow more
            _glowScript.OutlineWidth = 5;
        }
        
    }
}