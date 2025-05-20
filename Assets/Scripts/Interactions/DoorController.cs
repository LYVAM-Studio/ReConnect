using UnityEngine;
using Reconnect.Audio;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;
    public float slideUpDistance = 3f;
    public float speed = 2f;

    private bool isPlayerNearby = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    private AudioSource audioSource;
    private bool canTrigger = false; // Prevents early trigger on scene load

    void Start()
    {
    
        if (door != null)
        {
            closedPosition = door.position;
            openPosition = closedPosition + Vector3.up * slideUpDistance;
        }

        audioSource = door.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = door.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Start coroutine to delay trigger activation
        StartCoroutine(EnableTriggerAfterDelay(0.5f)); // 0.5s delay can be adjusted
    }

    void Update()
    {
        if (door == null) return;
        Vector3 targetPosition = isPlayerNearby ? openPosition : closedPosition;
        if (Vector3.Distance(door.position, targetPosition) > 0.01f)
        {
            door.position = Vector3.Lerp(door.position, targetPosition, Time.deltaTime * speed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canTrigger) return;

        isPlayerNearby = true;
        if (AudioManager.Instance != null && AudioManager.Instance.doorOpen != null)
        {
            audioSource.clip = AudioManager.Instance.doorOpen;
            audioSource.volume = AudioManager.Instance.ambianceVolume;
            audioSource.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!canTrigger) return;

        isPlayerNearby = false;
        if (AudioManager.Instance != null && AudioManager.Instance.doorClose != null)
        {
            audioSource.clip = AudioManager.Instance.doorClose;
            audioSource.volume = AudioManager.Instance.ambianceVolume;
            audioSource.Play();
        }
    }

    private IEnumerator EnableTriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canTrigger = true;
    }
}
