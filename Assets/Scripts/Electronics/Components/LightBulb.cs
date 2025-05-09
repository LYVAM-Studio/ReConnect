using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Components
{
    public class LightBulb : MonoBehaviour
    {
        public Material lightOn;
        public Material lightOff;
        public void Set(bool isOn)
        {
            // Debug.Log($"Activated to {on}");
            if (!TryGetComponent(out Renderer renderer))
                throw new ComponentNotFoundException("No Renderer component has been found on the light bulb.");

            renderer.material = isOn ? lightOn : lightOff;
        }
    }
}
