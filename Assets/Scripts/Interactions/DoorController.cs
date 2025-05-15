using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;
    public float slideUpDistance = 3f;
    public float speed = 2f;

    private bool isPlayerNearby = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        if (door != null)
        {
            closedPosition = door.position;
            openPosition = closedPosition + Vector3.up * slideUpDistance;
        }
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
        // Entree
        isPlayerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        // Sortie
        isPlayerNearby = false;
    }
}