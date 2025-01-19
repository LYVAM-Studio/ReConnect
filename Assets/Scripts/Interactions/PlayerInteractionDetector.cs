using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Display of the interaction range")]
        public GameObject playerPrefab;
        public GameObject visualRange;
        public bool isShownByDefault;
        private bool _showRange;
        // A dictionary containing every interactable objects in the interaction range of the player, stored with their distance with respect to the player
        private readonly Dictionary<IInteractable, double> _interactableInRange = new();

        public void Start()
        {
            _showRange = isShownByDefault;
            visualRange.SetActive(_showRange);
            Debug.Log("Started");
        }

        // Update is called once per frame
        void Update()
        {
            // Toggle display of the interaction range
            if (Input.GetKeyDown(KeyCode.N))
            {
                _showRange = !_showRange;
                visualRange.SetActive(_showRange);
            }
            
            // If right click and can interact
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_interactableInRange.Count > 0)
                {
                    Debug.Log("Interaction");
                    var interactable = GetNearestInteractable();
                    interactable.Interact();
                    // If interaction is no longer possible
                    if (!interactable.CanInteract())
                        _interactableInRange.Remove(interactable);
                }
                else
                {
                    Debug.Log("No interactable objects in range");
                }
            }
            
            // Debug _interactableInRange
            if (Input.GetKeyDown(KeyCode.I))
            {
                var count = _interactableInRange.Count;
                Debug.Log($"Count: {count}\nFirst: {(count > 0 ? GetNearestInteractable().ToString() : "none")}");
            }
        }
    
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("Interactable entered");
            var interactable = other.GetComponent<IInteractable>();
            if (interactable is not null && interactable.CanInteract())
            {
                var distance = Vector3.Distance(
                    other.GetComponent<Transform>().position,
                    playerPrefab.GetComponent<Transform>().position
                );
                _interactableInRange.Add(interactable, distance);
            }
        }
    
        public void OnTriggerExit(Collider other)
        {
            Debug.Log("Interactable exited");
            var interactable = other.GetComponent<IInteractable>();
            if (interactable is not null && _interactableInRange.ContainsKey(interactable))
            {
                _interactableInRange.Remove(interactable);
            }
        }
    
        // Gets the nearest interactable in the range of the player
        private IInteractable GetNearestInteractable()
        {
            if (_interactableInRange.Count == 0)
                throw new ArgumentException("Cannot find the nearest interactable if no interactable is in range.");
    
            IInteractable interactable = null;
            var minDistance = double.MaxValue;
            foreach (var (i, d) in _interactableInRange)
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