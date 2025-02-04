using System.Linq;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Dipole : MonoBehaviour
    {
        [Header("Poles management")]
        [Tooltip("The vector from the center of gravity of this object to the main pole position used for the breadboard positioning.")]
        public Vector2 mainPoleAnchor;
        [SerializeField]
        [Tooltip("The poles coordinates. Note that the first pole is considered as the main one. The y axis is from bottom to top like in the Unity editor.")]
        private Vector2[] poles = { new(0, 0), new(0, -1) };
        
        private Breadboard _breadboard;
        // The last position occupied by this component
        private Vector3 _lastPosition;
        // The component responsible for the outlines
        private Outline _outline;
        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;

        private void Start()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
            _breadboard = GameObject.FindObjectsByType<Breadboard>(FindObjectsSortMode.None)[0]; // TODO: create a better and more efficient link
        }

        public Pole[] GetPoles(Vector2 position)
        {
            var poleList = from p in poles select Pole.PositionToPole(position + p + mainPoleAnchor + new Vector2(3.5f, -3.5f));
            return poleList.ToArray();
        }

        public Pole[] GetPoles() => GetPoles((Vector2)transform.position);
        
        

        void OnMouseDown()
        {
            _lastPosition = transform.position;
            _deltaCursor = transform.position - ElecHelper.GetFlattedCursorPos();
        }

        void OnMouseUp()
        {
            var pos = _breadboard.GetClosestValidPosition(this);
            if (pos is null)
            {
                // return to the previous pos if invalid
                transform.position = _lastPosition;
            }
            else
            {
                // otherwise go to the nearest slot
                transform.position = (Vector3)pos;
            }
        }
        
        void OnMouseDrag()
        {
            transform.position = ElecHelper.GetFlattedCursorPos() + _deltaCursor;
        }

        private void OnMouseEnter()
        {
            _outline.enabled = true;
        }
        
        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

    }

}

