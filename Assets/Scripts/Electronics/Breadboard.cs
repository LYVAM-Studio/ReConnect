using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;
using TestGraph.Components;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Breadboard : MonoBehaviour
    {
        // The list of the components in the breadboard
        private List<Dipole> _dipoles;

        // The start of the wire ig _onWireCreation
        private Vector3 _lastNodePosition;

        // Whether the wire creation is in deletion mode (removes wires instead of placing them)
        private bool _onDeletionMode;

        // Whether a wire is being created (implies that the mouse is down)
        private bool _onWireCreation;

        // The list of the wires on the breadboard
        private List<WireScript> _wires;

        // The Z coordinate at which the dipoles are positioned on the breadboard
        // it is the Z position of the breadboard (8f) minus half its thickness (1f/2) to have it sunk into the board
        public float zPositionDipoles = 7.5f;

        public ElecComponent Target { get; private set; }
        private void Start()
        {
            _dipoles = new List<Dipole>();
            _wires = new List<WireScript>();
            _onWireCreation = false;
            _onDeletionMode = false;
            //LoadCircuit("1_series_lvl_1");
        }
        
        /// <summary>
        /// Cleanup the breadboard content, deletes the wires and dipoles
        /// </summary>
        public void Clean()
        {
            foreach (WireScript wireScript in _wires)
            {
                Destroy(wireScript.gameObject);
            }
            foreach (Dipole dipole in _dipoles)
            {
                Destroy(dipole.gameObject);
            }

            _wires = new List<WireScript>();
            _dipoles = new List<Dipole>();
        }

        public void LoadCircuit(string circuitName)
        {
            Clean();
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, $"CircuitsPresets/{circuitName}.csv");
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] content = line.Split(',');
                    if (content[0] == "wire")
                    {
                        (int h, int w) from = (int.Parse(content[1]), int.Parse(content[2]));
                        (int h, int w) to = (int.Parse(content[3]), int.Parse(content[4]));
                        Vector3 fromPos = Point.PointToVector(new Point(from.h, from.w), zPositionDipoles);
                        Vector3 toPos = Point.PointToVector(new Point(to.h, to.w), zPositionDipoles);
                        CreateWire(fromPos, toPos);
                    }
                    else if (content[0] == "resistor")
                    {
                        (int h, int w) from = (int.Parse(content[1]), int.Parse(content[2]));
                        (int h, int w) to = (int.Parse(content[3]), int.Parse(content[4])); 
                        double r = double.Parse(content[5], CultureInfo.InvariantCulture);
                        Point fromPos = new Point(from.h, from.w);
                        Point toPos = new Point(to.h, to.w);
                        var resistorGameObj = Instantiate(Helper.GetPrefabByName("Components/ResistorPrefab"));
                        if (resistorGameObj is null)
                            throw new Exception("The resistor prefab could not be found.");
                        var dipoleScript = resistorGameObj.GetComponent<Dipole>();
                        if (dipoleScript is null)
                            throw new Exception("The Dipole script component could not be found in the wire prefab.");
                        var inner = new Resistor("R", r);
                        dipoleScript.Inner = inner;
                        dipoleScript.Breadboard = this;
                        dipoleScript.SetPosition(fromPos, toPos);
                        _dipoles.Add(dipoleScript);
                        if (Target is null) Target = inner;
                    }
                    else if (content[0] == "lamp")
                    {
                        (int h, int w) from = (int.Parse(content[1]), int.Parse(content[2]));
                        (int h, int w) to = (int.Parse(content[3]), int.Parse(content[4])); 
                        double r = double.Parse(content[5], CultureInfo.InvariantCulture);
                        Debug.Log(content[6]);
                        double tension = double.Parse(content[6], CultureInfo.InvariantCulture);
                        Point fromPos = new Point(from.h, from.w);
                        Point toPos = new Point(to.h, to.w);
                        var lampGameObj = Instantiate(Helper.GetPrefabByName("Components/LampPrefab"));
                        if (lampGameObj is null)
                            throw new Exception("The lamp prefab could not be found.");
                        var dipoleScript = lampGameObj.GetComponent<Dipole>();
                        if (dipoleScript is null)
                            throw new Exception("The Dipole script component could not be found in the wire prefab.");
                        var inner = new Lamp("L", r, tension);
                        inner.LightBulb = lampGameObj.GetComponentInChildren<LightBulb>();
                        dipoleScript.Inner = inner;
                        dipoleScript.Breadboard = this;
                        dipoleScript.SetPosition(fromPos, toPos);
                        _dipoles.Add(dipoleScript);
                        if (Target is null) Target = inner;
                    }
                    else
                    {
                        throw new Exception($"{content[0]} is not a valid dipole type.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"The file {circuitName} could not be loaded because an exception was thrown:\n{e.Message}", e);
            }
        }
        
        // #########################
        // #    WIRE MANAGEMENT    #
        // #########################

        public void StartWire(Vector3 nodePosition)
        {
            _onWireCreation = true;
            _onDeletionMode = false;
            _lastNodePosition = nodePosition;
        }

        public void EndWire()
        {
            _onWireCreation = false;
        }

        // This function is called by a breadboard node when the mouse collides it
        public void OnMouseNodeCollision(Vector3 nodePosition)
        {
            // If not no wire creation, then does nothing
            if (!_onWireCreation) return;

            // The difference between the two wire start position and the current mouse position, ignoring the z component
            // This vector corresponds to the future wire
            var delta = (Vector2)nodePosition - (Vector2)_lastNodePosition;

            // nodes are spaced by 1.0f, the diagonal distance would be sqrt(2) ~ 1.41
            // We check if the distance is greater because we want to avoid skipping surrounding nodes.
            if (delta.magnitude > 1.5f)
            {
                // The user skipped one or more node. A wire cannot be created that way
                // Enter in deletion mode to delete wires if the users wants to
                _onDeletionMode = true;
                // Set the start to the current end
                _lastNodePosition = nodePosition;
            }
            else if (delta != Vector2.zero)
            {
                // Is null if a wire is not already at the given position. Otherwise, contains the wire.
                var wire = _wires.Find(w =>
                    (w.Pole1 == Point.VectorToPoint(_lastNodePosition) &&
                     w.Pole2 == Point.VectorToPoint(nodePosition)) ||
                    (w.Pole2 == Point.VectorToPoint(_lastNodePosition) &&
                     w.Pole1 == Point.VectorToPoint(nodePosition)));
                if (wire is not null)
                {
                    // A wire is already at this position
                    // Delete the wire at this position
                    DeleteWire(wire);
                    // Enter the deletion mode
                    _onDeletionMode = true;
                }
                else if (!_onDeletionMode)
                {
                    CreateWire(_lastNodePosition, nodePosition);
                }

                // Set the start to the current end
                _lastNodePosition = nodePosition;
            }
        }

        public void DeleteWire(WireScript wire)
        {
            _wires.Remove(wire);
            Destroy(wire.gameObject);
        }

        public void CreateWire(Vector3 from, Vector3 to)
        {
            if ((from - to).magnitude > 1.5)
                throw new Exception($"The wire from {from} to {to} cannot be created: the cire is too long");
            // Instantiate a wire from the wire prefab
            var wireGameObj = Instantiate(Helper.GetPrefabByName("Components/WirePrefab"));
            if (wireGameObj is null)
                throw new Exception("The wire prefab could not be found.");
            var wireScript = wireGameObj.GetComponent<WireScript>();
            if (wireScript is null)
                throw new Exception("The WireScript component could not be found in the wire prefab.");
            wireGameObj.name = $"WirePrefab (Clone {(uint)wireScript.GetHashCode()})";
            // Debug.Log($"{_lastNodePosition} <=> {Pole.PoleToPosition(Pole.PositionToPole(_lastNodePosition), _zPositionDipoles)}");
            wireScript.Init(this, Point.VectorToPoint(from), Point.VectorToPoint(to));
            _wires.Add(wireScript);
            // Set the wire's position
            wireGameObj.transform.position = (from + to) / 2;
            // Set the wire's rotation
            wireGameObj.transform.LookAt(to);
            wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            // Set the wire's scale (length of the wire)
            var scale = wireGameObj.transform.localScale;
            scale[1] /* y component */ = (to - from).magnitude / 2f;
            wireGameObj.transform.localScale = scale;
        }

        // // Traverses the circuit calculating U and I.
        // public void LaunchElectrons()
        // {
        //     var circuit = InitCircuit();
        //     var explored = new List<(int h, int w)>();
        //     LaunchElectronsRec(circuit, explored, 0, 0);
        // }
        //
        // public void LaunchElectronsRec(List<ElecComponent>[,] circuit, List<(int h, int w)> explored, int h, int w)
        // {
        //     // Forward while there is a wire
        //     while (circuit[h, w].Count == 1 && circuit[h, w][0].type == ComponentType.WireScript)
        //         (h, w) = circuit[h, w][0].GetNormalizedPos();
        //     foreach (var component in circuit[h, w])
        //     {
        //         
        //     }
        // }
        //
        // // Returns the current circuit as a 2-dimensional array. Each element of this array represents the list of every component (given by ref, no copy) that has a pole there (i.e. is plugged in the corresponding breadboard hole).  
        // private List<ElecComponent>[,] InitCircuit()
        // {
        //     // Init the 2-dimensional array with empty lists
        //     var circuit = new List<ElecComponent>[8, 8];
        //     for (int h = 0; h < 8; h++)
        //     for (int w = 0; w < 8; w++)
        //         circuit[h, w] = new List<ElecComponent>();
        //
        //     // Add the components in the empty 2-dimensional array
        //     foreach (var component in _components)
        //     {
        //         // The position of the currently processed component according to the breadboard
        //         var origin = component.GetNormalizedPos();
        //         foreach (var pole in component.poles)
        //         {
        //             if (origin.h + pole.GetH() is < 0 or >= 8 || origin.w + pole.GetW() is < 0 or >= 8)
        //                 throw new IndexOutOfRangeException(
        //                     $"The pole (x:{pole.x}, y:{pole.y}) or also (h:{pole.GetH()}, w:{pole.GetW()}) goes outside the breadboard.");
        //             circuit[origin.h + pole.GetH(), origin.w + pole.GetW()].Add(component);
        //         }
        //     }
        //     
        //     return circuit;
        // }
        
        
        

        public Graph CreateGraph()
        {
            Debug.Log("WIRES:" + string.Join(", ",
                                   from wire in _wires select "(" + wire.Pole1 + " " + wire.Pole2 + ")")
                               + "\nDIPOLES:" + string.Join(", ",
                                   from dipole in _dipoles
                                   select "(" + dipole.GetPoles()[0] + " " + dipole.GetPoles()[1] + ")"));
            var input = new CircuitInput("input", 240, 16);
            var output = new CircuitOutput("output");
            var graph = new Graph("Main graph", input, output, Target);
            var queue = new Queue<(Point lastPosition, Point position, Vertice lastComponent)>();
            var visitedDipoles = new List<Vertice>();
            var visitedNodes = new List<Node>();
            queue.Enqueue((new Point(-1, -1), new Point(0, 3), input));
            while (queue.Count > 0)
            {
                var (lastPos, pos, lastComponent) = queue.Dequeue();
                Debug.Log($"### Dequeued {lastComponent} of type {lastComponent.GetType()}");

                if (pos == new Point(7, 3))
                {
                    // Arrived to the exit point
                    lastComponent.AddAdjacent(output);
                    output.AddAdjacent(lastComponent);
                    Debug.Log("Arrived to exit point");
                    continue;
                }
                
                // The wires that goes from pos to point different from lastPos
                var adjacentWires = _wires.FindAll(wire => wire.Pole1 == pos || wire.Pole2 == pos);
                adjacentWires.RemoveAll(wire => wire.Pole1 == lastPos || wire.Pole2 == lastPos);
                // The components that goes from pos to a point different from lastPos 
                var adjacentDipoles= _dipoles.FindAll(dipole => dipole.GetPoles().Contains(pos));
                adjacentDipoles.RemoveAll(dipole => dipole.GetPoles().Contains(lastPos));
                Debug.Log($"Found {adjacentWires.Count} wires and {adjacentDipoles.Count} dipoles.");
                
                if (adjacentWires.Count + adjacentDipoles.Count == 0)
                {
                    // This is a dead end
                    Debug.Log("Dead end");
                    break;
                }
                else if (adjacentWires.Count + adjacentDipoles.Count == 1)
                {
                    // The branch is continuing
                    if (adjacentWires.Count == 1)
                    {
                        // There is a wire
                        var wire = adjacentWires[0];
                        var newPos = wire.Pole1 == pos
                            ? wire.Pole2
                            : wire.Pole1;
                        queue.Enqueue((pos, newPos, lastComponent));
                    }
                    else
                    {
                        // There is a component
                        var vertex = adjacentDipoles[0].Inner;
                        // Manage the visited list
                        if (visitedDipoles.Contains(vertex)) continue;
                        visitedDipoles.Add(vertex);
                        graph.AddVertice(vertex);
                        // Add links
                        lastComponent.AddAdjacent(vertex);
                        vertex.AddAdjacent(lastComponent);
                        Debug.Log($"current pole = {pos}\nnext pole = {adjacentDipoles[0].GetOtherPole(pos)}");
                        queue.Enqueue((pos, adjacentDipoles[0].GetOtherPole(pos), vertex));
                    }
                }
                else
                {
                    Debug.Log("Node found");
                    // There is a node
                    var node = new Node($"{pos}");
                    // Manage the visited list
                    if (visitedNodes.Exists(n => n.Name == node.Name)) continue;
                    visitedNodes.Add(node);
                    // Add links
                    node.AddAdjacent(lastComponent);
                    lastComponent.AddAdjacent(node);
                    // Manage connected wires
                    foreach (var w in adjacentWires)
                    {
                        var newPos = w.Pole1 == pos
                            ? w.Pole2
                            : w.Pole1;
                        queue.Enqueue((pos, newPos, node));
                    }
                    // Manage connected dipoles
                    foreach (var d in adjacentDipoles)
                    {
                        var vertex = d.Inner;
                        // Manage visited
                        if (visitedDipoles.Contains(vertex)) continue;
                        visitedDipoles.Add(vertex);
                        graph.AddVertice(vertex);
                        // Add links
                        node.AddAdjacent(vertex);
                        vertex.AddAdjacent(node);
                        queue.Enqueue((pos, d.GetOtherPole(pos), vertex));
                    }
                }
            }

            return graph;
        }
        
        public void RegisterComponent(Dipole component)
        {
            if (_dipoles.Contains(component))
                return;
            _dipoles.Add(component);
        }

        public void UnRegisterComponent(Dipole component)
        {
            if (!_dipoles.Contains(component))
                return;
            _dipoles.Remove(component);
        }

        public Vector3 GetClosestValidPosition(Dipole dipole, Vector3 defaultPos)
        {
            var pos = GetClosestValidPosition(dipole);
            return pos ?? defaultPos;
        }

        public Vector3? GetClosestValidPosition(Dipole component)
        {
            var closest = new Vector3(
                ClosestHalf(component.transform.position.x + component.mainPoleAnchor.x) - component.mainPoleAnchor.x,
                ClosestHalf(component.transform.position.y + component.mainPoleAnchor.y) - component.mainPoleAnchor.y,
                zPositionDipoles);

            // The poles of the component if it was at the closest position
            var poles = component.GetPoles(closest);

            if (poles.Any(pole => pole.H is < 0 or >= 8 || pole.W is < 0 or >= 8))
            {
                // A pole is outside the breadboard
                Debug.Log("A pole is outside the breadboard");
                return null;
            }

            if (_dipoles.Exists(c => c.GetPoles().Intersect(poles).Count() >= 2))
            {
                // A component is already here
                Debug.Log("A component is already here");
                return null;
            }

            return closest;
        }

        // Returns the given number rounded to the closet half
        private static float ClosestHalf(float x)
        {
            return (float)Math.Round(x - 0.5f) + 0.5f;
        }
    }
}