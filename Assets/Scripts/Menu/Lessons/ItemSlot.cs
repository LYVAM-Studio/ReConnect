using System;
using Reconnect.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    // ===== ITEM DATA =====
    public string itemName;
    public Sprite itemSprite;
    public bool isFull;
    
    // ===== ITEM SLOT =====
    [SerializeField] private GameObject lessonNamePanel;
    [SerializeField] private TMP_Text lessonNameText;
    [SerializeField] private Image lessonImage;

    public void AddItem(string itemName, Sprite sprite)
    {
        this.itemName = itemName;
        itemSprite = sprite;
        isFull = true;

        lessonNameText.text = itemName;
        lessonNamePanel.SetActive(true);
        lessonImage.sprite = itemSprite;
    }
}
