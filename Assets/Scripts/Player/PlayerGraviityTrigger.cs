using System;
using Player;
using Reconnect.Physics;
using UnityEngine;

public class PlayerGraviityTrigger : MonoBehaviour
{
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("AirLock"))
        {
            Debug.Log("Player collided with the AirLock layer!");
            if (!TryGetComponent(out PhysicsScript physicsScript))
                throw new ArgumentException("No PhysicsScript found on this Player");
            if (!collision.gameObject.TryGetComponent(out IAirlockCollider airlockCollider))
                throw new ArgumentException("No Airlock collider found on this AirLock tagged object");
            airlockCollider.CollisionHandle(physicsScript);
        }
    }
}
