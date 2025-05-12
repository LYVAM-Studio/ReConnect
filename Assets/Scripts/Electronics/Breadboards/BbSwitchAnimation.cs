using System;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class BbSwitchAnimation : MonoBehaviour
    {
        public BbSwitch bbSwitch;

        public void OnIdleDown()
        {
            if (bbSwitch is null)
                throw new ArgumentException("Missing reference to the BbSwitch");
            bbSwitch.OnSwitchIdleDown();
        }

        public void OnStartUp()
        {
            if (bbSwitch is null)
                throw new ArgumentException("Missing reference to the BbSwitch");
            bbSwitch.OnSwitchStartUp();
        }
    }
}

