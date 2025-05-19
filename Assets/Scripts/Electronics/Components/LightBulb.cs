using System;
using Mirror;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Electronics.Components
{
    public class LightBulb : NetworkBehaviour
    {
        public Material lightOn;
        public Material lightOff;
        [SyncVar(hook = nameof(OnStateChanged))]
        public bool isOn;

        private Renderer _renderer;

        private void Awake()
        {
            if (!TryGetComponent(out Renderer renderer))
                throw new ComponentNotFoundException("No Renderer component has been found on the light bulb.");
            _renderer = renderer;
        }

        void OnStateChanged(bool _, bool newVal)
        {
            // Update visuals
            _renderer.material = newVal ? lightOn : lightOff;
        }
        
        public void Set(bool isTurnedOn)
        {
            isOn = isTurnedOn;
        }
    }
}
