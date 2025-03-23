using UnityEngine;

namespace Reconnect.Physics
{
    public class InBaseCollider : MonoBehaviour, IAirlockCollider
    {
        public void CollisionHandle(PhysicsScript physics)
        {
            physics.SetInBase(true);
        }
    }
}