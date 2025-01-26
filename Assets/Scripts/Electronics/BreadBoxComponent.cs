using System;
using System.Linq;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class BreadBoxComponent : MonoBehaviour
    {
        [Tooltip("Distance between the camera and the breadboard.")]
        public float distance = 8;
        
        [Header("Poles management")]
        [Tooltip("The vector from the center of gravity of this object to the main pole position used for the breadboard positioning.")]
        public Vector2 mainPoleAnchor;
        [Tooltip("The poles coordinates. Note that the first pole is considered as the main one.")]
        public Point[] poles;
        
        // Whether this object is being dragged or not
        private bool _isDragging;
        private Vector3 _startPosition;
        
        // The main camera. Avoids to access it everytime a position is calculated.
        private Camera _cam;
        
        // The component responsible for the outlines
        private Outline _outline;
        
        [Serializable]
        public class Point
        {
            public float x, y;
        }

        private void Start()
        {
            _isDragging = false;
            _cam = Camera.main;
            _outline = GetComponent<Outline>();
            if (_outline is not null) _outline.enabled = false;
        }

        void OnMouseDown()
        {
            _startPosition = transform.localPosition;
            _isDragging = true;
        }

        void OnMouseUp()
        {
            _isDragging = false;
            transform.localPosition = IsOutsideOfBreadboard()
                ? _startPosition // return to the previous pos if invalid
                : GetNearestValidPos(); // otherwise go to the nearest slot
        }

        private void OnMouseEnter()
        {
            if (_outline is not null) _outline.enabled = true;
        }
        
        private void OnMouseExit()
        {
            if (_outline is not null) _outline.enabled = false;
        }

        void Update() // OnMouseDrag?
        {
            if (_isDragging)
                transform.localPosition = GetFlattedCursorPos();
        }

        // Gets the position of the cursor projected on the breadboard plane. This vector's z component is therefore always 0.
        private Vector3 GetFlattedCursorPos()
        {
            if (_cam is null)
                throw new NullReferenceException("No main camera has been found for the drag and drop script.");
            
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            var rayDirection = ray.direction;
            return new Vector3(
                rayDirection.x / rayDirection.z * distance,
                rayDirection.y / rayDirection.z * distance,
                0);
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

    }

}

