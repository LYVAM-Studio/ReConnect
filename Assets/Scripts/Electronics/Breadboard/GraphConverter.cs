using System.Collections.Generic;
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
            Vertex.AddReciprocalAdjacent(GetVertexOrNewAt(grid, 0, 3), input);
            var output = new CircuitOutput("output");
            Vertex.AddReciprocalAdjacent(GetVertexOrNewAt(grid, 7, 3), output);
            var graph = new Graph("Main graph", input, output, breadboard.Target);
            foreach (Vertex v in grid)
            {
                if (v is not null)
                {
                    if (v.AdjacentComponents.Count > 2)
                        graph.AddVertex(v.ToNode());
                    else
                        graph.AddVertex(v);
                }
            }

            foreach (var d in breadboard.Dipoles)
            {
                graph.AddVertex(d.Inner);
            }

            return graph;
        }

        private static void ClearInnerAdjacences(List<Dipole> dipoles)
        {
            foreach (var d in dipoles)
                d.Inner.ClearAdjacent();
        }

        private static Vertex GetVertexOrNewAt(Vertex[,] grid, int h, int w)
        {
            if (grid[h, w] is null)
            {
                grid[h, w] = new Vertex($"({h}, {w})");
            }

            return grid[h, w];
        }

        private static void PlaceWires(Vertex[,] grid, List<WireScript> wires)
        {
            foreach (var w in wires)
            {
                Vertex v1 = GetVertexOrNewAt(grid, w.Pole1.y, w.Pole1.x);
                Vertex v2 = GetVertexOrNewAt(grid, w.Pole2.y, w.Pole2.x);
                Vertex.AddReciprocalAdjacent(v1, v2);
            }
        }
        
        private static void  PlaceDipoles(Vertex[,] grid, List<Dipole> dipoles)
        {
            foreach (var d in dipoles)
            {
                Vertex v1 = GetVertexOrNewAt(grid, d.Pole1.y, d.Pole1.x);
                Vertex v2 = GetVertexOrNewAt(grid, d.Pole2.y, d.Pole2.x);
                Vertex.AddReciprocalAdjacent(v1, d.Inner);
                Vertex.AddReciprocalAdjacent(d.Inner, v2);
            }
        }
    }
}