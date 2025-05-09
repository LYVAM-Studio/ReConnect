using System;
using System.Collections.Generic;
using System.Linq;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.ResistorComponent;
using UnityEngine;
using UnityEngine.Serialization;

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
        /// Whether a wire is being created. It implies that the mouse is down.
        /// </summary>
        [NonSerialized] public bool OnWireCreation;

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

        private GameObject _wireBeingCreated;
        
        /// <summary>
        /// The YAML TextAsset of the circuit to be loaded on the breadboard.
        /// </summary>
        public TextAsset circuitToLoad;

        public static Vector3 PointToLocalPos(Vector2Int point)
            => new Vector3(
                -4f + point.x,
                3.5f - point.y,
                -0.5f);

        public static Vector2Int LocalPosToPoint(Vector3 pos)
            => new Vector2Int(
                (int)(pos.x + 4f),
                (int)(-pos.y + 3.5f));
        
        public Vector3 LocalToWorld(Vector3 localPos)
            => transform.position + transform.rotation * (transform.lossyScale.x * localPos);

        public Vector3 WorldToLocal(Vector3 worldPos)
            => Quaternion.Inverse(transform.rotation) * (worldPos - transform.position) / transform.lossyScale.x;

        private void Start()
        {
            OnWireCreation = false;
            _wireBeingCreated =
                Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/WirePrefab"), transform.parent, false);
            _wireBeingCreated.GetComponent<WireScript>().enabled = false;
            _wireBeingCreated.name = "WirePrefab (wireBeingCreated)";
            Loader.LoadCircuit(this, circuitToLoad);
        }
        
        private void Update()
        {
            if (breadboardHolder.IsActive)
            {
                if (OnWireCreation)
                {
                    _wireBeingCreated.SetActive(true);
                    _wireBeingCreated.transform.position =
                        (LocalToWorld(PointToLocalPos(_lastNodePoint)) + breadboardHolder.GetFlattenedCursorPos()) / 2;
                    _wireBeingCreated.transform.LookAt(breadboardHolder.GetFlattenedCursorPos());
                    _wireBeingCreated.transform.eulerAngles += new Vector3(90, 0, 0);
                    var scale = _wireBeingCreated.transform.lossyScale;
                    scale[1] = (_wireBeingCreated.transform.position - breadboardHolder.GetFlattenedCursorPos())
                        .magnitude;
                    _wireBeingCreated.transform.localScale = scale / transform.lossyScale.x;

                    if (_wireBeingCreated.transform.localScale.y > 0.9f)
                    {
                        _wireBeingCreated.SetActive(false);
                        OnWireCreation = false;
                    }
                }
            }
            else
            {
                _wireBeingCreated.SetActive(false);
                OnWireCreation = false;
            }
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
            var wireGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/WirePrefab"), transform.parent, false);
            wireGameObj.name = $"WirePrefab ({name})";
            wireGameObj.transform.localPosition = (PointToLocalPos(sourcePoint) + PointToLocalPos(destinationPoint)) / 2;
            wireGameObj.transform.LookAt(LocalToWorld(PointToLocalPos(destinationPoint)));
            wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            var scale = wireGameObj.transform.localScale;
            scale[1] /* y component */ = (PointToLocalPos(sourcePoint) - PointToLocalPos(destinationPoint)).magnitude / 2f;
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
            var resistorGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/ResistorPrefab"), transform.parent, false);
            resistorGameObj.name = $"ResistorPrefab ({name})";
            resistorGameObj.transform.localPosition = (PointToLocalPos(sourcePoint) + PointToLocalPos(destinationPoint)) / 2;
            resistorGameObj.transform.LookAt(LocalToWorld(PointToLocalPos(destinationPoint)));
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
            var lampGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/LampPrefab"), transform.parent, false);
            lampGameObj.name = $"LampPrefab ({name})";
            lampGameObj.transform.localPosition = (PointToLocalPos(sourcePoint) + PointToLocalPos(destinationPoint)) / 2;
            lampGameObj.transform.LookAt(LocalToWorld(PointToLocalPos(destinationPoint)));
            lampGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            LightBulb lightBulb = GetComponentInChildren<LightBulb>();
            // TODO : add exception component not found
            var inner = new Lamp(name, resistance, lightBulb);
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
            OnWireCreation = true;
            _lastNodePoint = nodePoint;
            _wireBeingCreated.SetActive(true);
        }

        public void EndWire()
        {
            OnWireCreation = false;
            _wireBeingCreated.SetActive(false);
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
            if (!OnWireCreation) return;

            // The difference between the two wire start position and the current mouse position, ignoring the z component
            // This vector corresponds to the future wire
            var delta = nodePoint - _lastNodePoint;

            if (delta.magnitude is < 1.5f and > 0)
            {
                var wireAtPos = Wires.Find(w =>
                    (w.Pole1 == _lastNodePoint && w.Pole2 == nodePoint) ||
                    (w.Pole2 == _lastNodePoint && w.Pole1 == nodePoint));
                var dipoleAtPos = Dipoles.Find(d =>
                    (d.Pole1 == _lastNodePoint && d.Pole2 == nodePoint) ||
                    (d.Pole2 == _lastNodePoint && d.Pole1 == nodePoint));
                if (wireAtPos == null && dipoleAtPos == null)
                    CreateWire(_lastNodePoint, nodePoint, $"_: {_lastNodePoint} <-> {nodePoint}");

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
            
            newPole1 = LocalPosToPoint(closest + component.MainPoleAnchor);
            newPole2 = LocalPosToPoint(closest - component.MainPoleAnchor);
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