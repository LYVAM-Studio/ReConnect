using Reconnect.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reconnect.Menu.Lessons
{
    public class ItemSlot : MonoBehaviour, IPointerClickHandler
    {
        // ===== ITEM DATA =====
        public Sprite itemSprite;
        public bool isFull;
    
        // ===== ITEM SLOT =====
        [SerializeField] private GameObject lessonNamePanel;
        [SerializeField] private TMP_Text lessonNameText;
        [SerializeField] private Image lessonImage;


        public void AddItem(string itemName, Sprite sprite)
        {
            itemSprite = sprite;
            isFull = true;

            lessonNameText.text = itemName;
            lessonNamePanel.SetActive(true);
            lessonImage.sprite = itemSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isFull)
                    MenuManager.Instance.OpenImageInViewer(itemSprite);
            }
        }
    }
}
