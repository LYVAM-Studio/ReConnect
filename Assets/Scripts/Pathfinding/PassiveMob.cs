using System.Collections.Generic;
using System.Linq;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class PassiveMob : MonoBehaviour
    {
        private NavMeshAgent _agent; 
        void Start()
        {
            if (!TryGetComponent(out _agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
        }
        void Update()
        {
            if (!_agent.hasPath || _agent.remainingDistance <= 3)
            {
                _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
            }  
        }
    }   
}