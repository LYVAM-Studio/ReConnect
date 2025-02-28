using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace TestGraph.Components
{
    public class Node : Vertice
    {
        public Node(string name) : base(name)
        {
        }

        public Node(string name, List<Vertice> adjacentComponents) : base(name, adjacentComponents)
        {
        }
    
        public Node(Node other) : base(other.Name, new List<Vertice>(other.AdjacentComponents))
        {
        }
    }
}