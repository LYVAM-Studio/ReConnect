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
            foreach (var e in circuitGraph.Vertices)
            {
                Debug.Log($"{e.Name}");
            }
            Debug.Log(circuitGraph.Vertices.Count);
            Debug.Log("==============================================");
            circuitGraph.DefineBranches();
            foreach (var e in circuitGraph.Branches)
            {
                Debug.Log($"{e.Display()}");
            }
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp) BreadboardUI.Breadboard.Target;
            targetLamp.Set(intensity);
            Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
            Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
        }
    }
}