using System.Collections;
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
        [SerializeField] private float maxWaitingTime = 15f;
        [SerializeField] private float minWaitingTime = 5f;
        private NavMeshAgent _agent;
        private List<Transform> _playersInRange = new List<Transform>();
        private bool _isWaiting;

        private Animator _animator;

        private int _isDeadHash;
        private int _isRunningHash;
        private int _isAttackingHash;
        
        private void Start()
        {
            if (!TryGetComponent(out _agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
            SetDestination();

            _animator = GetComponent<Animator>();
            _isDeadHash = Animator.StringToHash("IsDead");
            _isRunningHash = Animator.StringToHash("IsRunning");
            _isAttackingHash = Animator.StringToHash("IsAttacking");
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

        private void Update()
        {
            var closestPlayerTransform = GetClosestPlayerTransform();
            if (closestPlayerTransform is not null)
            {
                _agent.SetDestination(closestPlayerTransform.position);
                _agent.speed = 4f;
            }
            else if (_agent.remainingDistance <= 3 && !_isWaiting)
            {
                StartCoroutine(PauseForSeconds(Random.Range(minWaitingTime, maxWaitingTime)));
            }

            _animator.SetBool(_isRunningHash, _agent.velocity.magnitude >= 0.1f);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (_isWaiting)
                {
                    StopCoroutine(nameof(PauseForSeconds));
                    _isWaiting = false;
                }

                _playersInRange.Add(other.transform);
                _agent.SetDestination(GetClosestPlayerTransform().position);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playersInRange.Remove(other.transform);
                // reset the destination that will be set by update
                _agent.SetDestination(transform.position);
                // reset to low speed when not attacking player 
                _agent.speed = 2f;
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