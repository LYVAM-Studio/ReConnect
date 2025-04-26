using System;
using System.Collections.Generic;
using System.Linq;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class Breadboard : MonoBehaviour
    {
        /// <summary>
        /// The list of the components currently on the breadboard.
        /// </summary>
        public readonly List<Dipole> Dipoles = new List<Dipole>();
        
        /// <summary>
        /// The list of the wires currently on the breadboard.
        /// </summary>
        public readonly List<WireScript> Wires = new List<WireScript>();

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
        public ElecComponent Target { get; set; }

        /// <summary>
        /// The title of the circuit that has been loaded on this breadboard.
        /// </summary>
        [NonSerialized] public CircuitInfo CircuitInfo;
        
        private void Start()
        {
            _onWireCreation = false;
            _onDeletionMode = false;
            Loader.LoadCircuit(this, "2_parallel_lvl_2");
        }
        
        /// <summary>
        /// Cleans the breadboard i.e. deletes the registered wires and dipoles. It both destroys the game objects and removes their pointers in _wires and _dipoles.
        /// </summary>
        public void Clean()
        {
            foreach (WireScript wireScript in Wires)
            {
                Destroy(wireScript.gameObject);
            }
            foreach (Dipole dipole in Dipoles)
            {
                Destroy(dipole.gameObject);
            }

            Wires.Clear();
            Dipoles.Clear();
            Target = null;
        }
        
        public void CreateWire(Vector3 sourcePos, Vector3 destinationPos, string name, Point sourcePoint, Point destinationPoint, bool isLocked = false)
        {
            var wireGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/WirePrefab"), transform.parent, false);
            var wireScript = wireGameObj.GetComponent<WireScript>();
            wireGameObj.name = $"WirePrefab ({name})";
            wireScript.Init(this, sourcePoint, destinationPoint, isLocked);
            Wires.Add(wireScript);
            wireGameObj.transform.position = (sourcePos + destinationPos) / 2;
            wireGameObj.transform.LookAt(destinationPos);
            wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var scale = wireGameObj.transform.localScale;
            scale[1] /* y component */ = (destinationPos - sourcePos).magnitude / 2f;
            wireGameObj.transform.localScale = scale;
        }
        
        public Resistor CreateResistor(Vector3 sourcePos, Vector3 destinationPos, string name, float resistance, bool isLocked = false)
        {
            var resistorGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/ResistorPrefab"), transform.parent, false);
            var dipoleScript = resistorGameObj.GetComponent<Dipole>();
            resistorGameObj.name = $"ResistorPrefab ({name})";
            var inner = new Resistor(name, resistance);
            dipoleScript.Inner = inner;
            dipoleScript.Breadboard = this;
            dipoleScript.IsLocked = isLocked;
            resistorGameObj.transform.position = (sourcePos + destinationPos) / 2;
            resistorGameObj.transform.LookAt(destinationPos);
            resistorGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            Dipoles.Add(dipoleScript);
            return inner;
        }
        
        public Lamp CreateLamp(Vector3 sourcePos, Vector3 destinationPos, string name, float resistance, float nominalTension, bool isLocked = false)
        {
            var lampGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/LampPrefab"), transform.parent, false);
            var dipoleScript = lampGameObj.GetComponent<Dipole>();
            lampGameObj.name = $"LampPrefab ({name})";
            var inner = new Lamp(name, resistance, nominalTension);
            dipoleScript.Inner = inner;
            dipoleScript.Breadboard = this;
            dipoleScript.IsLocked = isLocked;
            lampGameObj.transform.position = (sourcePos + destinationPos) / 2;
            lampGameObj.transform.LookAt(destinationPos);
            lampGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            Dipoles.Add(dipoleScript);
            return inner;
        }
        
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
        
        public void DeleteWire(WireScript wire)
        {
            Wires.Remove(wire);
            Destroy(wire.gameObject);
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
                var wire = Wires.Find(w =>
                    (w.Pole1 == Point.VectorToPoint(_lastNodePosition) &&
                     w.Pole2 == Point.VectorToPoint(nodePosition)) ||
                    (w.Pole2 == Point.VectorToPoint(_lastNodePosition) &&
                     w.Pole1 == Point.VectorToPoint(nodePosition)));
                if (wire is not null)
                {
                    // A wire is already at this position
                    // Delete the wire at this position
                    if (!wire.IsLocked) DeleteWire(wire);
                    // Enter the deletion mode
                    _onDeletionMode = true;
                }
                else if (!_onDeletionMode)
                {
                    CreateWire(_lastNodePosition, nodePosition, $"_: {_lastNodePoint} <-> {nodePoint}",_lastNodePoint, nodePoint);
                }

                // Set the start to the current end
                _lastNodePosition = nodePosition;
                _lastNodePoint = nodePoint;
            }
        }
        
        public void RegisterComponent(Dipole component)
        {
            if (Dipoles.Contains(component))
                return;
            Dipoles.Add(component);
        }

        public void UnRegisterComponent(Dipole component)
        {
            if (!Dipoles.Contains(component))
                return;
            Dipoles.Remove(component);
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

            if (Dipoles.Exists(c => c.GetPoles().Intersect(poles).Count() >= 2))
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