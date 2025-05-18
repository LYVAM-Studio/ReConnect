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
        [SerializeField] private float runningSpeed = 5f;
        [SerializeField] private float minPauseTime = 5f;
        [SerializeField] private float maxPauseTime = 15f;
        [SerializeField] private float minMovementRadius = 5f;
        [SerializeField] private float maxMovementRadius = 20f;
        
        private List<Transform> _playersInRange = new();
        
        private Animator _animator;
        private int _isDeadHash;
        private int _isRunningHash;
        private int _isAttackingHash;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            MinMovementRadius = minMovementRadius;
            MaxMovementRadius = maxMovementRadius;
            
            _animator = GetComponent<Animator>();
            _isDeadHash = Animator.StringToHash("IsDead");
            _isRunningHash = Animator.StringToHash("IsRunning");
            _isAttackingHash = Animator.StringToHash("IsAttacking");
        }

        private void Update()
        {
            if (!isServer) return;
            
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

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            
            if (other.CompareTag("Player"))
                _playersInRange.Add(other.transform);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;
            
            if (other.CompareTag("Player"))
                _playersInRange.Remove(other.transform);

            Agent.SetDestination(transform.position);
        }
        
        protected override void ChooseRandomDestination()
        {
            Agent.SetDestination(GetRandomDestination());
        }

        private Transform GetClosestPlayerTransform()
        {
            // Remove destroyed components
            _playersInRange = _playersInRange.Where(p => p).ToList();

            return _playersInRange
                // .Where(p => !p.IsKO)
                // .Where(p => !p.IsCrouching)
                .OrderBy(p => Vector3.Distance(transform.position, p.position))
                .FirstOrDefault();
        }
        
        public void AttackAnimation() => _animator.SetTrigger(_isAttackingHash);
        public void DeathAnimation() => _animator.SetTrigger(_isDeadHash);
    }
}