using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace TestGraph.Components
{
    public class Node : Vertex
    {
        public Node(string name) : base(name)
        {
        }

        public Node(string name, List<Vertex> adjacentComponents) : base(name, adjacentComponents)
        {
        }
    
        public Node(Node other) : base(other.Name, new List<Vertex>(other.AdjacentComponents))
        {
        }
    }
}