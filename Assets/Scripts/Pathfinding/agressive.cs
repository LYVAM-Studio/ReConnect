using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public class agressif : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public NavMeshAgent agent;
    public Random _random;
    private readonly List<Transform> playerInRange = new();
    void Start()
    {
        Vector3 position = agent.transform.position;
        float xa = position.x;
        float za = position.z;
        
        _random = new Random(42);
        var x = _random.Next(-30, 30);
        var z = _random.Next(-30, 30);
        agent.SetDestination(new Vector3(x, position.y, z));
    }
    void Update()
    {
        if (playerInRange.Count > 0)
        {
            var player = playerInRange[0];
            float px = player.position.x;
            float py = player.position.y;
            float pz = player.position.z;
            Vector3 newp = new Vector3(px, py ,pz);
            agent.SetDestination(newp); 
        }
        else
        {
            if (agent.remainingDistance == 0)
            {
                float px = agent.transform.position.x;
                float py = agent.transform.position.y;
                float pz = agent.transform.position.z;
                int x = (int)px + _random.Next(-30, 30);
                int z = (int)pz + _random.Next(-30, 30);
                Vector3 newp = new Vector3(x, py, z);
                agent.SetDestination(newp);
            }
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Player entered");
            playerInRange.Add(other.transform);
        }
    }

    // This method is called when a trigger leaves the player interaction range.
    public void OnTriggerExit(Collider other)
    {
        if (other is not null && other.CompareTag("Player"))
        {
            // Debug.Log("Interactable exited");
            playerInRange.Remove(other.transform);
        }
    }
    
}