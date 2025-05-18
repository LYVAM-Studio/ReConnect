using System;
using System.Collections;
using Mirror;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public abstract class Mob : NetworkBehaviour
    {
        protected float MinMovementRadius;
        protected float MaxMovementRadius;
        
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

        public override void OnStartServer()
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                throw new UnreachableCaseException("This should have been checked before in the MobManager.");
            
            Agent.Warp(hit.position);
            ChooseRandomDestination();
        }

        protected abstract void ChooseRandomDestination();

        protected Vector3 GetRandomDestination()
        {
            NavMeshHit hit;
            int count = 0;
            while (!NavMesh.SamplePosition(GetNewTarget(), out hit, 5f, NavMesh.AllAreas) && count < 30)
                count++;

            if (count >= 30)
                throw new Exception("No target pos on the navmesh has been found.");
            
            return hit.position;
        }
        
        protected IEnumerator PauseForSeconds(float seconds)
        {
            IsWaiting = true;
            yield return new WaitForSeconds(seconds);
            IsWaiting = false;
            ChooseRandomDestination();
        }
        
        /// <summary>
        /// Calculates a new random target position within a circular area around the current object.
        /// </summary>
        /// <returns>
        /// A <see cref="Vector3"/> representing a random point on the XZ plane within the specified radius range from the current position.
        /// </returns>
        private Vector3 GetNewTarget()
        {
            float radius = Random.Range(MinMovementRadius, MaxMovementRadius);
            float angle = Random.Range(0f, Mathf.PI * 2);
            return transform.position + 
                   new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }
    }
}