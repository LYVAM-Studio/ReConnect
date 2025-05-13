using UnityEngine;

namespace Reconnect.Menu.Lessons
{
    public class TestLessons : MonoBehaviour
    {
        public Sprite[] lessonsSprites;
        public LessonsInventoryManager lessonsInventoryManager;
        private void Start()
        {
            foreach (Sprite lessonsSprite in lessonsSprites)
            {
                lessonsInventoryManager.AddItem(lessonsSprite.name, lessonsSprite);
            }
        }
    }
}