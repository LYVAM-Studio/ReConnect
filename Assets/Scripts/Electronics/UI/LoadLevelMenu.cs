using System;
using Reconnect.Electronics.UI;
using UnityEngine;

public class LoadLevelMenu : MonoBehaviour
{
    private BreadboardUI _breadboardUI;

    public string CircuitName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _breadboardUI = GetComponentInParent<BreadboardUI>();
    }

    public void LoadCircuitLevel()
    {
        _breadboardUI.Breadboard.LoadCircuit(CircuitName);
    }
}
