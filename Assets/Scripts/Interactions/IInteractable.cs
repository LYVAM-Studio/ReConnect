using UnityEngine;

namespace Reconnect.Interactions
{
    public interface IInteractable
    {
        public void Interact(GameObject player);
        public bool CanInteract();
    }
}