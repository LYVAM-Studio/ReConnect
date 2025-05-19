using System.Linq;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Menu.Lessons
{
    
    public class LessonsInventoryManager : MonoBehaviour
    {
        public ItemSlot[] itemSlots;

        public void AddItem(string itemName, Sprite sprite)
        {
            ItemSlot itemSlot = itemSlots.FirstOrDefault(slot => !slot.isFull);
            if (itemSlot is null)
                throw new InventoryFullException("Not enough space in the lesson inventory to add a new lesson");
            itemSlot.AddItem(itemName, sprite);
        }
    }
}
