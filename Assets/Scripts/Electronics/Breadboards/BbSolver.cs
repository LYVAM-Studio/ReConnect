using System;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;

namespace Electronics.Breadboards
{
    public enum BreadboardResult
    {
        Success,
        Failure,
        ShortCircuit
    }
    public static class BbSolver
    {
        private static bool CheckTension(ElecComponent target, double intensity, double expectedTension, float tolerance)
            => Math.Abs(target.GetTension(intensity) - expectedTension) / expectedTension < tolerance;

        private static bool CheckIntensity(double intensity, double expectedIntensity, float tolerance)
            => Math.Abs(intensity - expectedIntensity) / expectedIntensity < tolerance;

        public static BreadboardResult ExecuteCircuit(Breadboard breadboard)
        {
            Graph circuitGraph = GraphConverter.CreateGraph(breadboard);
            circuitGraph.DefineBranches();
            double intensity = circuitGraph.GetGlobalIntensity();

            if (double.IsPositiveInfinity(intensity))
            {
                return BreadboardResult.ShortCircuit;
            }
            
            WriteReport(circuitGraph, breadboard, intensity);

            if (breadboard.CircuitInfo.TargetQuantity is CircuitInfo.Quantity.Tension)
            {
                if (!CheckTension(UidDictionary.Get<ElecComponent>(breadboard.TargetUid), intensity,
                        breadboard.CircuitInfo.TargetValue, breadboard.CircuitInfo.TargetTolerance))
                {
                    return BreadboardResult.Failure;
                }
            }
            else
            {
                if (!CheckIntensity(intensity, breadboard.CircuitInfo.TargetValue,
                        breadboard.CircuitInfo.TargetTolerance))
                {
                    return BreadboardResult.Failure;
                }
            }

            UidDictionary.Get<ElecComponent>(breadboard.TargetUid).DoAction();
            return BreadboardResult.Success;
        }
        
        // Debug function
        private static void WriteReport(Graph circuitGraph, Breadboard breadboard, double intensity, string path = "CircuitReport.md")
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
            writer.WriteLine($"- Actual tension: {UidDictionary.Get<ElecComponent>(breadboard.TargetUid).GetTension(intensity):0.##} V");
            var tVal = breadboard.CircuitInfo.TargetValue;
            writer.WriteLine($"- Expected tension: {tVal:0.##} V");
            writer.WriteLine($"- Actual percentage: {Math.Abs(UidDictionary.Get<ElecComponent>(breadboard.TargetUid).GetTension(intensity) - tVal) / tVal * 100:0.##}%");
            writer.WriteLine($"- Tolerance: {breadboard.CircuitInfo.TargetTolerance * 100:0.##}%");
        }
    }
}