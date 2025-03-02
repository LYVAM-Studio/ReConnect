using System;
using System.Collections.Generic;
using TestGraph.Components;

namespace Reconnect.Electronics.Graphs
{
    public class Vertice
    {
        public List<Vertice> AdjacentComponents { get; }
        public string Name { get; }
        public Vertice(string name)
        {
            AdjacentComponents = new List<Vertice>();
            Name = name;
        }
        
        public Vertice(string name, List<Vertice> adjacentComponents)
        {
            AdjacentComponents = adjacentComponents;
            Name = name;
        }

        public void AddAdjacent(Vertice adjacent) => AdjacentComponents.Add(adjacent);
        public void AddAdjacent(IEnumerable<Vertice> adjacentsList) => AdjacentComponents.AddRange(adjacentsList);
        public virtual int AdjacentCount() => AdjacentComponents.Count;
        // public static bool operator==(Vertice? left, Vertice? right) => left is not null && right is not null && left.Equals(right);
        // public static bool operator!=(Vertice? left, Vertice? right) => !(left == right);
        // public override bool Equals(object obj) => obj is Vertice pole && Equals(pole) ;
        //
        // public override int GetHashCode()
        // {
        //     return HashCode.Combine(Name, AdjacentComponents);
        // }
        //
        // private bool Equals(Vertice other) => Name == other.Name && AdjacentComponents == other.AdjacentComponents;

        public override string ToString()
        {
            return $"({Name})";
        }
    }
}