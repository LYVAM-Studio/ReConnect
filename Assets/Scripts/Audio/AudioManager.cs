using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Reconnect.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Sources")]
        public AudioSource sfxSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] public float ambianceVolume = 1f;
        [Range(0f, 1f)] public float interfaceVolume = 1f;
        [Range(0f, 1f)] public float movementVolume = 1f;

        [Header("Ambiance")]
        public AudioClip doorOpen;
        public AudioClip doorClose;

        [Header("Interface")]
        public AudioClip buttonClick;
        public AudioClip error;
        public AudioClip breadboardopen;
        public AudioClip musicMenu;

        [Header("Movement")]
        public AudioClip[] footstepClips;
        public AudioClip[] runningClips;

        private int lastMusicIndex = -1;

        public void SetAmbianceVolume(float value)
        {
            ambianceVolume = Mathf.Clamp01(value);
        }

        public void SetInterfaceVolume(float value)
        {
            interfaceVolume = Mathf.Clamp01(value);
        }

        public void SetMovementVolume(float value)
        {
            movementVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayButtonClick()
        {
            sfxSource.PlayOneShot(buttonClick, interfaceVolume);
        }

        public void PlayError()
        {
            sfxSource.PlayOneShot(error, interfaceVolume);
        }

        public void PlayFootstep(bool isRunning)
        {
            AudioClip[] selectedClips;
            if (isRunning)
            {
                selectedClips = runningClips;
            }
            else
            {
                selectedClips = footstepClips;
            }

            if (selectedClips == null || selectedClips.Length == 0)
                return;

            int randomIndex = Random.Range(0, selectedClips.Length);
            AudioClip chosenClip = selectedClips[randomIndex];

            sfxSource.PlayOneShot(chosenClip, movementVolume);
        }

        public void PlayDoorOpen()
        {
            sfxSource.PlayOneShot(doorOpen, ambianceVolume);
        }

        public void PlayDoorClose()
        {
            sfxSource.PlayOneShot(doorClose, ambianceVolume);
        }
    }
}
