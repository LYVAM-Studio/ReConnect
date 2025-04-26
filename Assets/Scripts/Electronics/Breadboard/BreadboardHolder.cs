using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    /// <summary>
    /// To ease the access to the breadboard, the UI and the camera. 
    /// </summary>
    public class BreadboardHolder : MonoBehaviour
    {
        public Breadboard breadboard;
        public Camera cam;
        
        public Vector3 GetFlattenedCursorPos()
        {
            Plane plane = new Plane(transform.rotation * Vector3.forward, transform.position);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var dist))
                return ray.GetPoint(dist);
            
            return Vector3.zero; // throw?
        }
    }
    
    
}