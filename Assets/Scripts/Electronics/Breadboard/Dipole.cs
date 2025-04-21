using System;
using System.Linq;
using Reconnect.Electronics.Graphs;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
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

        public Breadboard Breadboard { get; set; }

        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;

        // Whether this object is rotated or not
        private bool _isHorizontal;

        // The last position occupied by this component
        private Vector3 _lastPosition;
        
        // Whether this was rotated or not on its last position
        private bool _lastRotation;

        // The component responsible for the outlines
        private Outline _outline;

        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set 
            {
                _isLocked = value;
                if (_isLocked) _outline.enabled = false;
            }
        }
        
        public Vertex Inner { get; set; }
        
        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void OnMouseDown()
        {
            if (_isLocked) return;
            _lastPosition = transform.position;
            _lastRotation = _isHorizontal;
            _deltaCursor = transform.position - ElecHelper.GetFlattedCursorPos();
        }

        private void OnMouseDrag()
        {
            if (_isLocked) return;
            transform.position = ElecHelper.GetFlattedCursorPos() + _deltaCursor;
            if (Input.GetKeyDown(KeyCode.R)) // todo: use new input system
            {
                // Toggles the rotation
                SetRotation(!_isHorizontal);
            }
        }

        private void OnMouseEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUp()
        {
            if (_isLocked) return;
            var pos = Breadboard.GetClosestValidPosition(this);
            if (pos is Vector3 validPos)
            {
                transform.position = validPos;
            }
            else
            {
                // Restore the last valid position and rotation
                transform.position = _lastPosition;
                SetRotation(_lastRotation);
            }
        }

        public Point[] GetPoles(Vector2 position)
        {
            var poleList = from p in poles select Point.VectorToPoint(position + p + mainPoleAnchor);
            return poleList.ToArray();
        }

        public Point[] GetPoles()
        {
            return GetPoles(transform.position);
        }

        public Point GetOtherPole(Point other) => Array.Find(GetPoles(), p => p != other);

        public void SetRotation(bool horizontal)
        {
            if (horizontal == _isHorizontal) return;

            if (horizontal)
            {
                _isHorizontal = true;
                poles[1] = new Vector2(1, 0);
                transform.eulerAngles = new Vector3(0, 0, 90);
                mainPoleAnchor = new Vector2(-0.5f, 0);
            }
            else
            {
                _isHorizontal = false;
                poles[1] = new Vector2(0, -1);
                transform.eulerAngles = Vector3.zero;
                mainPoleAnchor = new Vector2(0, 0.5f);
            }
        }

        public void SetPosition(Point pole1, Point pole2)
        {
            if (!Mathf.Approximately(((Vector2)pole1 - (Vector2)pole2).magnitude, 1))
                throw new Exception(
                    $"This dipole cannot be set between nodes {pole1} and {pole2} because the distance between them is not 1.");
            if (pole1.H == pole2.H)
            {
                // Horizontal
                if (pole1.W > pole2.W) pole1 = pole2; // Make the pole1 the leftmost point
                SetRotation(true);
            }
            else
            {
                // Vertical
                if (pole1.H > pole2.H) pole1 = pole2; // Make the pole1 the upmost point
                SetRotation(false);
            }

            transform.position = Point.PointToVector(pole1, Breadboard.zPositionDipoles) - (Vector3)mainPoleAnchor;
        }
    }
}