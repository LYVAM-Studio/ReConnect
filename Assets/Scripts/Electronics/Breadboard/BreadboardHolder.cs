using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Electronics.Breadboards
{
    /// <summary>
    /// To ease the access to the breadboard, the UI and the camera. 
    /// </summary>
    public class BreadboardHolder : MonoBehaviour
    {
        public Breadboard breadboard;
        public Camera cam;
        
        // Gets the position of the cursor projected on the breadboard plane. This vector's z component is therefore always 0.
        public Vector3 GetFlattedCursorPos(float distanceCamBreadboard = 8f)
        {
            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            // // Raycast version:
            // Physics.Raycast(MainCamera.transform.position, ray.direction, out var hit);
            // return new Vector3(hit.point.x, hit.point.y, 0);
            var rayDirection = ray.direction;
            return new Vector3(
                rayDirection.x / rayDirection.z * distanceCamBreadboard,
                rayDirection.y / rayDirection.z * distanceCamBreadboard,
                0);
        }
    }
    
    
}