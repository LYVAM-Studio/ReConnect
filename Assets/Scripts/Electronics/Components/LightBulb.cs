using UnityEngine;

namespace Reconnect.Electronics.Components
{
    public class LightBulb : MonoBehaviour
    {
        public Material lightOn;
        public Material lightOff;
        public void Set(bool on)
        {
            GetComponent<Renderer>().material = on ? lightOn : lightOff;
        }
    }
}
