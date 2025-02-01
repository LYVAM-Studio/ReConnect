using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class Breadboard : MonoBehaviour
    {
        // The list of the components in the breadboard
        private List<ELecComponent> _components;

        void Start()
        {
            _components = new List<ELecComponent>();
        }
        
        // The following methods are research about how to traverse the circuit graph and to calculate 'U' and 'I' etc...
        // It is not used yet.

        
        // Traverses the circuit calculating U and I.
        public void LaunchElectrons()
        {
            var circuit = InitCircuit();
            var explored = new List<(int h, int w)>();
            LaunchElectronsRec(circuit, explored, 0, 0);
        }

        public void LaunchElectronsRec(List<ELecComponent>[,] circuit, List<(int h, int w)> explored, int h,
            int w)
        {
            // Forward while there is a wire
            while (circuit[h, w].Count == 1 && circuit[h, w][0].type == ComponentType.Wire)
                (h, w) = circuit[h, w][0].GetNormalizedPos();
        }

        // Returns the current circuit as a 2-dimensional array. Each element of this array represents the list of every component (given by ref, no copy) that has a pole there (i.e. is plugged in the corresponding breadboard hole).  
        private List<ELecComponent>[,] InitCircuit()
        {
            // Init the 2-dimensional array with empty lists
            var circuit = new List<ELecComponent>[8, 8];
            for (int h = 0; h < 8; h++)
            for (int w = 0; w < 8; w++)
                circuit[h, w] = new List<ELecComponent>();

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


        public void RegisterComponent(ELecComponent component)
        {
            if (_components.Contains(component))
                return;
            _components.Add(component);
        }

        public void UnRegisterComponent(ELecComponent component)
        {
            if (!_components.Contains(component))
                return;
            _components.Remove(component);
        }
    }
}
