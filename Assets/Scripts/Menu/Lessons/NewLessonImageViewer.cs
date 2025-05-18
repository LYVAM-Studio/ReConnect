using Reconnect.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.Menu.Lessons
{
    public class NewLessonImageViewer : MonoBehaviour
    {
        [SerializeField] private Image image;
        private void Awake()
        {
            if (image is null)
                throw new MissingSerializedFieldException(nameof(image));
        }

        public void LoadImage(Sprite sprite)
        {
            image.sprite = sprite;
        }
    
        public void CloseImage()
        {
            image.sprite = null;
        }
    }
}
