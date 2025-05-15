using UnityEngine;

namespace Reconnect.Menu.Lessons
{
    public class TestLessons : MonoBehaviour
    {
        public Sprite[] lessonsSprites;
        public LessonsInventoryManager lessonsInventoryManager;
        public void PopulateLessons()
        {
            foreach (Sprite lessonsSprite in lessonsSprites)
            {
                lessonsInventoryManager.AddItem(lessonsSprite.name, lessonsSprite);
            }
        }
    }
}