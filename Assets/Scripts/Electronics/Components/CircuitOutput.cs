using System.Collections.Generic;
using Reconnect.Electronics.Graph;

namespace TestGraph.Components
{
    public class CircuitOutput : Node
    {
        public CircuitOutput(string name) : base(name)
        {
        }

        public CircuitOutput(string name, List<Vertice> adjacentComponents) : base(name, adjacentComponents)
        {
        }
    }
}