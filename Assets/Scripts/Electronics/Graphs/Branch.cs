using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TestGraph.Components;

namespace Reconnect.Electronics.Graphs
{
    public class Branch
    {
        public List<Vertice> Components;
        public Node StartNode;
        public Node EndNode;
        public double Resistance;

        /// <summary>
        /// Create a new branch being a list of components between 2 nodes.
        /// The <see cref="Components"/> list is initialized empty
        /// </summary>
        /// <param name="startNode">The starting node of the branch</param>
        /// <param name="endNode">The ending node of the branch</param>
        public Branch(Node startNode, Node endNode)
        {
            StartNode = startNode;
            EndNode = endNode;
            Components = new List<Vertice>();
            Resistance = 0;
        }
        /// <summary>
        /// Create a new branch being a list of components between 2 nodes.
        /// The <see cref="Components"/> list contains the components given
        /// </summary>
        /// <param name="startNode">The starting node of the branch</param>
        /// <param name="endNode">The ending node of the branch</param>
        /// <param name="components">The components the branch contains</param>
        public Branch(Node startNode, Node endNode, IEnumerable<Vertice> components)
        {
            StartNode = startNode;
            EndNode = endNode;
            Components = new List<Vertice>();
            Resistance = 0;
            AddVertice(components);
        }

        /// <summary>
        /// Add a vertice to the <see cref="Components"/> list
        /// </summary>
        /// <param name="vertice">The vertice to add</param>
        public void AddVertice(Vertice vertice)
        {
            // skip if it is already in the list
            if (Components.Contains(vertice))
                return;
            
            Components.Add(vertice);
            if (vertice is ElecComponent component)
                Resistance += component.Resistance;
        }
        /// <summary>
        /// Add multiple vertices to the <see cref="Components"/> list
        /// </summary>
        /// <param name="vertices">The vertices to add</param>
        public void AddVertice(IEnumerable<Vertice> vertices)
        {
            foreach (Vertice vertice in vertices)
            {
                AddVertice(vertice);
            }
        }

        /// <summary>
        /// Tests whether 2 braches are int parallel or not
        /// </summary>
        /// <remarks>Two branches are considered in parallel if they have the same 2 nodes, regardless of their order</remarks>
        /// <param name="other">The other branch to test with</param>
        /// <returns><c>true</c> if they are in parallel, otherwise <c>false</c></returns>
        public bool AreParallelBranches(Branch other) =>
            (other.StartNode, other.EndNode) == (EndNode, StartNode) || (other.StartNode, other.EndNode) == (StartNode, EndNode);
        
        public override string ToString() => $"{StartNode} [{string.Join(", ", Components)}] {EndNode}";
        /// <summary>
        /// Displays information about the branch
        /// </summary>
        /// <returns>A <see cref="string"/> containing the nodes, components and the resistance of the branch</returns>
        public string Display() => $"{this} - Resistance : {Resistance} Ohms";

        public static bool operator ==(Branch? left, Branch? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left.Components.SequenceEqual(right.Components) &&
                   left.StartNode == right.StartNode &&
                   left.EndNode == right.EndNode &&
                   left.Resistance.Equals(right.Resistance);
        }

        public static bool operator !=(Branch left, Branch right) => !(left == right);

        public override bool Equals(object? obj) => obj is Branch other && this == other;
    
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Components?.Aggregate(0, (acc, v) => acc ^ v.GetHashCode()) ?? 0,
                StartNode,
                EndNode,
                Resistance
            );
        }
    }
}