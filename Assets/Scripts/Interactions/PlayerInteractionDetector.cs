using System.Collections.Generic;
using System.Linq;
using Mirror;
using Reconnect.Menu;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Interactions
{
    public class InteractionDetector : NetworkBehaviour
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
        private Interactable _currentNearest;

        // Whether the interaction range is shown or not
        private bool _showRange;
        // // Whether the player has already started an interaction
        // private bool _isInteracting;
        
        private PlayerControls _controls;

        private PlayerGetter _playerGetter;

        private void Awake()
        {
            _controls = new PlayerControls();
            _controls.Player.Interact.performed += OnInteraction;
            
            if (!player.TryGetComponent(out _playerGetter))
                throw new ComponentNotFoundException(
                    "No PlayerGetter has been found on the player game object.");
        }
        
        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }
        
        private void OnDestroy()
        {
            _controls.Player.Interact.performed -= OnInteraction;
        }
        
        public void Start()
        {
            enabled = isLocalPlayer;
            _showRange = isShownByDefault;
            _currentNearest = null;
            visualRange.enabled = _showRange;
        }

        private void OnInteraction(InputAction.CallbackContext context)
        {
            if (MenuManager.Instance.CurrentMenuState is not MenuState.Pause
                && !_playerGetter.Movements.IsKo
                && _interactableInRange.Count > 0)
                GetNearestInteractable()?.Interact(player);
        }

        // Update is called once per frame
        private void Update()
        {
            if (!isLocalPlayer)
                return;
            
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
            if (!isLocalPlayer)
                return;
            if (other.TryGetComponent(out Interactable interactable))
            {
                // Debug.Log("Interactable entered");
                if (interactable.CanInteract())
                    interactable.OnEnterPlayerRange();
                
                _interactableInRange.Add((interactable, other.transform));
            }
        }

        // This method is called when a trigger leaves the player interaction range.
        public void OnTriggerExit(Collider other)
        {
            if (!isLocalPlayer)
                return;
            if (other.TryGetComponent(out Interactable interactable) &&
                _interactableInRange.Any(e => e.interactable.Equals(interactable)))
            {
                // Debug.Log("Interactable exited");
                interactable.OnExitPlayerRange();
                _interactableInRange.RemoveAll(e => e.interactable.Equals(interactable));
            }
        }

        // Gets the nearest interactable in the range of the player. If none is found, returns null.
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