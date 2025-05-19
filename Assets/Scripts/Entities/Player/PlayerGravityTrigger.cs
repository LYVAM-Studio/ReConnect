using Reconnect.Physics;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Player
{
    public class PlayerGravityTrigger : MonoBehaviour
    {
        [SerializeField]
        private PhysicsScript physicsScript;

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("AirLock"))
            {
                if (!other.gameObject.TryGetComponent(out IAirlockCollider airlockCollider))
                    throw new ComponentNotFoundException("No Airlock collider found on this AirLock tagged object");
                airlockCollider.CollisionHandle(physicsScript);
                // Debug.Log(-_physicsScript.Gravity);
            }
        }
    }
}