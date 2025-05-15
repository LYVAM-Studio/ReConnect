using System;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;

namespace Electronics.Breadboards
{
    public class BbSolver
    {
        private static BbSolver _instance;

        public static BbSolver Instance
        {
            get { return _instance ??= new BbSolver(); }
        }

        private BbSolver()
        {
            if (_instance is not null)
                throw new Exception("UniqueIdDictionary has already been instantiated");
        }

        private bool CheckTension(ElecComponent target, double intensity, double expectedTension, float tolerance)
            => Math.Abs(target.GetTension(intensity) - expectedTension) / expectedTension < tolerance;

        private bool CheckIntensity(double intensity, double expectedIntensity, float tolerance)
            => Math.Abs(intensity - expectedIntensity) / expectedIntensity < tolerance;

        public bool ExecuteCircuit(Breadboard breadboard)
        {
            Graph circuitGraph = GraphConverter.CreateGraph(breadboard);
            circuitGraph.DefineBranches();
            double intensity = circuitGraph.GetGlobalIntensity();

            // WriteReport(circuitGraph, breadboard, intensity);

            if (breadboard.CircuitInfo.TargetQuantity is CircuitInfo.Quantity.Tension)
            {
                if (!CheckTension(UniqueIdDictionary.Instance.Get<ElecComponent>(breadboard.TargetID), intensity,
                        breadboard.CircuitInfo.TargetValue, breadboard.CircuitInfo.TargetTolerance))
                {
                    return false;
                }
            }
            else
            {
                if (!CheckIntensity(intensity, breadboard.CircuitInfo.TargetValue,
                        breadboard.CircuitInfo.TargetTolerance))
                {
                    return false;
                }
            }

            UniqueIdDictionary.Instance.Get<ElecComponent>(breadboard.TargetID).DoAction();
            return true;
        }
        
        // Debug function
        private void WriteReport(Graph circuitGraph, Breadboard breadboard, double intensity, string path = "CircuitReport.md")
        {
            using System.IO.StreamWriter writer = new System.IO.StreamWriter(path);
            writer.WriteLine("## State");
            writer.WriteLine();
            foreach (Vertex v in circuitGraph.Vertices)
            {
                writer.WriteLine($"- {v.GetType().Name} {v.Name}:");
                foreach (Vertex adj in v.AdjacentComponents)
                    writer.WriteLine($"  - {adj}");
            }

            writer.WriteLine();
            writer.WriteLine("## Vertices");
            writer.WriteLine();
            writer.WriteLine($"**Number: {circuitGraph.Vertices.Count}**");
            writer.WriteLine();
            foreach (Vertex v in circuitGraph.Vertices)
                writer.WriteLine($"- {v}");
            writer.WriteLine();
            writer.WriteLine("## Branches");
            writer.WriteLine();
            writer.WriteLine($"**Number: {circuitGraph.Branches.Count}**");
            writer.WriteLine();
            foreach (Branch b in circuitGraph.Branches)
            {
                writer.WriteLine(
                    $"- {b.Resistance:0.##} Ohms from {b.StartNode} to {b.EndNode}");
                foreach (var c in b.Components)
                    writer.WriteLine($"  - {c}");
            }

            writer.WriteLine();
            writer.WriteLine("## Result");
            writer.WriteLine();
            writer.WriteLine($"- Actual intensity: {intensity:0.##} A");
            writer.WriteLine($"- Actual tension: {UniqueIdDictionary.Instance.Get<ElecComponent>(breadboard.TargetID).GetTension(intensity):0.##} V");
            var tVal = breadboard.CircuitInfo.TargetValue;
            writer.WriteLine($"- Expected tension: {tVal:0.##} V");
            writer.WriteLine($"- Actual percentage: {Math.Abs(UniqueIdDictionary.Instance.Get<ElecComponent>(breadboard.TargetID).GetTension(intensity) - tVal) / tVal * 100:0.##}%");
            writer.WriteLine($"- Tolerance: {breadboard.CircuitInfo.TargetTolerance * 100:0.##}%");
        }
    }
}