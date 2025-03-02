using System;
using Reconnect.Electronics.Graphs;
using TestGraph.Components;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class CircuitSubmitButton : MonoBehaviour
    {
        public Breadboard Breadboard;

        public void Awake()
        {
            // Get the grandparent transform
            Transform grandparent = transform.parent?.parent;

            if (grandparent is null)
                throw new ArgumentException(
                    "Missing grand parent in scene architecture : could not get the breadboard component");
            
            // Get the component from the grandparent
            Breadboard myComponent = grandparent.GetComponent<Breadboard>();

            if (myComponent is null)
                throw new ArgumentException(
                    "Missing breadboard component in the scene architecture : could not get the breadboard component");

            Breadboard = myComponent;
        }

        public void ExecuteCircuit()
        {
            Graph circuitGraph = Breadboard.CreateGraph();
            circuitGraph.DefineBranches();
            double intensity = circuitGraph.GetGlobalIntensity();
            Lamp targetLamp = (Lamp) Breadboard.Target;
            Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
        }
    }
}