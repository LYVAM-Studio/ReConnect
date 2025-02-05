using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Reconnect.Interactions
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Display of the interaction range")]
        [SerializeField]
        [Tooltip("The player prefab object so the distance with interactable objects can be computed.")]
        private GameObject player;

        [SerializeField] [Tooltip("A sphere to display so the player can see its own interaction range.")]
        private MeshRenderer visualRange;

        [SerializeField] [Tooltip("Whether the visual range is shown by default (at the player instantiation).")]
        private bool isShownByDefault;

        // A list containing every interactable objects in the interaction range of the player, stored with their distance with respect to the player.
        private readonly List<(Interactable interactable, Transform transform)> _interactableInRange = new();

        // The most recently calculated nearest interactable in range (avoids recalculation).
        [CanBeNull] private Interactable _currentNearest;

        // Whether the interaction range is shown or not
        private bool _showRange;
        // // Whether the player has already started an interaction
        // private bool _isInteracting;

        public void Start()
        {
            _showRange = isShownByDefault;
            _currentNearest = null;
            visualRange = GetComponent<MeshRenderer>();
            visualRange.enabled = _showRange;
            player = transform.parent.gameObject;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _interactableInRange.Count > 0) // TODO -> use new input system
                GetNearestInteractable()!.Interact(player);

            // Make the nearest interactable glow more
            if (_currentNearest is not null) _currentNearest.ResetNearest();
            var newNearest = GetNearestInteractable();
            if (newNearest is not null) newNearest.SetNearest();


            // Debug key: Debug _interactableInRange
            if (Input.GetKeyDown(KeyCode.I))
            {
                var count = _interactableInRange.Count;
                Debug.Log($"Count: {count}\nFirst: {(count > 0 ? GetNearestInteractable()!.ToString() : "none")}");
            }

            // Debug key: Toggle display of the interaction range
            if (Input.GetKeyDown(KeyCode.N))
            {
                _showRange = !_showRange;
                visualRange.enabled = _showRange;
            }
        }

        // This method is called when a trigger enters the player interaction range.
        public void OnTriggerEnter(Collider other)
        {
            var interactable = other.GetComponent<Interactable>();
            if (interactable is not null)
            {
                // Debug.Log("Interactable entered");
                interactable.OnEnterPlayerRange();
                _interactableInRange.Add((interactable, other.transform));
            }
        }

        // This method is called when a trigger leaves the player interaction range.
        public void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<Interactable>();
            if (interactable is not null && _interactableInRange.Any(e => e.interactable.Equals(interactable)))
            {
                // Debug.Log("Interactable exited");
                interactable.OnExitPlayerRange();
                _interactableInRange.RemoveAll(e => e.interactable.Equals(interactable));
            }
        }

        // Gets the nearest interactable in the range of the player. If none is found, returns null.
        [CanBeNull]
        private Interactable GetNearestInteractable()
        {
            Interactable nearest = null;
            var minDistance = double.MaxValue;
            foreach (var (interactable, transformComponent) in _interactableInRange)
                if (interactable.CanInteract())
                {
                    var distance = Dist(transformComponent);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = interactable;
                    }
                }

            _currentNearest = nearest;
            return nearest;
        }

        // Returns the distance between the given transform and the player transform
        private double Dist(Transform otherTransform)
        {
            return Vector3.Distance(otherTransform.position, player.transform.position);
        }
    }
}