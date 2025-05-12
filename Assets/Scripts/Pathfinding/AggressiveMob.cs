using System.Collections.Generic;
using System.Linq;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class AggressiveMob : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private List<Transform> _playersInRange = new List<Transform>();

        private void Start()
        {
            if (!TryGetComponent(out _agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
        }

        private void Update()
        {
            var closestPlayerTransform = GetClosestPlayerTransform();
            if (closestPlayerTransform is not null)
            {
                _agent.SetDestination(closestPlayerTransform.position); 
            }
            else if (!_agent.hasPath || _agent.remainingDistance <= 3)
            {
                _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
            }
        }
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playersInRange.Add(other.transform);
                _agent.SetDestination(GetClosestPlayerTransform().position);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playersInRange.Remove(other.transform);
                _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
            }
        }

        private Transform GetClosestPlayerTransform()
        {
            // Remove destroyed components
            _playersInRange = _playersInRange.Where(p => p).ToList();
            
            return _playersInRange
                // .Where(p => p.!IsKO)
                .OrderBy(p => Vector3.Distance(transform.position, p.position))
                .FirstOrDefault();
        }
    }
}