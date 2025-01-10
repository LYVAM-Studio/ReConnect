using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class InteractionDetector : MonoBehaviour
    {
        private Dictionary<IInteractable, double> _interactablesInRange = new();
    
        // Update is called once per frame
        void Update()
        {
            // If right click and can interact
            if (Input.GetMouseButtonDown(3) && _interactablesInRange.Count > 0)
            {
                var interactable = GetNearestInteractable();
                interactable.Interact();
                // If interaction is no longer possible
                if (!interactable.CanInteract())
                    _interactablesInRange.Remove(interactable);
            }
        }
    
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("Interactable entered");
            var interactable = other.GetComponent<IInteractable>();
            var distance = Vector3.Distance(
                other.GetComponent<Transform>().position,
                this.GetComponent<Transform>().position
            );
            if (interactable is not null && interactable.CanInteract())
            {
                _interactablesInRange.Add(interactable, distance);
            }
        }
    
        public void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable is not null && _interactablesInRange.ContainsKey(interactable))
            {
                _interactablesInRange.Remove(interactable);
            }
        }
    
        // Gets the nearest interactable in the range of the player
        private IInteractable GetNearestInteractable()
        {
            if (_interactablesInRange.Count == 0)
                throw new ArgumentException("Cannot find the nearest interactable if no interactable is in range.");
    
            IInteractable interactable = null;
            var minDistance = double.MaxValue;
            foreach (var (i, d) in _interactablesInRange)
            {
                if (d < minDistance)
                {
                    minDistance = d;
                    interactable = i;
                }
            }
    
            return interactable;
        }
    }

}