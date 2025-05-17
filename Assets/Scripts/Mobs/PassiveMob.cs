using UnityEngine;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class PassiveMob : Mob
    {
        [SerializeField] private GameObject model;
        
        private void Start()
        {
            ChooseRandomDestination();
        }
        
        private void Update()
        {
            if (Agent.remainingDistance <= 3 && !IsWaiting)
            {
                StartCoroutine(PauseForSeconds(Random.Range(minWaitingTime, maxWaitingTime)));
            }
        }
    }   
}