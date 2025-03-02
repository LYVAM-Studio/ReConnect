using System;
using Reconnect.Electronics.Graphs;
using TestGraph.Components;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class CircuitSubmitButton : MonoBehaviour
    {
        public Breadboard Breadboard;

        public void Start()
        {
            if (Breadboard is null)
                throw new ArgumentException("No reference to the BreadBoard component");
        }

        public void ExecuteCircuit()
        {
            Graph circuitGraph = Breadboard.CreateGraph();
            circuitGraph.DefineBranches();
            Debug.Log('\n');
            foreach (var e in circuitGraph.Vertices)
            {
                Debug.Log(e);
            }
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp) Breadboard.Target;
            Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
            Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
        }
    }
}