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
            Target = null;
        }

        public void LoadCircuit(string circuitName)
        {
            Clean();
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, $"CircuitsPresets/{circuitName}.csv");
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
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
                        var inner = new Resistor($"R{i}", r);
                        dipoleScript.Inner = inner;
                        dipoleScript.Breadboard = this;
                        dipoleScript.SetPosition(fromPos, toPos);
                        _dipoles.Add(dipoleScript);
                        // if (Target is null) Target = inner;
                    }
                    else if (content[0] == "lamp")
                    {
                        (int h, int w) from = (int.Parse(content[1]), int.Parse(content[2]));
                        (int h, int w) to = (int.Parse(content[3]), int.Parse(content[4])); 
                        double r = double.Parse(content[5], CultureInfo.InvariantCulture);
                        double tension = double.Parse(content[6], CultureInfo.InvariantCulture);
                        Point fromPos = new Point(from.h, from.w);
                        Point toPos = new Point(to.h, to.w);
                        var lampGameObj = Instantiate(Helper.GetPrefabByName("Components/LampPrefab"));
                        if (lampGameObj is null)
                            throw new Exception("The lamp prefab could not be found.");
                        var dipoleScript = lampGameObj.GetComponent<Dipole>();
                        if (dipoleScript is null)
                            throw new Exception("The Dipole script component could not be found in the wire prefab.");
                        var inner = new Lamp($"L{i}", r, tension);
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


        public Graph CreateGraph()
        {
            // Debug.Log("WIRES:" + string.Join(", ",
            //                        from wire in _wires select "(" + wire.Pole1 + " " + wire.Pole2 + ")")
            //                    + "\nDIPOLES:" + string.Join(", ",
            //                        from dipole in _dipoles
            //                        select "(" + dipole.GetPoles()[0] + " " + dipole.GetPoles()[1] + ")"));
            var input = new CircuitInput("input", 240, 16);
            var output = new CircuitOutput("output");
            var graph = new Graph("Main graph", input, output, Target);
            var queue = new Queue<(Point lastPosition, Point position, Vertex lastComponent, Vector2 lastCmpntPos)>();
            var createdLiasons = new List<Liason>();
            queue.Enqueue((new Point(-1, -1), new Point(0, 3), input, new Vector2(0, 3)));
            while (queue.Count > 0)
            {
                var (lastPos, pos, lastComponent, lastCmpntPos) = queue.Dequeue();

                if (pos == new Point(7, 3))
                {
                    // Arrived to the exit point
                    Vertex.ReciprocalAddAdjacent(lastComponent, output);
                    continue;
                }
                
                // The wires that goes from pos to point different from lastPos
                var adjacentWires = _wires.FindAll(wire => wire.Pole1 == pos || wire.Pole2 == pos);
                adjacentWires.RemoveAll(wire => wire.Pole1 == lastPos || wire.Pole2 == lastPos);
                // The components that goes from pos to a point different from lastPos 
                var adjacentDipoles= _dipoles.FindAll(dipole => dipole.GetPoles().Contains(pos));
                adjacentDipoles.RemoveAll(dipole => dipole.GetPoles().Contains(lastPos));
                
                if (adjacentWires.Count + adjacentDipoles.Count == 1) // The branch is continuing
                {
                    if (adjacentWires.Count == 1) // There is a wire
                    {
                        var wire = adjacentWires[0];
                        var newPos = wire.Pole1 == pos ? wire.Pole2 : wire.Pole1;
                        queue.Enqueue((pos, newPos, lastComponent, lastCmpntPos));
                    }
                    else // There is a component
                    {
                        var dipole = adjacentDipoles[0];
                        var liaison = new Liason(lastCmpntPos, (Vector2)dipole.GetPoles().Aggregate((p1, p2) => p1 + p2)/2);
                        if (createdLiasons.Contains(liaison)) continue;
                        createdLiasons.Add(liaison);
                        var vertex = dipole.Inner;
                        graph.AddVertex(vertex);
                        Vertex.ReciprocalAddAdjacent(lastComponent, vertex);
                        queue.Enqueue((pos, dipole.GetOtherPole(pos), vertex, (Vector2)pos));
                    }
                }
                else if (adjacentWires.Count + adjacentDipoles.Count > 1) // There is a node 
                {
                    var node = new Node($"{pos}");
                    Vertex.ReciprocalAddAdjacent(lastComponent, node);
                    // Manage connected wires
                    foreach (var w in adjacentWires)
                    {
                        var newPos = w.Pole1 == pos
                            ? w.Pole2
                            : w.Pole1;
                        queue.Enqueue((pos, newPos, node, (Vector2)pos));
                    }
                    // Manage connected dipoles
                    foreach (var dipole in adjacentDipoles)
                    {
                        var liaison = new Liason(lastCmpntPos, (Vector2)(dipole.GetPoles().Aggregate((p1, p2) => p1 + p2))/2);
                        if (createdLiasons.Contains(liaison)) continue;
                        createdLiasons.Add(liaison);
                        var vertex = dipole.Inner;
                        graph.AddVertex(vertex);
                        Vertex.ReciprocalAddAdjacent(vertex, node);
                        queue.Enqueue((pos, dipole.GetOtherPole(pos), vertex, (Vector2)dipole.GetOtherPole(pos)));
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
                // Debug.Log("A pole is outside the breadboard");
                return null;
            }

            if (_dipoles.Exists(c => c.GetPoles().Intersect(poles).Count() >= 2))
            {
                // A component is already here
                // Debug.Log("A component is already here");
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