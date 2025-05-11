using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BbSwitchAnimation : MonoBehaviour
{
    public BbSwitch bbSwitch;

    public void OnIdleDown()
    {
        Debug.Log("Lever is Down");
        if (bbSwitch != null)
            bbSwitch.OnSwitchIdleDown();
    }

    public void OnIdleUp()
    {
        Debug.Log("Lever is Up");
        if (bbSwitch != null)
            bbSwitch.OnSwitchIdleUp();
    }
}
