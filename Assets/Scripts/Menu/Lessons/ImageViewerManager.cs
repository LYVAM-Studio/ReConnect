using System;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.Menu.Lessons
{
    public class ImageViewerManager : MonoBehaviour
    {
        public static ImageViewerManager Instance;
        
        [SerializeField] private Image image;
        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("A MenuController has already been instantiated.");
            Instance = this;
            
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
