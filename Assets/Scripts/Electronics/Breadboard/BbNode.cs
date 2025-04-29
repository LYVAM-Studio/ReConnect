using System.Collections.Generic;
using Reconnect.MouseHover;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.Electronics.Breadboards
{
    public class BbNode : MonoBehaviour, IMouseInteractable
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

        public void OnHoverEnter()
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

        public void OnHoverExit()
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
        
        private static bool IsPointerOverUI()
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