using System;
using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace TestGraph.Components
{
    
    public class Resistor : ElecComponent
    {
        public Resistor(string name, double resistance) : base(name, resistance)
        {
        }

        public Resistor(string name, List<Vertice> adjacentComponents, double resistance) : base(name, adjacentComponents, resistance)
        {
        }
    
        public override double GetVoltage(double intensity)
        {
            if (intensity <= 0)
                throw new ArgumentException("Intensity going through a component must be positive and non-zero");
            return Resistance * intensity;
        }
    }
}