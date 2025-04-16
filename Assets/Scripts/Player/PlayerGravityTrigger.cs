using System;
using Reconnect.Physics;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerGravityTrigger : MonoBehaviour
    {
        private PhysicsScript _physicsScript;

        private void Start()
        {
            _physicsScript = GetComponentInParent<PhysicsScript>();
            if (_physicsScript is null)
                throw new ArgumentException("The PhysicsScript has not been found in this player.");
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("AirLock"))
            {
                if (!other.gameObject.TryGetComponent(out IAirlockCollider airlockCollider))
                    throw new ArgumentException("No Airlock collider found on this AirLock tagged object");
                airlockCollider.CollisionHandle(_physicsScript);
                Debug.Log(-_physicsScript.Gravity);
            }
        }
    }
}
