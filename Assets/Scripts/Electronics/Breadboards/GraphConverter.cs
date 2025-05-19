using System.Collections.Generic;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;

namespace Reconnect.Electronics.Breadboards
{
    public static class GraphConverter
    {
        public static Graph CreateGraph(Breadboard breadboard)
        {
            ClearInnerAdjacences(breadboard.Dipoles);
            
            // inner vertices can be null!
            Vertex[,] grid = new Vertex[8, 8];
            PlaceWires(grid, breadboard.Wires);
            PlaceDipoles(grid, breadboard.Dipoles);
            
            var input = new CircuitInput("input", (int)breadboard.CircuitInfo.InputTension, (int)breadboard.CircuitInfo.InputIntensity);
            Vertex.AddReciprocalAdjacent(GetVertexOrNewAt(grid, breadboard.CircuitInfo.InputPoint.x, 0), input);
            var output = new CircuitOutput("output");
            Vertex.AddReciprocalAdjacent(GetVertexOrNewAt(grid, breadboard.CircuitInfo.OutputPoint.x, 7), output);
            var graph = new Graph("Main graph", input, output, UidDictionary.Get<ElecComponent>(breadboard.TargetUid));
            foreach (Vertex v in grid)
                if (v is not null)
                    graph.AddVertex(v);

            foreach (var d in breadboard.Dipoles)
                graph.AddVertex(UidDictionary.Get<Vertex>(d.InnerUid));

            Clean(graph);
            MakeNodes(graph);

            return graph;
        }

        private static void Clean(Graph graph)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = graph.Vertices.Count - 1; i >= 0; i--)
                {
                    Vertex vertex = graph.Vertices[i];
                    if (vertex.AdjacentComponents.Count <= 1)
                    {
                        if (vertex.AdjacentComponents.Count == 1)
                            Vertex.RemoveReciprocalAdjacent(vertex, vertex.AdjacentComponents[0]);

                        graph.Vertices.RemoveAt(i);
                        changed = true;
                    }
                }
            }
        }

        private static void MakeNodes(Graph graph)
        {
            for (int i = graph.Vertices.Count - 1; i >= 0; i--)
            {
                Vertex vertex = graph.Vertices[i];
                if (vertex is Vertex node && node.AdjacentComponents.Count > 2)
                {
                    graph.Vertices.Remove(vertex);
                    graph.AddVertex(vertex.ToNode());
                }
            }
        }

        private static void ClearInnerAdjacences(List<Dipole> dipoles)
        {
            foreach (var d in dipoles)
                UidDictionary.Get<Vertex>(d.InnerUid).ClearAdjacent();
        }

        private static Vertex GetVertexOrNewAt(Vertex[,] grid, int x, int y)
        {
            if (grid[y, x] is null)
            {
                grid[y, x] = new Vertex($"({y}, {x})");
            }

            return grid[y, x];
        }

        private static void PlaceWires(Vertex[,] grid, List<WireScript> wires)
        {
            foreach (var w in wires)
            {
                Vertex v1 = GetVertexOrNewAt(grid, w.Pole1.x, w.Pole1.y);
                Vertex v2 = GetVertexOrNewAt(grid, w.Pole2.x, w.Pole2.y);
                Vertex.AddReciprocalAdjacent(v1, v2);
            }
        }
        
        private static void  PlaceDipoles(Vertex[,] grid, List<Dipole> dipoles)
        {
            foreach (var d in dipoles)
            {
                Vertex v1 = GetVertexOrNewAt(grid, d.Pole1.x, d.Pole1.y);
                Vertex v2 = GetVertexOrNewAt(grid, d.Pole2.x, d.Pole2.y);
                Vertex.AddReciprocalAdjacent(v1, UidDictionary.Get<Vertex>(d.InnerUid));
                Vertex.AddReciprocalAdjacent(UidDictionary.Get<Vertex>(d.InnerUid), v2);
            }
        }
    }
}