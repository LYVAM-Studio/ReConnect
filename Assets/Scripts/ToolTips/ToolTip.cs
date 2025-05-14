using System.Collections;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Reconnect.ToolTips
{
    public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        [TextArea(1, 3)]
        public string text;

        public float timeBeforeAppearing = 0.5f;

        private ToolTipWindow _window;
        private Coroutine _coroutine;

        private void Awake()
        {
            var prefab = Instantiate(Resources.Load<GameObject>("Prefabs/ToolTipPrefab"));
            if (!prefab.TryGetComponent(out _window))
                throw new ComponentNotFoundException("No ToolTipWindow component has been found on the ToolTipPrefab.");

            _window.text.text = text;
            
            SetVisible(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _coroutine = StartCoroutine(ShowCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetVisible(false);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            SetVisible(false);
        }
        
        private void SetVisible(bool visible)
        {
            _window.canvas.SetActive(visible);
            
            if (!visible && _coroutine is not null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
        
        private void UpdatePos()
        {
            _window.position.anchoredPosition = Mouse.current.position.ReadValue() + new Vector2(5, 5);
        }
        
        private IEnumerator ShowCoroutine()
        {
            yield return new WaitForSeconds(timeBeforeAppearing);
            UpdatePos();
            SetVisible(true);
        }
    }
}