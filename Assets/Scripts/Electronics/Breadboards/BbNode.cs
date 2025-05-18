using Reconnect.MouseEvents;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class BbNode : MonoBehaviour, ICursorHandle
    {
        bool ICursorHandle.IsPointerDown { get; set; }
        bool ICursorHandle.IsPointerOver { get; set; }
        
        [SerializeField]
        private Vector2Int point;

        // Returns the parent breadboard
        [SerializeField]
        private Breadboard breadboard;

        private Outline _outline;

        private void Start()
        {
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("Outline component not found in this BbNode.");
            _outline.enabled = false;
        }

        void ICursorHandle.OnCursorEnter()
        {
            _outline.enabled = true;
            breadboard.OnMouseNodeCollision(point);
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }

        void ICursorHandle.OnCursorDown()
        {
            breadboard.StartWire(point);
        }
        
        void ICursorHandle.OnCursorUp()
        {
            breadboard.EndWire();
        }
    }
}