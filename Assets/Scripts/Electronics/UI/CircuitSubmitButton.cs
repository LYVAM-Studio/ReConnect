using System;
using System.Linq;
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
            Debug.Log($"VERTICES({circuitGraph.Vertices.Count}) :::\n"+string.Join('\n', circuitGraph.Vertices/*from e in circuitGraph.Vertices select e.ToString()*/));
            Debug.Log($"BRANCHES({circuitGraph.Branches.Count}) :::\n"+string.Join('\n', circuitGraph.Branches/*from e in circuitGraph.Branches select e.Display()*/));
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp) BreadboardUI.Breadboard.Target;
            targetLamp.Set(intensity);
            Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
            Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
        }
    }
}