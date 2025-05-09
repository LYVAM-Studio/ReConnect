using System;
using Reconnect.Interactions;
using Reconnect.Player;
using Reconnect.Utils;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Electronics.Breadboards
{
    /// <summary>
    /// To ease the access to the breadboard, the UI and the camera. 
    /// </summary>
    public class BreadboardHolder : Interactable
    {
        public Breadboard breadboard;
        public CinemachineCamera cam;
        public GameObject ui;

        [NonSerialized] public bool IsActive = false;

        private Camera _mainCam;
        private Plane _raycastPlane;

        // Cache to avoid multiple raycasts in the same frame
        private Vector3 _lastRaycast;
        private int _lastFrame;

        private void Awake()
        {
            _mainCam = Camera.main;
            _raycastPlane = new Plane(
                transform.forward,
                transform.position - transform.rotation * (0.5f * transform.lossyScale.x * transform.forward));
        }

        public override void Interact(GameObject player)
        {
            if (!player.TryGetComponent(out PlayerGetter p))
                throw new ComponentNotFoundException("No PlayerGetter component found on the player.");
            
            if (IsActive)
            {
                // quit the interface
                breadboard.Dipoles.ForEach(d => d.OnBreadBoardExit());
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Outline.enabled = true;
                p.MovementsNetwork.isLocked = false;
                p.DummyModel.SetActive(true);
                FreeLookCamera.InputAxisController.enabled = true;
                cam.gameObject.SetActive(false);
                cam.Priority = 0;
                ui.SetActive(false);
                IsActive = false;
            }
            else
            {
                // enter the interface
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                Outline.enabled = false;
                p.MovementsNetwork.isLocked = true;
                p.DummyModel.SetActive(false);
                FreeLookCamera.InputAxisController.enabled = false;
                cam.gameObject.SetActive(true);
                cam.Priority = 2;
                ui.SetActive(true);
                IsActive = true;
            }
        }

        public override bool CanInteract()
        {
            return true;
        }
        
        public Vector3 GetFlattenedCursorPos()
        {
            if (_lastFrame == Time.frameCount)
                return _lastRaycast;

            var ray = _mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!_raycastPlane.Raycast(ray, out var dist))
                throw new UnreachableCaseException("Failed to raycast on breadboard plane.");

            _lastRaycast = ray.GetPoint(dist);
            _lastFrame = Time.frameCount;
            return _lastRaycast;
        }
    }
}