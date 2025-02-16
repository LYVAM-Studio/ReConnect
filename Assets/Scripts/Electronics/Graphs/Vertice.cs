using System;
using System.Collections.Generic;
using Reconnect.Electronics.Components;

namespace Reconnect.Electronics.Graph
{
    public class Vertice
    {
        public IElecComponent Component { get; }
        public List<Vertice> AdjacentComponents { get; }
        public string Name { get; }
        public Vertice(string name, IElecComponent component)
        {
            AdjacentComponents = new List<Vertice>();
            Component = component;
            Name = name;
        }
        
        public Vertice(string name, IElecComponent component, List<Vertice> adjacentComponents)
        {
            Component = component;
            AdjacentComponents = adjacentComponents;
            Name = name;
        }

        public void AddAdjacent(Vertice adjacent) => AdjacentComponents.Add(adjacent);
        public void AddAdjacent(IEnumerable<Vertice> adjacentsList) => AdjacentComponents.AddRange(adjacentsList);
        
        public static bool operator==(Vertice left, Vertice right) => left is not null && left.Equals(right);
        public static bool operator!=(Vertice left, Vertice right) => !(left == right);
        public override bool Equals(object obj) => obj is Vertice pole && Equals(pole) ;

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Component);
        }

        private bool Equals(Vertice other) => Component == other.Component && AdjacentComponents == other.AdjacentComponents;

        public override string ToString()
        {
            return $"({Name})";
        }
    }
}