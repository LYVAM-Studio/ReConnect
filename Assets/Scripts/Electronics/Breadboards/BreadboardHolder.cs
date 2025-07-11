using System;
using Mirror;
using Reconnect.Game;
using Reconnect.Interactions;
using Reconnect.Menu;
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
        public static Vector3 SwitchWireInputCorner = new Vector3(4, 4, -0.5f);
        public static Vector3 SwitchWireOutputCorner = new Vector3(4, -4, -0.5f);
        
        public Breadboard breadboard;
        public BbSwitch breadboardSwitch;
        public CinemachineCamera cam;
        public GameObject ui;
        public TextAsset circuitYaml;
        
        [NonSerialized] public bool IsActive = false;

        private Camera _mainCam;
        private Plane _raycastPlane;

        // Cache to avoid multiple raycasts in the same frame
        private Vector3 _lastRaycast;
        private int _lastFrame;

        private new void Awake()
        {
            base.Awake();
            _mainCam = Camera.main;
            _raycastPlane = new Plane(
                transform.forward,
                breadboard.LocalToWorld(new Vector3(0, 0, -0.5f)));
            breadboard.circuitToLoad = circuitYaml;
        }

        public override void Interact(GameObject player)
        {
            if (IsActive)
            {
                // quit the interface
                MenuManager.Instance.BackToPreviousMenu();
                MenuManager.Instance.BreadBoardHolder = null;
            }
            else
            {
                // enter the interface
                MenuManager.Instance.BreadBoardHolder = this;
                MenuManager.Instance.SetMenuTo(MenuState.BreadBoard, CursorState.Shown);
            }
        }

        public void Activate(bool active)
        {
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerGetter p))
                throw new ComponentNotFoundException("No PlayerGetter component found on the player.");
            
            if (active)
            {
                // enter the interface
                Outline.enabled = false;
                p.Movements.isLocked = true;
                p.DummyModel.SetActive(false);
                FreeLookCamera.InputAxisController.enabled = false;
                cam.Priority = 2;
                ui.SetActive(true);
                IsActive = true;
            }
            else
            {
                // quit the interface
                p.Network.CmdOnBreadboardExit(netIdentity);
                Outline.enabled = true;
                p.Movements.isLocked = false;
                p.DummyModel.SetActive(true);
                FreeLookCamera.InputAxisController.enabled = true;
                cam.Priority = 0;
                ui.SetActive(false);
                IsActive = false;
                MenuManager.Instance.BreadBoardHolder = null;
            }
        }

        public override bool CanInteract() => GameManager.Level == level;
        
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
        
        // private void OnDrawGizmos()
        // {
        //     // Recompute the plane in editor mode for visualization
        //     Plane debugPlane = new Plane(
        //         transform.forward,
        //         transform.position - transform.rotation * (0.5f * transform.lossyScale.x * transform.forward));
        //
        //     // Get a point on the plane and draw the normal
        //     Vector3 center = debugPlane.ClosestPointOnPlane(transform.position);
        //     Vector3 normal = debugPlane.normal;
        //
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawRay(center, normal);
        //
        //     // Draw a quad to represent the plane (approximate)
        //     Vector3 right = Vector3.Cross(normal, Vector3.up);
        //     if (right == Vector3.zero) right = Vector3.Cross(normal, Vector3.forward);
        //     Vector3 up = Vector3.Cross(normal, right);
        //
        //     float size = 1f;
        //     Vector3 p1 = center + (right + up) * size;
        //     Vector3 p2 = center + (right - up) * size;
        //     Vector3 p3 = center + (-right - up) * size;
        //     Vector3 p4 = center + (-right + up) * size;
        //
        //     Gizmos.DrawLine(p1, p2);
        //     Gizmos.DrawLine(p2, p3);
        //     Gizmos.DrawLine(p3, p4);
        //     Gizmos.DrawLine(p4, p1);
        // }
    }
}