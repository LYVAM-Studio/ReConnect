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

        private readonly Dictionary<int, ToolTipWindow> _toolTips = new();

        [SerializeField]
        private Canvas canvas;

        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("ToolTipManager singleton has already been initialised");
            Instance = this;
        }

        public void CreateToolTip(int id, string content, Size size)
        {
            if (_toolTips.TryGetValue(id, out var previous))
            {
                Debug.LogWarning($"A tooltip with id {id} has already been created. It has been destroy because of an id collision.");
                Destroy(previous);
            }
            
            var toolTipGameObj = InstantiateToolTip(size);
            if (!toolTipGameObj.TryGetComponent(out ToolTipWindow window))
                throw new ComponentNotFoundException(
                    "No ToolTipWindow component has been found on the tooltip prefab.");
            _toolTips[id] = window;
            window.text.text = content;
            window.gameObject.SetActive(false);
        }

        public void DestroyToolTip(int id)
        {
            if (!_toolTips.TryGetValue(id, out var tooltip))
                throw new ArgumentException($"No tooltip with id {id} has been found.");
            Destroy(tooltip.gameObject);
            _toolTips[id] = null;
        }
        
        public void ShowToolTip(int id)
        {
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
            tooltip.text.text = text;
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
            
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //     canvas.transform as RectTransform,
            //     Mouse.current.position.ReadValue(),
            //     canvas.worldCamera,
            //     out Vector2 mousePos);

            Vector2 mousePos = Mouse.current.position.ReadValue();

            mousePos += tooltip.Size switch
            {
                Size.Small => new Vector2(80, 15),
                Size.Medium => new Vector2(80, 15),
                Size.Large => new Vector2(80, 15),
                _ => throw new ArgumentException($"Size {tooltip.Size} does not exist.")
            };
            
            mousePos += new Vector2(1, 1);

            tooltip.transform.position = mousePos;
        }

        private GameObject InstantiateToolTip(Size size)
        {
            string sizeName = size switch
            {
                Size.Small => "Small",
                Size.Medium => "Medium",
                Size.Large => "Large",
                _ => throw new ArgumentException($"Size {size} does not exist.")
            };
            return Instantiate(Resources.Load<GameObject>($"Prefabs/ToolTips/{sizeName}ToolTip"), canvas.transform);
        }
    }
}