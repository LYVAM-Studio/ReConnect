using System;
using System.Collections.Generic;
using System.Linq;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.ResistorComponent;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class Breadboard : MonoBehaviour
    {
        public BreadboardHolder breadboardHolder;
        
        /// <summary>
        /// The list of the components currently on the breadboard.
        /// </summary>
        public readonly List<Dipole> Dipoles = new List<Dipole>();
        
        /// <summary>
        /// The list of the wires currently on the breadboard.
        /// </summary>
        public readonly List<WireScript> Wires = new List<WireScript>();

        /// <summary>
        /// The "virtual" start position of the wire being created. Equivalent to _lastNodePosition.
        /// </summary>
        private Vector2Int _lastNodePoint;

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
        private const float ZPositionDipoles = -0.5f;

        /// <summary>
        /// The target of the currently loaded circuit.
        /// </summary>
        public ElecComponent Target { get; set; }

        /// <summary>
        /// The title of the circuit that has been loaded on this breadboard.
        /// </summary>
        [NonSerialized] public CircuitInfo CircuitInfo;


        public static Vector3 PointToPos(Vector2Int point)
            => new Vector3(
                -3.5f + point.x,
                3.5f - point.y,
                -0.5f);

        public static Vector2Int PosToPoint(Vector3 pos)
            => new Vector2Int(
                (int)(pos.x + 3.5f),
                (int)(-pos.y + 3.5f));
        
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
        
        public void CreateWire(Vector2Int sourcePoint, Vector2Int destinationPoint, string name, bool isLocked = false)
        {
            var wireGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/WirePrefab"), transform.parent, false);
            wireGameObj.name = $"WirePrefab ({name})";
            wireGameObj.transform.localPosition = (PointToPos(sourcePoint) + PointToPos(destinationPoint)) / 2;
            wireGameObj.transform.LookAt(transform.position + transform.rotation * (transform.lossyScale.x * PointToPos(destinationPoint)));
            wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var scale = wireGameObj.transform.localScale;
            scale[1] /* y component */ = (PointToPos(sourcePoint) - PointToPos(destinationPoint)).magnitude / 2f;
            wireGameObj.transform.localScale = scale;
            var wireScript = wireGameObj.GetComponent<WireScript>();
            wireScript.Breadboard = this;
            wireScript.Pole1 = sourcePoint;
            wireScript.Pole2 = destinationPoint;
            wireScript.IsLocked = isLocked;
            Wires.Add(wireScript);
        }
        
        public Resistor CreateResistor(Vector2Int sourcePoint, Vector2Int destinationPoint, string name, float resistance, float tolerance, bool isLocked = false)
        {
            var resistorGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/ResistorPrefab"), transform.parent, false);
            resistorGameObj.name = $"ResistorPrefab ({name})";
            resistorGameObj.transform.localPosition = (PointToPos(sourcePoint) + PointToPos(destinationPoint)) / 2;
            resistorGameObj.transform.LookAt(transform.position + transform.rotation * (transform.lossyScale.x * PointToPos(destinationPoint)));
            resistorGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var resistorColor = resistorGameObj.GetComponent<ResistorColorManager>();
            resistorColor.ResistanceValue = resistance;
            resistorColor.Tolerance = tolerance;
            resistorColor.UpdateBandColors();
            var inner = new Resistor(name, resistance);
            var dipoleScript = resistorGameObj.GetComponent<Dipole>();
            dipoleScript.Pole1 = sourcePoint;
            dipoleScript.Pole2 = destinationPoint;
            dipoleScript.IsHorizontal = (destinationPoint - sourcePoint).y == 0;
            dipoleScript.Breadboard = this;
            dipoleScript.IsLocked = isLocked;
            dipoleScript.Inner = inner;
            Dipoles.Add(dipoleScript);
            return inner;
        }
        
        public Lamp CreateLamp(Vector2Int sourcePoint, Vector2Int destinationPoint, string name, float resistance, float nominalTension, bool isLocked = false)
        {
            var lampGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Components/LampPrefab"), transform.parent, false);
            lampGameObj.name = $"LampPrefab ({name})";
            lampGameObj.transform.localPosition = (PointToPos(sourcePoint) + PointToPos(destinationPoint)) / 2;
            lampGameObj.transform.LookAt(transform.position + transform.rotation * (transform.lossyScale.x * PointToPos(destinationPoint)));
            lampGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var inner = new Lamp(name, resistance, nominalTension);
            var dipoleScript = lampGameObj.GetComponent<Dipole>();
            dipoleScript.Pole1 = sourcePoint;
            dipoleScript.Pole2 = destinationPoint;
            dipoleScript.IsHorizontal = (destinationPoint - sourcePoint).y == 0;
            dipoleScript.Breadboard = this;
            dipoleScript.IsLocked = isLocked;
            dipoleScript.Inner = inner;
            Dipoles.Add(dipoleScript);
            return inner;
        }
        
        public void StartWire(Vector2Int nodePoint)
        {
            _onWireCreation = true;
            _onDeletionMode = false;
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
        public void OnMouseNodeCollision(Vector2Int nodePoint)
        {
            // If not no wire creation, then does nothing
            if (!_onWireCreation) return;

            // The difference between the two wire start position and the current mouse position, ignoring the z component
            // This vector corresponds to the future wire
            var delta = nodePoint - _lastNodePoint;

            // nodes are spaced by 1.0f, the diagonal distance would be sqrt(2) ~ 1.41
            // We check if the distance is greater because we want to avoid skipping surrounding nodes.
            if (delta.magnitude > 1.5f)
            {
                // The user skipped one or more node. A wire cannot be created that way
                // Enter in deletion mode to delete wires if the users wants to
                _onDeletionMode = true;
                // Set the start to the current end
                _lastNodePoint = nodePoint;
            }
            else if (delta != Vector2Int.zero)
            {
                // Is null if a wire is not already at the given position. Otherwise, contains the wire.
                var wireAtPos = Wires.Find(w =>
                    (w.Pole1 == _lastNodePoint && w.Pole2 == nodePoint) ||
                    (w.Pole2 == _lastNodePoint && w.Pole1 == nodePoint));
                var dipoleAtPos = Dipoles.Find(d =>
                    (d.Pole1 == _lastNodePoint && d.Pole2 == nodePoint) ||
                    (d.Pole2 == _lastNodePoint && d.Pole1 == nodePoint));
                if (wireAtPos != null)
                {
                    // A wire is already at this position
                    // Delete the wire at this position
                    if (!wireAtPos.IsLocked) DeleteWire(wireAtPos);
                    // Enter the deletion mode
                    _onDeletionMode = true;
                }
                else if (dipoleAtPos == null && !_onDeletionMode)
                {
                    CreateWire(_lastNodePoint, nodePoint, $"_: {_lastNodePoint} <-> {nodePoint}");
                }

                // Set the start to the current end
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
        
        public bool TryGetClosestValidPos(Dipole component, out Vector3 closest, out Vector2Int newPole1, out Vector2Int newPole2)
        {
            closest = new Vector3(
                ClosestHalf(component.transform.localPosition.x + component.MainPoleAnchor.x) - component.MainPoleAnchor.x,
                ClosestHalf(component.transform.localPosition.y + component.MainPoleAnchor.y) - component.MainPoleAnchor.y,
                ZPositionDipoles);
            
            newPole1 = PosToPoint(closest + component.MainPoleAnchor);
            newPole2 = PosToPoint(closest - component.MainPoleAnchor);
            var newPoles = new[] { newPole1, newPole2 };

            if (newPoles.Any(pole => pole.x is < 0 or >= 8 || pole.y is < 0 or >= 8))
                return false; // A pole is outside the breadboard

            if (Dipoles.Concat<IDipole>(Wires).Any(d => d.GetPoles().Intersect(newPoles).Count() >= 2))
                return false; // A component is already here

            return true;
        }
        
        // Returns the given number rounded to the closet half
        private static float ClosestHalf(float x)
            => (float)Math.Round(x - 0.5f) + 0.5f;
    }
}