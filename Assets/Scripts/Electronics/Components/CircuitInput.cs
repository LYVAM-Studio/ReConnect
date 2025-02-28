using System.Collections.Generic;
using Reconnect.Electronics.Graph;

namespace TestGraph.Components
{
    public class CircuitInput : Node
    {
        public int InputTension;
        public int MaxIntensity;

        public CircuitInput(string name, int inputTension, int inputIntensity) : base(name)
        {
            InputTension = inputTension;
            MaxIntensity = inputIntensity;
        }

        public CircuitInput(string name, List<Vertice> adjacentComponents, int inputTension, int inputIntensity) : base(name, adjacentComponents)
        {
            InputTension = inputTension;
            MaxIntensity = inputIntensity;
        }
    
        public override int AdjacentCount() => AdjacentComponents.Count + 1;

    }
}