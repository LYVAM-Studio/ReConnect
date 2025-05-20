using UnityEngine;

namespace Reconnect.Physics
{
    public class PhysicsScript : MonoBehaviour
    {
        // parameters that can be edited in Editor
        [Header("In Base gravity force constant")]
        public readonly float InBaseGravity = -9.81f; // gravity strength (default: -9.81, earth gravity)

        [Header("Outer Base gravity force constant")]
        public readonly float
            OuterBaseGravity = -5f; // gravity strength outside the base (lower to have the moon effect)

        private bool _isInBase = true;
        public float Gravity => _isInBase ? InBaseGravity : OuterBaseGravity;

        public void ToggleGravity() => _isInBase = !_isInBase;
        public void SetInBase(bool value) => _isInBase = value;
    }
}