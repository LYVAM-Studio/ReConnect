using System;
using System.Collections.Generic;
using Reconnect.Electronics.Components;
using Unity.VisualScripting;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Breadboard : MonoBehaviour
    {
        // The list of the components in the breadboard
        private List<ElecComponent> _components;
        private List<WireScript> _wires;
        private bool _onWireCreation;
        private bool _onDeletionMode;
        private Vector3 _wireStart;

        void Start()
        {
            _components = new List<ElecComponent>();
            _wires = new List<WireScript>();
            _onWireCreation = false;
            _onDeletionMode = false;
        }
        
        // #########################
        // #    WIRE MANAGEMENT    #
        // #########################

        public void StartWire(Vector3 nodePosition)
        {
            _onWireCreation = true;
            _onDeletionMode = false;
            _wireStart = nodePosition;
        }

        public void EndWire()
        {
            _onWireCreation = false;
        }

        public void OnNodeCollision(Vector3 nodePosition)
        {
            if (!_onWireCreation) return;
            
            // The difference between the two positions, ignoring the z component.
            var delta = (Vector2)nodePosition - (Vector2)_wireStart;

            if (delta.magnitude > 1.5f)
            {
                // The user skipped one or more node. A wire cannot be created that way.
                _onDeletionMode = true;
                // set the start to the current end
                _wireStart = nodePosition;
            }
            else if (delta != Vector2.zero)
            {
                // Is null if a wire is not already at the given position. Otherwise, contains the wire.
                var wire = _wires.Find(w =>
                    w.Pole1 == PositionToPole(_wireStart) && w.Pole2 == PositionToPole(nodePosition) ||
                    w.Pole2 == PositionToPole(_wireStart) && w.Pole1 == PositionToPole(nodePosition));
                if (wire is not null)
                {
                    DeleteWire(wire.GetComponent<WireScript>());
                    Destroy(wire.gameObject);
                    _onDeletionMode = true;
                }
                else if (!_onDeletionMode) // create a new wire
                {
                    // instantiate a wire from the wire prefab
                    var wireGameObj = Instantiate(Helper.GetPrefabByName("Components/WirePrefab"));
                    if (wireGameObj is null)
                        throw new Exception("The wire prefab could not be found.");
                    var wireScript = wireGameObj.GetComponent<WireScript>();
                    if (wireScript is null)
                        throw new Exception("The WireScript component could not be found in the wire prefab.");
                    wireGameObj.name = $"WirePrefab (Clone {(uint)wireScript.GetHashCode()})";
                    wireScript.Init(this, PositionToPole(_wireStart), PositionToPole(nodePosition));
                    _wires.Add(wireScript);
                    // set position
                    wireGameObj.transform.position = (_wireStart + nodePosition) / 2;
                    // set rotation
                    wireGameObj.transform.LookAt(nodePosition);
                    wireGameObj.transform.eulerAngles += new Vector3(90, 0, 0);
                    // set scale (length of the wire)
                    var scale = wireGameObj.transform.localScale;
                    scale[1] = (nodePosition - _wireStart).magnitude / 2f;
                    wireGameObj.transform.localScale = scale;
                }
                // set the start to the current end
                _wireStart = nodePosition;
            }
        }

        public void DeleteWire(WireScript wire)
        {
            _wires.Remove(wire);
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
        
        public void RegisterComponent(ElecComponent component)
        {
            if (_components.Contains(component))
                return;
            _components.Add(component);
        }

        public void UnRegisterComponent(ElecComponent component)
        {
            if (!_components.Contains(component))
                return;
            _components.Remove(component);
        }

        // Helper methods
        
        
        public static Pole PositionToPole(Vector2 position) => new((int)(position.y - 3.5f), (int)(position.x - 3.5f));
        
    }
}
