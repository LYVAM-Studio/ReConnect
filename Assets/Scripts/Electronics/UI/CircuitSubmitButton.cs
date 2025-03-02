using System;
using Reconnect.Electronics.Graphs;
using TestGraph.Components;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class CircuitSubmitButton : MonoBehaviour
    {
        public BreadboardUI BreadboardUI;

        public void Start()
        {
            BreadboardUI = GetComponentInParent<BreadboardUI>();
        }

        public void ExecuteCircuit()
        {
            Graph circuitGraph = BreadboardUI.Breadboard.CreateGraph();
            circuitGraph.DefineBranches();
            Debug.Log('\n');
            foreach (var e in circuitGraph.Vertices)
            {
                Debug.Log(e);
            }
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp) BreadboardUI.Breadboard.Target;
            Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
            Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
        }
    }
}