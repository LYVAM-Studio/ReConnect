using System.Collections;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public abstract class Mob : MonoBehaviour
    {
        [SerializeField] protected float maxWaitingTime = 15f;
        [SerializeField] protected float minWaitingTime = 5f;
        
        protected bool IsWaiting;
        
        protected NavMeshAgent Agent;

        protected void Awake()
        {
            if (!TryGetComponent(out Agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
        }

        protected void ChooseRandomDestination()
        {
            float radius = Random.Range(5, 20);
            float angle = Random.Range(0f, Mathf.PI * 2);

            Agent.SetDestination(transform.position +
                                  new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }
        
        protected IEnumerator PauseForSeconds(float seconds)
        {
            IsWaiting = true;
            yield return new WaitForSeconds(seconds);
            IsWaiting = false;
            ChooseRandomDestination();
        }
    }
}