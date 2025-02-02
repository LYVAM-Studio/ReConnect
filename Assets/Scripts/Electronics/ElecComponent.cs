using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.Electronics
{
    public class ElecComponent : MonoBehaviour
    {
        [Tooltip("Distance between the camera and the breadboard.")]
        public ComponentType type;
        
        [Tooltip("Distance between the camera and the breadboard.")]
        public float distance = 8;
        
        [Header("Poles management")]
        [Tooltip("The vector from the center of gravity of this object to the main pole position used for the breadboard positioning.")]
        public Vector2 mainPoleAnchor;
        
        [Tooltip("The poles coordinates. Note that the first pole is considered as the main one. The y axis is from bottom to top like in the Unity editor.")]
        public Point[] poles;
        
        // The last position occupied by this component
        private Vector3 _startPosition;
        // The main camera. Avoids to access it everytime a position is calculated.
        private Camera _cam;
        // The component responsible for the outlines
        private Outline _outline;
        // The distance between the cursor and the center of this object
        private Vector3 _deltaCursor;
        
        [Serializable]
        public class Point
        {
            // The coordinates in the Unity editor
            public int x, y;
            // The coordinates on a breadboard
            public int GetH() => -y;
            public int GetW() => x;
        }

        private void Start()
        {
            _cam = Camera.main;
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        void OnMouseDown()
        {
            _startPosition = transform.localPosition;
            _deltaCursor = transform.localPosition - ElecHelper.GetFlattedCursorPos();
        }

        void OnMouseUp()
        {
            transform.localPosition = IsOutsideOfBreadboard()
                ? _startPosition // return to the previous pos if invalid
                : GetNearestValidPos(); // otherwise go to the nearest slot
        }
        
        void OnMouseDrag()
        {
            transform.localPosition = ElecHelper.GetFlattedCursorPos() + _deltaCursor;
        }

        private void OnMouseEnter()
        {
            _outline.enabled = true;
        }
        
        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        

        // Returns whether this object is in the bounds of the breadboard ot not.
        private bool IsOutsideOfBreadboard()
        {
            // Returns true if any of the poles is outside the breadboard
            return poles.Any(pole =>
                transform.localPosition.x + mainPoleAnchor.x + pole.x is < -4.25f or > 4.25f
                || transform.localPosition.y + mainPoleAnchor.y + pole.y is < -4.25f or > 4.25f);
        }
        
        // Assuming that this object is not out of the bounds of the breadboard, returns the nearest valid position.
        private Vector3 GetNearestValidPos()
        {
            return new Vector3(
                NearestHalf(transform.localPosition.x + mainPoleAnchor.x) - mainPoleAnchor.x,
                NearestHalf(transform.localPosition.y + mainPoleAnchor.y) - mainPoleAnchor.y,
                0);
        }

        // Returns the nearest n such that n = k + 0.5 with k an integer. In other words, rounds the given value to the half.
        private float NearestHalf(float x) => (float)Math.Round(x - 0.5f) + 0.5f;

        public (int h, int w) GetNormalizedPos()
        {
            return ((int)Math.Round(transform.localPosition.y + mainPoleAnchor.x + 3.5f),
                (int)Math.Round(-(transform.localPosition.x + mainPoleAnchor.y) + 3.5f));
        }

    }

}

