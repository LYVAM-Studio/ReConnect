using System;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class BbNodeScript : MonoBehaviour
    {
        private Breadboard _breadboard;

        public Breadboard Breadboard
        {
            // Returns the parent breadboard
            get
            {
                if (_breadboard is null)
                {
                    _breadboard = GetComponentInParent<Breadboard>();
                    if (_breadboard is null)
                        throw new Exception(
                            $"Breadboard not found by BreadboardNode at position ({transform.position.x}, {transform.position.y}, {transform.position.z}).");
                }
                return _breadboard;
            }
        }
        private void OnMouseDown()
        {
            Breadboard.StartWire(transform.position);
        }

        private void OnMouseUp()
        {
            Breadboard.EndWire();
        }

        private void OnMouseEnter()
        {
            Breadboard.OnNodeCollision(transform.position);
        }
    }
}
