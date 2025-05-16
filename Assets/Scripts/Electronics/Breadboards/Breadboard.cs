using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.ResistorComponent;
using Reconnect.Player;
using Reconnect.ToolTips;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class Breadboard : NetworkBehaviour
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
        /// The target of the currently loaded circuit. ElecComponent
        /// </summary>
        [SyncVar] [NonSerialized] public Uid TargetUid;

        /// <summary>
        /// The title of the circuit that has been loaded on this breadboard.
        /// </summary>
        [NonSerialized] public CircuitInfo CircuitInfo;

        private GameObject _wireBeingCreated;
        
        /// <summary>
        /// The YAML TextAsset of the circuit to be loaded on the breadboard.
        /// </summary>
        public TextAsset circuitToLoad;
        
        /// <summary>
        /// The GameObject that holds all the dependencies of the breadboard switch
        /// </summary>
        public GameObject switchHolder;

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

        private bool _isInitialized; 
        public override void OnStartServer()
        {
            Debug.Log("Server started");
            OnWireCreation = false;
            Loader.LoadCircuit(this, circuitToLoad);
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("Client started");
            OnWireCreation = false;
            _wireBeingCreated =
                Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/WirePrefab"), transform.parent, false);
            _wireBeingCreated.GetComponent<WireScript>().enabled = false;
            _wireBeingCreated.name = "WirePrefab (wireBeingCreated)";
            _isInitialized = true;
        }
        
        private void Update()
        {
            if (isServerOnly || !_isInitialized)
                return;
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
                NetworkServer.Destroy(wireScript.gameObject);
            }
            foreach (Dipole dipole in Dipoles)
            {
                NetworkServer.Destroy(dipole.gameObject);
            }

            Wires.Clear();
            Dipoles.Clear();
            TargetUid = null;
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
            
            if (!wireGameObj.TryGetComponent(out WireScript wireScript))
                throw new ComponentNotFoundException(
                    "The wire prefab clone does not contain any WireScript component.");
            NetworkServer.Spawn(wireGameObj);
            wireScript.Pole1 = sourcePoint;
            wireScript.Pole2 = destinationPoint;
            wireScript.IsLocked = isLocked;
            wireScript.breadboardNetIdentity = netIdentity;
            Wires.Add(wireScript);
        }
        
        public Uid CreateResistor(Vector2Int sourcePoint, Vector2Int destinationPoint, string name, uint resistance, float tolerance, bool isLocked = false)
        {
            var resistorGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/ResistorPrefab"), transform.parent, false);
            
            resistorGameObj.name = $"ResistorPrefab ({name})";
            resistorGameObj.transform.localPosition = (PointToLocalPos(sourcePoint) + PointToLocalPos(destinationPoint)) / 2;
            resistorGameObj.transform.LookAt(LocalToWorld(PointToLocalPos(destinationPoint)));
            resistorGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            
            if (!resistorGameObj.TryGetComponent(out ResistorColorManager resistorColor))
                throw new ComponentNotFoundException(
                    "The resistor prefab clone does not contain any ResistorColorManager component.");
            resistorColor.ResistanceValue = resistance;
            resistorColor.Tolerance = tolerance;

            var innerUid = UidDictionary.Add(new Resistor(name, resistance));
            if (!resistorGameObj.TryGetComponent(out Dipole dipoleScript))
                throw new ComponentNotFoundException(
                    "The resistor prefab clone does not contain any Dipole component.");
            dipoleScript.breadboardNetIdentity = netIdentity;
            NetworkServer.Spawn(resistorGameObj);
            dipoleScript.Pole1 = sourcePoint;
            dipoleScript.Pole2 = destinationPoint;
            dipoleScript.IsHorizontal = (destinationPoint - sourcePoint).y == 0;
            dipoleScript.IsLocked = isLocked;
            dipoleScript.InnerUid = innerUid;
            Dipoles.Add(dipoleScript);
            
            if (!resistorGameObj.TryGetComponent(out HoverToolTip tooltipScript))
                throw new ComponentNotFoundException(
                    "The resistor prefab clone does not contain any TooltipScript component.");
            tooltipScript.Text = $"{resistance} Ω";
            
            return innerUid;
        }
        
        public Uid CreateLamp(Vector2Int sourcePoint, Vector2Int destinationPoint, string name, uint resistance, bool isLocked = false)
        {
            var lampGameObj = Instantiate(Resources.Load<GameObject>("Prefabs/Electronics/Components/LampPrefab"), transform.parent, false);
            lampGameObj.name = $"LampPrefab ({name})";
            lampGameObj.transform.localPosition = (PointToLocalPos(sourcePoint) + PointToLocalPos(destinationPoint)) / 2;
            lampGameObj.transform.LookAt(LocalToWorld(PointToLocalPos(destinationPoint)));
            lampGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
            LightBulb lightBulb = lampGameObj.GetComponentInChildren<LightBulb>();
            if (lightBulb is null)
                throw new ComponentNotFoundException(
                    "The lamp prefab does not contain LightBulb component in its children");
            var innerUid = UidDictionary.Add(new Lamp(name, resistance, lightBulb));
            if (!lampGameObj.TryGetComponent(out Dipole dipoleScript))
                throw new ComponentNotFoundException(
                    "The lamp prefab clone does not contain any Dipole component.");
            Debug.Log($"When spawn lamp : isServer {isServer}");
            dipoleScript.breadboardNetIdentity = netIdentity;
            NetworkServer.Spawn(lampGameObj);
            dipoleScript.Pole1 = sourcePoint;
            dipoleScript.Pole2 = destinationPoint;
            dipoleScript.IsHorizontal = (destinationPoint - sourcePoint).y == 0;
            dipoleScript.IsLocked = isLocked;
            dipoleScript.InnerUid = innerUid;
            Dipoles.Add(dipoleScript);
            
            return innerUid;
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
                if (wireAtPos is null && dipoleAtPos is null)
                {
                    if (!NetworkClient.localPlayer.TryGetComponent(out PlayerNetwork playerNetwork))
                        throw new ComponentNotFoundException("No component PlayerNetwork has been found on the local player");
                    playerNetwork.CmdRequestCreateWire(netIdentity, _lastNodePoint, nodePoint, 
                        $"_: {_lastNodePoint} <-> {nodePoint}", false);
                }

                // Set the start to the current end
                _lastNodePoint = nodePoint;
            }
        }
        // TODO : command ?
        public void RegisterComponent(Dipole component)
        {
            if (Dipoles.Contains(component))
                return;
            Dipoles.Add(component);
        }
        // TODO : command ?
        public void UnRegisterComponent(Dipole component)
        {
            if (!Dipoles.Contains(component))
                return;
            Dipoles.Remove(component);
        }
        
        public bool TryGetClosestValidPos(Dipole component, out Vector3 closest, out Vector2Int newPole1, out Vector2Int newPole2)
        {
            closest = new Vector3(
                (float)Math.Round(component.transform.localPosition.x + component.MainPoleAnchor.x) - component.MainPoleAnchor.x,
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