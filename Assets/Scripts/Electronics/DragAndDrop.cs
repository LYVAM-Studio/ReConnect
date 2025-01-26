using System;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class DragAndDrop : MonoBehaviour
    {
        [Tooltip("Distance between the camera and the breadboard.")]
        public float distance = 8;
        
        [Tooltip("Difference between the x coordinate of this object and the x coordinate of its main pole.")]
        public float xDistanceToMainPole = 0;
        [Tooltip("Difference between the y coordinate of this object and the y coordinate of its main pole.")]
        public float yDistanceToMainPole = 0.5f;
        
        // The vector form the real unity center of gravity of this object to the anchor position used for the breadboard positioning.
        private Vector3 _mainPoleAnchor;
        
        // Whether this object is being dragged or not
        private bool _isDragging;
        private Vector3 _startPosition;
        
        // The main camera. Avoids to access it everytime a position is calculated.
        private Camera _cam;

        private void Start()
        {
            _mainPoleAnchor = new Vector3(xDistanceToMainPole, yDistanceToMainPole, 0);
            _isDragging = false;
            _cam = Camera.main;
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

        void Update()
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

        // Returns whether this object is in the bounds of the breadboard.
        private bool IsOutsideOfBreadboard()
            => transform.localPosition.x + _mainPoleAnchor.x is < -4.25f or > 4.25f
               || transform.localPosition.y + _mainPoleAnchor.y is < -4.25f or > 4.25f;
        
        private Vector3 GetNearestValidPos()
        {
            return new Vector3(
                NearestHalf(transform.localPosition.x + _mainPoleAnchor.x) - _mainPoleAnchor.x,
                NearestHalf(transform.localPosition.y + _mainPoleAnchor.y) - _mainPoleAnchor.y,
                0);
        }

        private float NearestHalf(float x) => (float)Math.Round(x - 0.5f) + 0.5f;

    }

}

