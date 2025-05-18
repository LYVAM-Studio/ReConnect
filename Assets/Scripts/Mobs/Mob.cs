using System.Collections;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public abstract class Mob : MonoBehaviour
    {
        protected bool IsWaiting;
        
        protected NavMeshAgent Agent;
        
        protected bool HasArrived => !Agent.pathPending &&
                                     Agent.remainingDistance <= Agent.stoppingDistance &&
                                     (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f);

        protected void Awake()
        {
            if (!TryGetComponent(out Agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
        }

        protected abstract void ChooseRandomDestination();
        
        protected IEnumerator PauseForSeconds(float seconds)
        {
            IsWaiting = true;
            yield return new WaitForSeconds(seconds);
            IsWaiting = false;
            ChooseRandomDestination();
        }
    }
}