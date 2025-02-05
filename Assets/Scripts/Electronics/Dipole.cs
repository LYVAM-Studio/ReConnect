using System.Linq;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Dipole : MonoBehaviour
    {
        [Header("Poles management")]
        [Tooltip(
            "The vector from the center of gravity of this object to the main pole position used for the breadboard positioning.")]
        public Vector2 mainPoleAnchor = new(0, 0.5f);

        [SerializeField]
        [Tooltip(
            "The poles coordinates. Note that the first pole is considered as the main one. The y axis is from bottom to top like in the Unity editor.")]
        private Vector2[] poles = { new(0, 0), new(0, -1) };

        private Breadboard _breadboard;

        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;

        // Whether this object is rotated or not
        private bool _isRotated;

        // The last position occupied by this component
        private Vector3 _lastPosition;

        // The component responsible for the outlines
        private Outline _outline;

        private void Start()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
            _breadboard =
                FindObjectsByType<Breadboard>(FindObjectsSortMode.None)
                    [0]; // TODO: create a better and more efficient link
        }


        private void OnMouseDown()
        {
            _lastPosition = transform.position;
            _deltaCursor = transform.position - ElecHelper.GetFlattedCursorPos();
        }

        private void OnMouseDrag()
        {
            transform.position = ElecHelper.GetFlattedCursorPos() + _deltaCursor;
            if (Input.GetKeyDown(KeyCode.R)) // todo: use new input system
            {
                if (_isRotated)
                {
                    _isRotated = false;
                    poles[1] = new Vector2(0, -1);
                    transform.eulerAngles = Vector3.zero;
                    mainPoleAnchor = new Vector2(0, 0.5f);
                }
                else
                {
                    _isRotated = true;
                    poles[1] = new Vector2(1, 0);
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    mainPoleAnchor = new Vector2(-0.5f, 0);
                }
            }
        }

        private void OnMouseEnter()
        {
            _outline.enabled = true;
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUp()
        {
            transform.position = _breadboard.GetClosestValidPosition(this, _lastPosition);
        }

        public Pole[] GetPoles(Vector2 position)
        {
            var poleList = from p in poles select Pole.PositionToPole(position + p + mainPoleAnchor);
            return poleList.ToArray();
        }

        public Pole[] GetPoles()
        {
            return GetPoles(transform.position);
        }
    }
}