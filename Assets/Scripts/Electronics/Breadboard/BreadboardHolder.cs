using System;
using Reconnect.Interactions;
using Reconnect.Player;
using Unity.Cinemachine;
using UnityEngine;

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
        private bool _isActive = false;
        private Camera mainCam;
        
        private Plane? _currentDragPlane;

        private void Awake()
        {
            mainCam = Camera.main;
        }

        public void SetDragPlane(Plane plane)
        {
            _currentDragPlane = plane;
        }

        public void ClearDragPlane()
        {
            _currentDragPlane = null;
        }
        
        public override void Interact(GameObject player)
        {
            if (_isActive)
            {
                // quit the interface
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Outline.enabled = true;
                PlayerGetter p = player.GetComponent<PlayerGetter>();
                p.MovementsNetwork.isLocked = false;
                p.DummyModel.SetActive(true);
                cam.gameObject.SetActive(false);
                cam.Priority = 0;
                ui.SetActive(false);
                _isActive = false;
            }
            else
            {
                // enter the interface
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                Outline.enabled = false;
                PlayerGetter p = player.GetComponent<PlayerGetter>();
                p.MovementsNetwork.isLocked = true;
                p.DummyModel.SetActive(false);
                cam.gameObject.SetActive(true);
                cam.Priority = 2;
                ui.SetActive(true);
                _isActive = true;
            }
        }

        public override bool CanInteract()
        {
            return true;
        }
        
        public Vector3 GetFlattenedCursorPos()
        {
            if (_currentDragPlane == null)
                throw new Exception("Drag plane not set. Did you forget to call SetDragPlane()?");

            Ray ray = mainCam!.ScreenPointToRay(Input.mousePosition);

            if (_currentDragPlane.Value.Raycast(ray, out var dist))
                return ray.GetPoint(dist);

            throw new Exception("Failed to raycast on breadboard plane.");
        }
        
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = mainCam!.ScreenPointToRay(Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log("Hit: " + hit.collider.gameObject.name);
                }
            }
        }

    }
}