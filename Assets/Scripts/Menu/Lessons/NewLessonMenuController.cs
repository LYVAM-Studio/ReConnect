using Reconnect.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.Menu.Lessons
{
    public class NewLessonMenuController : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text tmpText;
        private void Awake()
        {
            if (image is null)
                throw new MissingSerializedFieldException(nameof(image));
            if (tmpText is null)
                throw new MissingSerializedFieldException(nameof(tmpText));
        }

        public void LoadImage(Sprite sprite)
        {
            image.sprite = sprite;
        }
    
        public void CloseImage()
        {
            image.sprite = null;
        }
        
        public void SetTextToLevel(uint level)
        {
            tmpText.text = $"You are now level {level} ! Let's continue learning how electricity works together !";
        }
    }
}
