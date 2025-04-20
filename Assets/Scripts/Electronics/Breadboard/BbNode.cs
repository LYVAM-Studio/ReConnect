using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Reconnect.Electronics.Breadboards
{
    public class BbNode : MonoBehaviour
    {
        private Point _point;
        private Breadboard _breadboard;

        // Returns the parent breadboard
        public Breadboard Breadboard
        {
            get
            {
                if (_breadboard is null)
                {
                    _breadboard = GetComponentInParent<Breadboard>();
                    if (_breadboard is null)
                        throw new Exception($"Breadboard not found by BreadboardNode at position ({transform.position.x}, {transform.position.y}, {transform.position.z}).");
                }

                return _breadboard;
            }
        }

        private void Start()
        {
            _point = Point.VectorToPoint(transform.position);
        }

        private void OnMouseDown()
        {
            // The click is over a UI element with Raycast Target = true
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            Breadboard.StartWire(transform.position, _point);
        }

        private void OnMouseEnter()
        {
            if (IsPointerOverUI())
            {
                Breadboard.EndWire();
            }
            else
            {

                Breadboard.OnMouseNodeCollision(transform.position, _point);
            }
        }

        private void OnMouseUp()
        {
            Breadboard.EndWire();
        }
        
        private bool IsPointerOverUI()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results.Count > 0;
        }
    }
}