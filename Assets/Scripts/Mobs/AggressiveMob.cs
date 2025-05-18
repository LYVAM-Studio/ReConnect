using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class AggressiveMob : Mob
    {
        [Header("Movement settings")]
        [SerializeField] private float walkingSpeed = 2f;
        [SerializeField] private float runningSpeed = 6f;
        [SerializeField] private float minPauseTime = 5f;
        [SerializeField] private float maxPauseTime = 15f;
        [SerializeField] private float minMovementRadius = 5f;
        [SerializeField] private float maxMovementRadius = 20f;
        
        private List<Transform> _playersInRange = new();
        
        private Animator _animator;
        private int _isDeadHash;
        private int _isRunningHash;
        private int _isAttackingHash;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _isDeadHash = Animator.StringToHash("IsDead");
            _isRunningHash = Animator.StringToHash("IsRunning");
            _isAttackingHash = Animator.StringToHash("IsAttacking");
            
            ChooseRandomDestination();
        }

        private void Update()
        {
            var closestPlayerTransform = GetClosestPlayerTransform();
            if (closestPlayerTransform is not null)
            {
                StopCoroutine(nameof(PauseForSeconds));
                IsWaiting = false;
                
                Agent.speed = runningSpeed;
                Agent.SetDestination(closestPlayerTransform.position);
            }
            else
            {
                Agent.speed = walkingSpeed;
                if (HasArrived && !IsWaiting)
                {
                    StartCoroutine(PauseForSeconds(Random.Range(minPauseTime, maxPauseTime)));
                }
            }

            _animator.SetBool(_isRunningHash, Agent.velocity.magnitude >= 1f);
        }

        protected override void ChooseRandomDestination()
        {
            float radius = Random.Range(minMovementRadius, maxMovementRadius);
            float angle = Random.Range(0f, Mathf.PI * 2);

            Agent.SetDestination(transform.position + new Vector3(
                Mathf.Cos(angle) * radius, 
                0, 
                Mathf.Sin(angle) * radius));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _playersInRange.Add(other.transform);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playersInRange.Remove(other.transform);

            Agent.SetDestination(transform.position);
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
        
        public void AttackAnimation() => _animator.SetTrigger(_isAttackingHash);
        public void DeathAnimation() => _animator.SetTrigger(_isDeadHash);
    }
}