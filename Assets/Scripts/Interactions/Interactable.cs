using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        protected Outline Outline;

        protected void Start()
        {
            if (!TryGetComponent(out Outline))
                throw new ComponentNotFoundException("No Outline component has been found attached to this interactable.");
            Outline.enabled = false;
            Outline.OutlineWidth = 2.5f;
            Outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        public abstract void Interact(GameObject player);
        public abstract bool CanInteract();

        // This method is called by the player when this interactable enters its range.
        public void OnEnterPlayerRange()
        {
            Outline.enabled = true;
        }

        // This method is called by the player when this interactable exits its range.
        public void OnExitPlayerRange()
        {
            Outline.enabled = false;
        }

        // This method is called from the player interaction detector script to inform this interactable that it is no longer the nearest.
        public void ResetNearest()
        {
            // Make it glow less
            Outline.OutlineWidth = 2.5f;
        }

        // This method is called from the player interaction detector script to inform this interactable that it is the nearest.
        public void SetNearest()
        {
            // Make it glow more
            Outline.OutlineWidth = 5;
        }
    }
}