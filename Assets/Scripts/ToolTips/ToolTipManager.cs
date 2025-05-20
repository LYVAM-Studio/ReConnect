using System;
using System.Collections.Generic;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.ToolTips
{
    public class ToolTipManager : MonoBehaviour
    {
        public static ToolTipManager Instance;
        
        public bool ForceHide { get; set; }

        private readonly Dictionary<int, ToolTipWindow> _toolTips = new();

        [SerializeField]
        private Canvas canvas;

        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("ToolTipManager singleton has already been initialised");
            Instance = this;
        }
        
        public void CreateToolTip(int id)
        {
            if (_toolTips.TryGetValue(id, out var previous))
            {
                Debug.LogWarning($"A tooltip with id {id} has already been created. It has been destroy because of an id collision.");
                Destroy(previous);
            }
            
            var toolTipGameObj = InstantiateToolTip();
            if (!toolTipGameObj.TryGetComponent(out ToolTipWindow window))
                throw new ComponentNotFoundException(
                    "No ToolTipWindow component has been found on the tooltip prefab.");
            
            _toolTips[id] = window;
            window.gameObject.SetActive(false);
        }

        public void DestroyToolTip(int id)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            Destroy(tooltip.gameObject);
            _toolTips.Remove(id);
        }
        
        public void ShowToolTip(int id)
        {
            if (ForceHide)
                return;
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            tooltip.gameObject.SetActive(true);
        }
        
        public void HideToolTip(int id)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            tooltip.gameObject.SetActive(false);
        }

        public void SetText(int id, string text)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            tooltip.Text = text;
        }
        
        public void SetPosition(int id, Vector2 position)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            tooltip.transform.position = position;
        }
        
        public void SetPositionToMouse(int id)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            
            tooltip.transform.position = Mouse.current.position.ReadValue() + tooltip.Size / 2;
        }
        
        public void SetSize(int id, float width, float height)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");

            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height arguments must be non zero positive numbers.");

            tooltip.Size = new Vector2(width, height);
        }

        private GameObject InstantiateToolTip()
        {
            return Instantiate(Resources.Load<GameObject>("Prefabs/ToolTipPrefab"), canvas.transform);
        }
    }
}