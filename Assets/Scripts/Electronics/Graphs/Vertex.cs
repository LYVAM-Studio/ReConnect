using System;
using System.Collections.Generic;
using Reconnect.Electronics.Components;

namespace Reconnect.Electronics.Graphs
{
    public class Vertex
    {
        public List<Vertex> AdjacentComponents { get; }
        public string Name { get; }
        public Vertex(string name)
        {
            AdjacentComponents = new List<Vertex>();
            Name = name;
        }
        
        public Vertex(string name, List<Vertex> adjacentComponents)
        {
            AdjacentComponents = adjacentComponents;
            Name = name;
        }

        public void AddAdjacent(Vertex adjacent) => AdjacentComponents.Add(adjacent);
        public void RemoveAdjacent(Vertex v) => AdjacentComponents.Remove(v);

        public static void ReciprocalAddAdjacent(Vertex v1, Vertex v2)
        {
            v1.AddAdjacent(v2);
            v2.AddAdjacent(v1);
        }
        public void AddAdjacent(IEnumerable<Vertex> adjacentsList) => AdjacentComponents.AddRange(adjacentsList);
        public virtual int AdjacentCount() => AdjacentComponents.Count;

        public Node ToNode()
        {
            Node node = new Node(Name, AdjacentComponents);
            foreach (var v in AdjacentComponents)
            {
                v.RemoveAdjacent(this);
                v.AddAdjacent(node);
            }

            return node;
        }
        
        public void ClearAdjacent()
        {
            AdjacentComponents.RemoveAll(v => true);
        }
        
        // NOTE: the comparison of vertices by pointer is needed by the Vertex.ToNode method used in Breadboard.CreateGraph method.
        // Please do not implement any other equality comparison on the Vertex class or any of its inherited classes.
        
        // public static bool operator==(Vertex? left, Vertex? right) => left is not null && right is not null && left.Equals(right);
        // public static bool operator!=(Vertex? left, Vertex? right) => !(left == right);
        // public override bool Equals(object obj) => obj is Vertex pole && Equals(pole) ;
        //
        // public override int GetHashCode()
        // {
        //     return HashCode.Combine(Name, AdjacentComponents);
        // }
        //
        // private bool Equals(Vertex other) => Name == other.Name && AdjacentComponents == other.AdjacentComponents;

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}