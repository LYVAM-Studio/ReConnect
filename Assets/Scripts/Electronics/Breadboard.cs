using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Breadboard : MonoBehaviour
    {
        // The list of the components in the breadboard
        private List<ElecComponent> _components;
        private bool _onWireCreation;
        private Vector3 _wireStart;

        void Start()
        {
            _components = new List<ElecComponent>();
            _onWireCreation = false;
        }
        
        // #########################
        // #    WIRE MANAGEMENT    #
        // #########################

        public void StartWire(Vector3 nodePosition)
        {
            _onWireCreation = true;
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
                EndWire();
            }
            else if (delta != Vector2.zero)
            {
                // instantiate a wire from the wire prefab
                var wire = Instantiate(Helper.GetPrefabByName("Components/WirePrefab"));
                if (wire is null)
                    throw new Exception("The wire prefab could not be found.");
                // set position
                wire.transform.position = (_wireStart + nodePosition) / 2;
                // set rotation
                wire.transform.LookAt(nodePosition);
                wire.transform.eulerAngles += new Vector3(90, 0, 0);
                // set scale (length of the wire)
                var scale = wire.transform.localScale;
                scale[1] = (nodePosition - _wireStart).magnitude / 2f;
                wire.transform.localScale = scale;
                // set the start to the current end
                _wireStart = nodePosition;
            }
        }
        
        
        
        
        
        // Traverses the circuit calculating U and I.
        public void LaunchElectrons()
        {
            var circuit = InitCircuit();
            var explored = new List<(int h, int w)>();
            LaunchElectronsRec(circuit, explored, 0, 0);
        }

        public void LaunchElectronsRec(List<ElecComponent>[,] circuit, List<(int h, int w)> explored, int h,
            int w)
        {
            // Forward while there is a wire
            while (circuit[h, w].Count == 1 && circuit[h, w][0].type == ComponentType.Wire)
                (h, w) = circuit[h, w][0].GetNormalizedPos();
            foreach (var component in circuit[h, w])
            {
                
            }
        }

        // Returns the current circuit as a 2-dimensional array. Each element of this array represents the list of every component (given by ref, no copy) that has a pole there (i.e. is plugged in the corresponding breadboard hole).  
        private List<ElecComponent>[,] InitCircuit()
        {
            // Init the 2-dimensional array with empty lists
            var circuit = new List<ElecComponent>[8, 8];
            for (int h = 0; h < 8; h++)
            for (int w = 0; w < 8; w++)
                circuit[h, w] = new List<ElecComponent>();

            // Add the components in the empty 2-dimensional array
            foreach (var component in _components)
            {
                // The position of the currently processed component according to the breadboard
                var origin = component.GetNormalizedPos();
                foreach (var pole in component.poles)
                {
                    if (origin.h + pole.GetH() is < 0 or >= 8 || origin.w + pole.GetW() is < 0 or >= 8)
                        throw new IndexOutOfRangeException(
                            $"The pole (x:{pole.x}, y:{pole.y}) or also (h:{pole.GetH()}, w:{pole.GetW()}) goes outside the breadboard.");
                    circuit[origin.h + pole.GetH(), origin.w + pole.GetW()].Add(component);
                }
            }
            
            return circuit;
        }
        
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
    }
}
