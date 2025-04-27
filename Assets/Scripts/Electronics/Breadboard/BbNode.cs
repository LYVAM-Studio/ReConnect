using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.Electronics.Breadboards
{
    public class BbNode : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int point;

        // Returns the parent breadboard
        [SerializeField]
        private Breadboard breadboard;

        private Outline _outline;

        private void Start()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void OnMouseEnter()
        {
            _outline.enabled = true;

            if (IsPointerOverUI())
            {
                breadboard.EndWire();
            }
            else
            {
                breadboard.OnMouseNodeCollision(point);
            }
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseDown()
        {
            // The click is over a UI element with Raycast Target = true
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            breadboard.StartWire(point);
        }
        
        private void OnMouseUp()
        {
            breadboard.EndWire();
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