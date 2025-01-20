using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Display of the interaction range")]
        [SerializeField]
        [Tooltip("The player prefab object so the distance with interactable objects can be computed.")]
        private GameObject player;
        [SerializeField]
        [Tooltip("A sphere to display so the player can see its own interaction range.")]
        private MeshRenderer visualRange;
        [SerializeField]
        [Tooltip("Whether the visual range is shown by default (at the player instantiation).")]
        private bool isShownByDefault;
        
        // Whether the interaction range is shown or not
        private bool _showRange;
        // A list containing every interactable objects in the interaction range of the player, stored with their distance with respect to the player.
        private readonly List<(IInteractable interactable, Transform transform)> _interactableInRange = new();
        // // Whether the player has already started an interaction
        // private bool _isInteracting;

        public void Start()
        {
            _showRange = isShownByDefault;
            visualRange = GetComponent<MeshRenderer>();
            visualRange.enabled = _showRange;
            player = transform.parent.gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _interactableInRange.Count > 0) // TODO -> use new input system
            {
                GetNearestInteractable().Interact(player);
            }

            
            // Debug _interactableInRange
            if (Input.GetKeyDown(KeyCode.I)) // Debug key
            {
                var count = _interactableInRange.Count;
                Debug.Log($"Count: {count}\nFirst: {(count > 0 ? GetNearestInteractable().ToString() : "none")}");
            }
            // Toggle display of the interaction range
            if (Input.GetKeyDown(KeyCode.N)) // Debug key
            {
                _showRange = !_showRange;
                visualRange.enabled = _showRange;
            }
        }
    
        public void OnTriggerEnter(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable is not null)
            {
                Debug.Log("Interactable entered");
                _interactableInRange.Add((interactable, other.transform));
            }
        }
    
        public void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable is not null && _interactableInRange.Any(e => e.interactable.Equals(interactable)))
            {
                Debug.Log("Interactable exited");
                _interactableInRange.RemoveAll(e => e.interactable.Equals(interactable));
            }
        }
    
        // Gets the nearest interactable in the range of the player
        private IInteractable GetNearestInteractable()
        {
            if (_interactableInRange.Count == 0)
                throw new ArgumentException("Cannot find the nearest interactable if no interactable is in range.");
    
            IInteractable nearest = null;
            var minDistance = double.MaxValue;
            foreach (var (interactable, transformComponent) in _interactableInRange)
            {
                if (interactable.CanInteract())
                {
                    var distance = Dist(transformComponent);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = interactable;
                    }
                }
            }
    
            return nearest;
        }

        // Returns the distance between the given transform and the player transform
        private double Dist(Transform otherTransform) =>
            Vector3.Distance(otherTransform.position, player.transform.position);
    }

}