using System.Collections;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class PassiveMob : MonoBehaviour
    {
        [SerializeField] private float maxWaitingTime = 15f;
        [SerializeField] private float minWaitingTime = 5f;
        private NavMeshAgent _agent;
        private bool _isWaiting;
        void Start()
        {
            if (!TryGetComponent(out _agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
            SetDestination();
        }

        private IEnumerator PauseForSeconds(float seconds)
        {
            _isWaiting = true;
            yield return new WaitForSeconds(seconds);
            _isWaiting = false;
            SetDestination();
        }

        private void SetDestination()
        {
            _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
        }
        
        void Update()
        {
            if (_agent.remainingDistance <= 3 && !_isWaiting)
            {
                StartCoroutine(PauseForSeconds(Random.Range(minWaitingTime, maxWaitingTime)));
            }
        }
    }   
}