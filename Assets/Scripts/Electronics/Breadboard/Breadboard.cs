using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;
using UnityEngine;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace Reconnect.Electronics.Breadboards
{
    public class Breadboard : MonoBehaviour
    {
        /// <summary>
        /// The list of the components currently on the breadboard.
        /// </summary>
        private readonly List<Dipole> _dipoles = new List<Dipole>();
        
        /// <summary>
        /// The list of the wires currently on the breadboard.
        /// </summary>
        private readonly List<WireScript> _wires = new List<WireScript>();

        /// <summary>
        /// The start position of the wire being created. Equivalent to _lastNodePoint.
        /// </summary>
        private Vector3 _lastNodePosition;
        
        /// <summary>
        /// The "virtual" start position of the wire being created. Equivalent to _lastNodePosition.
        /// </summary>
        private Point _lastNodePoint;

        /// <summary>
        /// Whether the user has entered the "deletion mode" which destroys every wire touched with the mouse.
        /// </summary>
        private bool _onDeletionMode;

        /// <summary>
        /// Whether a wire is being created. It implies that the mouse is down.
        /// </summary>
        private bool _onWireCreation;

        /// <summary>
        /// The Z coordinate at which the dipoles are positioned on the breadboard. It is the Z position of the breadboard (8f) minus half its thickness (1f / 2) to have it sunk into the board.
        /// </summary>
        public float zPositionDipoles = 7.5f;

        /// <summary>
        /// The target of the currently loaded circuit.
        /// </summary>
        public ElecComponent Target { get; private set; }
        
        private void Start()
        {
            _onWireCreation = false;
            _onDeletionMode = false;
            LoadCircuit("2_parallel_lvl_2");
        }
        
        
        // #####################################
        // #          CIRCUIT LOADING          #
        // #####################################
        
        
        /// <summary>
        /// Cleans the breadboard i.e. deletes the registered wires and dipoles. It both destroys the game objects and removes their pointers in _wires and _dipoles.
        /// </summary>
        private void Clean()
        {
            foreach (WireScript wireScript in _wires)
            {
                Destroy(wireScript.gameObject);
            }
            foreach (Dipole dipole in _dipoles)
            {
                Destroy(dipole.gameObject);
            }

            _wires.Clear();
            _dipoles.Clear();
            Target = null;
        }
        
        /// <summary>
        /// Returns the value associated with the yaml node in the given children that has the argument `key` as key.
        /// </summary>
        /// <param name="children">The children of a node in which the element in searched.</param>
        /// <param name="key">The key of the sought yaml node.</param>
        /// <returns>The value of the node.</returns>
        /// <example>
        /// <code>
        /// node:
        ///     child1: value1
        ///     child2: value2
        /// </code>
        /// For instance, `value2` can be got calling this function with `node.Children` and `child2`.
        /// </example>
        /// <exception cref="Exception">Throws an exception if the sought key is not found.</exception>
        private static string YamlGetScalarValue(IOrderedDictionary<YamlNode, YamlNode> children, string key)
        {
            if (!children.TryGetValue(new YamlScalarNode(key), out YamlNode nameNode))
                throw new Exception("Yaml error");
            return ((YamlScalarNode)nameNode).Value!;
        }
        
        /// <summary>
        /// Shifts the given point to the given direction by 1.
        /// </summary>
        /// <param name="sourcePoint"> The initial point.</param>
        /// <param name="direction">The direction of the shift. It is represented by cardinal points in a string (for instance "n", "se"...).</param>
        /// <param name="allowDiagonal">Whether the direction can be composed of two cardinal points ("ne", "sw"...) or only one ("n", "e"). By default, the diagonals are not allowed.</param>
        /// <returns>Returns a new point shifted by 1 in the given direction from the given point.</returns>
        /// <exception cref="Exception">Throws an exception if the direction is not valid.</exception>
        private static Point ShiftToDirection(Point sourcePoint, string direction, bool allowDiagonal = false)
        {
            return direction.ToLower().Trim() switch
            {
                "n"  => sourcePoint.Shift(-1,  0),
                "e"  => sourcePoint.Shift( 0, +1),
                "s"  => sourcePoint.Shift(+1,  0),
                "w"  => sourcePoint.Shift( 0, -1),
                "ne" when allowDiagonal => sourcePoint.Shift(-1, +1),
                "se" when allowDiagonal => sourcePoint.Shift(+1, +1),
                "sw" when allowDiagonal => sourcePoint.Shift(+1, -1),
                "nw" when allowDiagonal => sourcePoint.Shift(-1, -1),
                { } dir => throw new Exception(allowDiagonal
                    ? $"Invalid direction. Expected 'n', 'e', 's', 'w', 'ne', 'se', 'sw' or 'nw' but got {dir}."
                    : $"Invalid direction. Expected 'n', 'e', 's' or 'w' but got '{dir}'.")
            };
        }
        
        /// <summary>
        /// Loads the given circuit. Cleans the breadboard and load all the components required for the circuit.
        /// </summary>
        /// <param name="circuitName">The name of the circuit to be loaded. It must contain neither the extension (.yaml) nor the path (CircuitsPresets/).</param>
        public void LoadCircuit(string circuitName)
        {
            Clean();
            
            string circuitPath = Path.Combine(Application.streamingAssetsPath, $"CircuitsPresets/{circuitName}.yaml");

            using StreamReader reader = new StreamReader(circuitPath);
            YamlStream yaml = new YamlStream();
            yaml.Load(reader);

            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;
            
            string title = YamlGetScalarValue(root.Children, "title");
            float inputTension = float.Parse(YamlGetScalarValue(root.Children, "input-tension"));
            float inputIntensity = float.Parse(YamlGetScalarValue(root.Children, "input-intensity"));
            float targetTension = float.Parse(YamlGetScalarValue(root.Children, "target-tension"));
            
            YamlSequenceNode componentsNode = (YamlSequenceNode)root.Children[new YamlScalarNode("components")];

            int componentId = 0;
            
            foreach (YamlNode item in componentsNode)
            {
                YamlMappingNode component = (YamlMappingNode)item;
                string type = ((YamlScalarNode)component.Children.First().Key).Value;
                    
                // Fields in common (for any type of component) 

                string name = $"{componentId}: ";
                if (component.Children.TryGetValue(new YamlScalarNode("name"), out YamlNode nameNode))
                    name += ((YamlScalarNode)nameNode).Value;
                else
                    name += $"nameless {type}";
                int height = int.Parse(YamlGetScalarValue(component.Children, "height"));
                int width = int.Parse(YamlGetScalarValue(component.Children, "width"));
                string direction = YamlGetScalarValue(component.Children, "direction");
                bool isTarget = component.Children.ContainsKey(new YamlScalarNode("target"));
                
                Point sourcePoint = new Point(height, width);
                Point destinationPoint = ShiftToDirection(sourcePoint, direction, allowDiagonal: type == "wire");

                Vector3 sourcePos = Point.PointToVector(sourcePoint, zPositionDipoles);
                Vector3 destinationPos = Point.PointToVector(destinationPoint, zPositionDipoles);
                
                // Component-specific fields
                
                if (type == "wire")
                {
                    CreateWire(sourcePos, destinationPos, name, sourcePoint, destinationPoint);
                    // isTarget is ignored because a wire cannot be a target
                }
                else if (type == "resistor")
                {
                    float resistance = float.Parse(YamlGetScalarValue(component.Children, "resistance"));
                    
                    Resistor resistor = CreateResistor(sourcePos, destinationPos, name, resistance);
                    if (Target == null && isTarget) Target = resistor;
                }
                else if (type == "lamp")
                {
                    float resistance = float.Parse(YamlGetScalarValue(component.Children, "resistance"));
                    float nominalTension = float.Parse(YamlGetScalarValue(component.Children, "nominal-tension"));
                    
                    Lamp lamp = CreateLamp(sourcePos, destinationPos, name, resistance, nominalTension);
                    if (Target == null && isTarget) Target = lamp;
                }

                componentId++;
            }
        }

        public void CreateWire(Vector3 sourcePos, Vector3 destinationPos, string name, Point sourcePoint, Point destinationPoint)
        {
            var wireGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/WirePrefab"));
            var wireScript = wireGameObj.GetComponent<WireScript>();
            wireGameObj.name = $"WirePrefab ({name})";
            wireScript.Init(this, sourcePoint, destinationPoint);
            _wires.Add(wireScript);
            wireGameObj.transform.position = (sourcePos + destinationPos) / 2;
            wireGameObj.transform.LookAt(destinationPos);
            wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var scale = wireGameObj.transform.localScale;
            scale[1] /* y component */ = (destinationPos - sourcePos).magnitude / 2f;
            wireGameObj.transform.localScale = scale;
        }
        
        private Resistor CreateResistor(Vector3 sourcePos, Vector3 destinationPos, string name, float resistance)
        {
            var resistorGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/ResistorPrefab"));
            var dipoleScript = resistorGameObj.GetComponent<Dipole>();
            resistorGameObj.name = $"ResistorPrefab ({name})";
            var inner = new Resistor(name, resistance);
            dipoleScript.Inner = inner;
            dipoleScript.Breadboard = this;
            resistorGameObj.transform.position = (sourcePos + destinationPos) / 2;
            resistorGameObj.transform.LookAt(destinationPos);
            resistorGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            _dipoles.Add(dipoleScript);
            return inner;
        }
        
        private Lamp CreateLamp(Vector3 sourcePos, Vector3 destinationPos, string name, float resistance, float nominalTension)
        {
            var lampGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/LampPrefab"));
            var dipoleScript = lampGameObj.GetComponent<Dipole>();
            lampGameObj.name = $"LampPrefab ({name})";
            var inner = new Lamp(name, resistance, nominalTension);
            dipoleScript.Inner = inner;
            dipoleScript.Breadboard = this;
            lampGameObj.transform.position = (sourcePos + destinationPos) / 2;
            lampGameObj.transform.LookAt(destinationPos);
            lampGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            _dipoles.Add(dipoleScript);
            return inner;
        }
        
        
        // #####################################
        // #          WIRE MANAGEMENT          #
        // #####################################
        
        
        public void StartWire(Vector3 nodePosition, Point nodePoint)
        {
            _onWireCreation = true;
            _onDeletionMode = false;
            _lastNodePosition = nodePosition;
            _lastNodePoint = nodePoint;
        }

        public void EndWire()
        {
            _onWireCreation = false;
        }

        // This function is called by a breadboard node when the mouse collides it
        public void OnMouseNodeCollision(Vector3 nodePosition, Point nodePoint)
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
                    CreateWire(_lastNodePosition, nodePosition, "user created",_lastNodePoint, nodePoint);
                }

                // Set the start to the current end
                _lastNodePosition = nodePosition;
                _lastNodePoint = nodePoint;
            }
        }

        public void DeleteWire(WireScript wire)
        {
            _wires.Remove(wire);
            Destroy(wire.gameObject);
        }
        

        // #####################################
        // #         GRAPH MANAGEMENT          #
        // #####################################
        
        public Graph CreateGraph()
        {
            // clear the adjacent relation of the dipoles (avoid branch duplication)
            foreach (var d in _dipoles)
                d.Inner.ClearAdjacent();
            // inner vertices can be null!
            Vertex[,] grid = new Vertex[8, 8];
            PlaceWires(grid);
            PlaceDipoles(grid);
            var input = new CircuitInput("input", 240, 16);
            Vertex.ReciprocalAddAdjacent(GetVertexOrNewAt(grid, 0, 3), input);
            var output = new CircuitOutput("output");
            Vertex.ReciprocalAddAdjacent(GetVertexOrNewAt(grid, 7, 3), output);
            var graph = new Graph("Main graph", input, output, Target);
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

            foreach (var d in _dipoles)
            {
                graph.AddVertex(d.Inner);
            }

            return graph;
        }

        private Vertex GetVertexOrNewAt(Vertex[,] grid, int h, int w)
        {
            if (grid[h, w] is null)
            {
                grid[h, w] = new Vertex($"({h}, {w})");
            }

            return grid[h, w];
        }

        private void PlaceWires(Vertex[,] grid)
        {
            foreach (var w in _wires)
            {
                Vertex v1 = GetVertexOrNewAt(grid, w.Pole1.H, w.Pole1.W);
                Vertex v2 = GetVertexOrNewAt(grid, w.Pole2.H, w.Pole2.W);
                Vertex.ReciprocalAddAdjacent(v1, v2);
            }
        }
        
        private void PlaceDipoles(Vertex[,] grid)
        {
            foreach (var d in _dipoles)
            {
                Vertex v1 = GetVertexOrNewAt(grid, d.GetPoles()[0].H, d.GetPoles()[0].W);
                Vertex v2 = GetVertexOrNewAt(grid, d.GetPoles()[1].H, d.GetPoles()[1].W);
                Vertex.ReciprocalAddAdjacent(v1, d.Inner);
                Vertex.ReciprocalAddAdjacent(d.Inner, v2);
            }
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