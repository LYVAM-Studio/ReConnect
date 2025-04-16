using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace Reconnect.Electronics.Components
{
    public class Node : Vertex
    {
        public Node(string name) : base(name)
        {
        }

        public Node(string name, List<Vertex> adjacentComponents) : base(name, adjacentComponents)
        {
        }
    }
}