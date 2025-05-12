using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;
public class passif : MonoBehaviour
{
    public NavMeshAgent agent;

    public Random _random;
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
        if (agent.remainingDistance == 0)
        {
            float px = agent.transform.position.x;
            float py = agent.transform.position.y;
            float pz = agent.transform.position.z;
            int x = (int)px+_random.Next(-30, 30);
            int z = (int)pz+_random.Next(-30, 30);
            Vector3 newp = new Vector3(x, py ,z);
            Debug.Log(newp);
            agent.SetDestination(newp);  
        }   
    }
}