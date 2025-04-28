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
        
        private new void Start()
        {
            base.Start();
            cam.gameObject.SetActive(false);
            cam.Priority = 0;
            ui.SetActive(false);
        }

        public override void Interact(GameObject player)
        {
            if (_isActive)
            {
                // quit the interface
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Outline.enabled = true;
                player.GetComponent<PlayerMovementsNetwork>().isLocked = false;
                player.transform.GetChild(0).gameObject.SetActive(true);
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
                player.GetComponent<PlayerMovementsNetwork>().isLocked = true;
                player.transform.GetChild(0).gameObject.SetActive(false);
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
            Plane plane = new Plane(transform.rotation * Vector3.forward, transform.position);
            Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var dist))
                return ray.GetPoint(dist);

            throw new Exception("Failed to raycast on breadboard plane.");
        }
    }
    
    
}