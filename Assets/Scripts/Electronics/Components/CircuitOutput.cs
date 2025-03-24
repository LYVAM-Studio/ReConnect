using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace TestGraph.Components
{
    public class CircuitOutput : Node
    {
        public CircuitOutput(string name) : base(name)
        {
        }

        public CircuitOutput(string name, List<Vertex> adjacentComponents) : base(name, adjacentComponents)
        {
        }
    }
}