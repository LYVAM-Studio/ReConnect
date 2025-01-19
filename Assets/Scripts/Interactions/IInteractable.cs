using UnityEngine;

namespace Reconnect.Interactions
{
    public interface IInteractable
    {
        public void Interact();
        public bool CanInteract();
    }
}