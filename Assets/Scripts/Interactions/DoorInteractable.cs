using Reconnect.Game;
using Reconnect.Physics;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class DoorInteractable : Interactable
    {
        [SerializeField] private Vector3 tpPos;
        
        public override void Interact(GameObject player)
        {
            if (!player.TryGetComponent(out CharacterController controller))
                throw new ComponentNotFoundException("No PlayerController has been found on the player.");
            if (!player.TryGetComponent(out PhysicsScript physicsScript))
                throw new ComponentNotFoundException("No component PhysicsScript has been found on the player");
            controller.enabled = false;
            player.transform.transform.transform.position = tpPos;
            controller.enabled = true;
            physicsScript.SetInBase(false); // outdoor gravity
        }

        public override bool CanInteract()
        {
            return GameManager.Level > 1;
        }
    }
}