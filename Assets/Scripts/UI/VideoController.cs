using UnityEngine;
using UnityEngine.Video;

namespace Reconnect.UI
{
    public class VideoController : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public GameObject videoScreen; // The RawImage object

        public void PlayVideo()
        {
            videoScreen.SetActive(true);
            videoPlayer.Play();
        }

        public void StopVideo()
        {
            videoPlayer.Stop();
            videoScreen.SetActive(false);
        }

        public void PlayPauseVideo()
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
            {
                PlayVideo();
            }

        }
    }
}