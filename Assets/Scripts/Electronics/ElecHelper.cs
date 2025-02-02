using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reconnect.Electronics
{
    public static class ElecHelper
    {
        [CanBeNull] private static Camera _cam;

        // Converts the given position in the unity editor of an object to the corresponding position on the breadboard.
        // The breadboard is supposed to be centered on the x-axis and y-axis. The z component of the position of any electrical component on it should be constant. 
        public static (int h, int w) UnityToBreadboardPosConvert(float x, float y)
        {
            int h = (int)Math.Round(x + 3.5f);
            int w = (int)Math.Round(y + 3.5f);
            return (h, w);
        }
        
        public static (int h, int w) UnityToBreadboardPosConvert(Vector2 vector2)
            => UnityToBreadboardPosConvert(vector2.y, -vector2.x);

        public static (int h, int w) UnityToBreadboardPosConvert(Transform transform)
            => UnityToBreadboardPosConvert(
                transform.localPosition.y,
                -transform.localPosition.x);

        public static (int h, int w) UnityToBreadboardPosConvert(Transform transform, Vector2 mainPoleAnchor)
            => UnityToBreadboardPosConvert(
                transform.localPosition.y + mainPoleAnchor.x,
                -transform.localPosition.x + mainPoleAnchor.y);

        // Gets the position of the cursor projected on the breadboard plane. This vector's z component is therefore always 0.
        public static Vector3 GetFlattedCursorPos(float distanceCamBreadboard = 8f)
        {
            // Set the cam if Camera.main is not null
            _cam ??= Camera.main;
            
            if (_cam is null)
                throw new NullReferenceException("No main camera has been found for the drag and drop script.");
            
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            var rayDirection = ray.direction;
            return new Vector3(
                rayDirection.x / rayDirection.z * distanceCamBreadboard,
                rayDirection.y / rayDirection.z * distanceCamBreadboard,
                0);
        }
        
    }

}