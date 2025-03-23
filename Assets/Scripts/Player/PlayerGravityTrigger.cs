using System;
using Player;
using Reconnect.Physics;
using UnityEngine;

public class PlayerGravityTrigger : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("AirLock"))
        {
            Debug.Log("Player collided with the AirLock layer!");
            if (!TryGetComponent(out PhysicsScript physicsScript))
                throw new ArgumentException("No PhysicsScript found on this Player");
            if (!other.gameObject.TryGetComponent(out IAirlockCollider airlockCollider))
                throw new ArgumentException("No Airlock collider found on this AirLock tagged object");
            airlockCollider.CollisionHandle(physicsScript);
        }
    }
}
