using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class AggressiveMob : Mob
    {
        private List<Transform> _playersInRange = new List<Transform>();
        
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
                Agent.speed = 4f;
                Agent.SetDestination(closestPlayerTransform.position);
            }
            else
            {
                Agent.speed = 2f;
                if (Agent.remainingDistance <= 3 && !IsWaiting)
                {
                    StartCoroutine(PauseForSeconds(Random.Range(minWaitingTime, maxWaitingTime)));
                }
            }

            _animator.SetBool(_isRunningHash, Agent.velocity.magnitude >= 0.1f);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (IsWaiting)
                {
                    StopCoroutine(nameof(PauseForSeconds));
                    IsWaiting = false;
                }

                _playersInRange.Add(other.transform);
                Agent.SetDestination(GetClosestPlayerTransform().position);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playersInRange.Remove(other.transform);
                // reset the destination that will be set by update
                Agent.SetDestination(transform.position);
                // reset to low speed when not attacking player 
                Agent.speed = 2f;
            }
        }

        public void AttackAnimation() => _animator.SetTrigger(_isAttackingHash);
        public void DeathAnimation() => _animator.SetTrigger(_isDeadHash);

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