using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reconnect.MouseHover
{
    public class MouseHoverManager : MonoBehaviour
    {
        // [NonSerialized] public static MouseHoverManager Instance;
        
        private Camera _mainCam;
        [CanBeNull] private IMouseInteractable _lastHovered;
        
        private void Start()
        {
            // if (Instance != null)
            //     throw new Exception("A MouseEventManager has already been created.");
            // Instance = this;
            _mainCam = Camera.main;
        }

        private void Update()
        {
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            
            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit/*, 10f*/))
            {
                IMouseInteractable newHovered = hit.collider.gameObject.GetComponent<IMouseInteractable>();
                if (newHovered == _lastHovered)
                {
                    newHovered?.OnHover();
                }
                else
                {
                    _lastHovered?.OnHoverExit();
                    newHovered?.OnHoverEnter();
                    _lastHovered = newHovered;
                }
            }
            else
            {
                _lastHovered?.OnHoverExit();
                _lastHovered = null;
            }
        }
    }
}