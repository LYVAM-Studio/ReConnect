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
        
        private Animator animator;
        private bool isRunning = false;
        private bool isAttacking = false;
        private bool isDying = false;
        private void Start()
        {
            if (!TryGetComponent(out _agent))
                throw new ComponentNotFoundException("No NavMeshAgent component has been found on this mob.");
            SetDestination();
            
            animator = GetComponent<Animator>();

        }
        
        private IEnumerator PauseForSeconds(float seconds)
        {
            _isWaiting = true;
            Exit();
            yield return new WaitForSeconds(seconds);
            _isWaiting = false;
            Enter();
            SetDestination();
        }

        private void SetDestination()
        {
            _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
        }
        
        private void Update()
        {
            var closestPlayerTransform = GetClosestPlayerTransform();
            if (closestPlayerTransform is not null && _agent.remainingDistance <= 0.5)
            {
                Attack();
            }
            else if (closestPlayerTransform is not null)
            {
                _agent.SetDestination(closestPlayerTransform.position);
                Run();
                _agent.speed = 4f;
            }
            else if (_agent.remainingDistance <= 3 && !_isWaiting)
            {
                _agent.speed = 2f;
                Run();
                StartCoroutine(PauseForSeconds(Random.Range(minWaitingTime, maxWaitingTime)));
            }
            
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsAttacking", isAttacking);
            animator.SetBool("IsDie", isDying);
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
                _agent.SetDestination(transform.position + new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
            }
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

            void Enter()
            {
                isRunning = false;
                isAttacking = false;
                isDying = false;
                animator.SetTrigger("Entry");
            }

            void Run()
            {
                isRunning = true;
                isAttacking = false;
                isDying = false;
            }

            void Attack()
            {
                isRunning = false;
                isAttacking = true;
                isDying = false;
            }

            void Die()
            {
                isRunning = false;
                isAttacking = false;
                isDying = true;
            }

            void Exit()
            {
                isRunning = false;
                isAttacking = false;
                isDying = false;
            }
        }
}