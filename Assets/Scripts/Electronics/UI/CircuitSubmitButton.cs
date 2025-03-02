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
            string acc = "";
            Graph circuitGraph = Breadboard.CreateGraph();
            circuitGraph.DefineBranches();
            acc += $"Start vertices enumeration ({circuitGraph.Vertices.Count}):\n";
            foreach (var e in circuitGraph.Vertices) acc += $"{e}\n";
            acc += $"Start branches enumeration ({circuitGraph.Branches.Count}):\n";
            foreach (var e in circuitGraph.Branches) acc += $"{e}\n";
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp)Breadboard.Target;
            acc += targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success\n" : "The lamp is OFF ! You failed\n";
            acc += $"tension of the target : {targetLamp.GetVoltage(intensity)} Volts";
            Debug.Log(acc);
        }
    }
}