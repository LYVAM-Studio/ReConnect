using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class AggressiveMob : MonoBehaviour
    {
        public NavMeshAgent agent;
        private readonly List<Transform> _playersInRange = new List<Transform>();
    
        private void Update()
        {
            var closestPlayerTransform = GetClosestPlayerTransform();
            if (closestPlayerTransform is not null)
            {
                agent.SetDestination(closestPlayerTransform.position); 
            }
            else if (!agent.hasPath || agent.remainingDistance <= 3)
            {
                agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
            }
        }
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered");
                _playersInRange.Add(other.transform);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player exited");
                _playersInRange.Remove(other.transform);
            }
        }

        private Transform GetClosestPlayerTransform()
        {
            return _playersInRange
                // .Where(p => p.!IsKO)
                .OrderBy(p => Vector3.Distance(transform.position, p.position))
                .FirstOrDefault();
        }
    }
}