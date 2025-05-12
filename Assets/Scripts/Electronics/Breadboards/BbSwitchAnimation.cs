using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Electronics.Breadboards
{
    public class BbSwitchAnimation : MonoBehaviour
    {
        public BbSwitch bbSwitch;

        public void OnIdleDown()
        {
            if (bbSwitch != null)
                bbSwitch.OnSwitchIdleDown();
        }

        public void OnStartUp()
        {
            if (bbSwitch != null)
                bbSwitch.OnSwitchStartUp();
        }
    }
}

