using UnityEngine;
using System.Collections;

namespace Reconnect.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        private AudioSource musicSource;
        private AudioSource footstepSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] public float ambianceVolume = 1f;
        [Range(0f, 1f)] public float interfaceVolume = 0.5f;
        [Range(0f, 1f)] public float movementVolume = 1f;

        [Header("Ambiance")]
        public AudioClip doorOpen;
        public AudioClip doorClose;

        [Header("Interface")]
        public AudioClip buttonClick;
        public AudioClip error;
        public AudioClip breadboardopen;
        public AudioClip musicMenu;
        public AudioClip levelUP;

        [Header("Movement")]
        public AudioClip[] footstepClips;
        public AudioClip[] runningClips;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
        }
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.Play();
            StartCoroutine(DestroySourceWhenDone(source));
        }

        private IEnumerator DestroySourceWhenDone(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            Destroy(source);
        }

        public void PlayButtonClick()
        {
            PlayClip(buttonClick, interfaceVolume);
        }

        public void PlayError()
        {
            PlayClip(error, interfaceVolume);
        }

        public void PlayLevelUP()
        {
            PlayClip(levelUP, interfaceVolume);
        }

        public void PlayBreadboardOpen()
        {
            PlayClip(breadboardopen, interfaceVolume);
        }
        public void PlayFootstep(bool isRunning)
        {
            AudioClip[] selectedClips = isRunning ? runningClips : footstepClips;
            if (selectedClips == null || selectedClips.Length == 0) return;
            int randomIndex = Random.Range(0, selectedClips.Length);
            AudioClip clip = selectedClips[randomIndex];

            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = clip;
                footstepSource.volume = movementVolume;
                footstepSource.pitch = isRunning ? 1.5f : 1.0f;
                footstepSource.Play();
            }
        }

        public void PlayDoorOpen()
        {
            PlayClip(doorOpen, ambianceVolume);
        }

        public void PlayDoorClose()
        {
            PlayClip(doorClose, ambianceVolume);
        }

        public void PlayMusicMenu()
        {
            if (musicMenu == null) return;
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            musicSource.clip = musicMenu;
            musicSource.volume = interfaceVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

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

    }
}