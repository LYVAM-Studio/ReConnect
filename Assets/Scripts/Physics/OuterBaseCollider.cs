using UnityEngine;

namespace Reconnect.Physics
{
    public class OuterBaseCollider : MonoBehaviour, IAirlockCollider
    {
        public void CollisionHandle(PhysicsScript physics)
        {
            physics.SetInBase(false);
        }
    }
}