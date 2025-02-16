using System;
using System.Collections.Generic;

namespace Reconnect.Electronics.Graph
{
    public class Branch
    {
        public List<Vertice> Components;

        public Branch() => Components = new List<Vertice>();
        public Branch(IEnumerable<Vertice> components) => Components = new List<Vertice>(components);
        
        public void AddVertice(Vertice Component)
        {
            // skip if it is already in the list
            if (Components.Contains(Component))
                return;
            Components.Add(Component);
        }

        public override string ToString() => $"[{String.Join(", ", Components)}]";
    }
}