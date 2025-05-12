using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace Reconnect.Electronics.Components
{
    
    public class Resistor : ElecComponent
    {
        public Resistor(string name, double resistance) : base(name, resistance)
        {
        }

        public Resistor(string name, List<Vertex> adjacentComponents, double resistance) : base(name, adjacentComponents, resistance)
        {
        }
    }
}