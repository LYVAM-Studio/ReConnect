using UnityEngine;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class PassiveMob : Mob
    {
        [SerializeField] private GameObject model;
        
        [Header("Movement settings")]
        [SerializeField] private float minPauseTime = 1f;
        [SerializeField] private float maxPauseTime = 5f;
        [SerializeField] private float minMovementRadius = 2f;
        [SerializeField] private float maxMovementRadius = 5f;

        /// <summary>
        /// The y position of the previous target destination
        /// </summary>
        private float _previousY;
        /// <summary>
        /// The y position of the current target destination
        /// </summary>
        private float _targetY;
        /// <summary>
        /// The distance between the last target and the current target. It takes into account the y component which is not directly managed by the navmesh agent. 
        /// </summary>
        private float _targetDistance;
        
        private void Start()
        {
            _previousY = model.transform.position.y;
            ChooseRandomDestination();
        }

        private void Update()
        {
            if (IsWaiting) return;
            
            if (HasArrived)
            {
                StartCoroutine(PauseForSeconds(Random.Range(minPauseTime, maxPauseTime)));
            }
            else
            {
                // move the model on y-axis
                model.transform.position = new Vector3(
                    model.transform.position.x,
                    _previousY + (_targetY - _previousY) *
                    (1 - Vector3.Distance(Agent.destination, model.transform.position) / _targetDistance),
                    model.transform.position.z);
                
                // make the model spin
                model.transform.localEulerAngles +=
                    20 * Random.Range(0.5f, 1.5f) * Time.deltaTime * Vector3.up;
            }
        }

        protected override void ChooseRandomDestination()
        {
            float radius = Random.Range(minMovementRadius, maxMovementRadius);
            float angle = Random.Range(0f, Mathf.PI * 2);

            Agent.SetDestination(transform.position +
                                 new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));

            _previousY = model.transform.position.y;
            _targetY = Mathf.Max(model.transform.position.y + Random.Range(-2f, 2f), transform.position.y, Agent.destination.y);
            
            _targetDistance = Vector3.Distance(Agent.destination, model.transform.position);
        }
    }   
}