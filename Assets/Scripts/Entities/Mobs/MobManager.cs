using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Reconnect.Pathfinding
{
    public class MobManager : NetworkBehaviour
    {
        public static MobManager Instance { get; private set; }

        [Header("Aggressive mobs settings")]
        
        [SerializeField]
        [Tooltip("The game object whose children's transforms are the spawn positions.")]
        private Transform aggressiveSpawners;
        [SerializeField] private uint numberOfAggressiveMobs = 3;
        [SerializeField] private float minAggressiveSpawnInterval = 10f;
        [SerializeField] private float maxAggressiveSpawnInterval = 120f;
        private List<GameObject> _aliveAggressiveMobs = new();
        
        [Header("Aggressive mobs settings")]
        [SerializeField]
        [Tooltip("The game object whose children's transforms are the spawn positions.")]
        private Transform passiveSpawners;
        [SerializeField] private uint numberOfPassiveMobs = 20;
        [SerializeField] private float minPassiveSpawnInterval = 8f;
        [SerializeField] private float maxPassiveSpawnInterval = 15f;

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Check that the given aggressive spawners are placed around the navmesh
            foreach (Transform childTransform in aggressiveSpawners)
                if (!NavMesh.SamplePosition(childTransform.position, out NavMeshHit _, 10f, NavMesh.AllAreas))
                    Debug.LogException(new ArgumentException($"The given aggressive mob spawner '{childTransform.gameObject.name}' is not close enough to the navmesh."));
            
            // Check that the given passive spawners are placed around the navmesh
            foreach (Transform childTransform in passiveSpawners)
                if (!NavMesh.SamplePosition(childTransform.position, out NavMeshHit _, 10f, NavMesh.AllAreas))
                    Debug.LogException(new ArgumentException($"The given passive mob spawner '{childTransform.gameObject.name}' is not close enough to the navmesh."));
            
            StartCoroutine(SpawnPassiveMobs());
            
            StartCoroutine(UpdateForAggressive());
        }

        /// <summary>
        /// Spawns all the passive mobs across the network. This is a finite coroutine.
        /// </summary>
        private IEnumerator SpawnPassiveMobs()
        {
            for (int i = 0; i < numberOfPassiveMobs; i++)
            {
                _ = SpawnPassive();
                yield return new WaitForSeconds(Random.Range(minPassiveSpawnInterval, maxPassiveSpawnInterval));
            }
        }

        /// <summary>
        /// Spawn across the network a passive mob at a randomly chosen spawn position among the given ones.
        /// </summary>
        /// <returns>The spawned passive mob's game object.</returns>
        [Server]
        private GameObject SpawnPassive()
        {
            Vector3 spawnPos = Choose(passiveSpawners.OfType<Transform>().ToList()).position;
            GameObject mob = Instantiate(Resources.Load<GameObject>("Prefabs/Mobs/PassiveMob"), spawnPos, Quaternion.identity);
            NetworkServer.Spawn(mob);
            return mob;
        }

        /// <summary>
        /// A coroutine used as an Update function but with a low call frequency. The interval between two calls is a random number between minAggressiveSpawnInterval and maxAggressiveSpawnInterval. This is an infinite coroutine.
        /// </summary>
        /// <returns>An IEnumerator corresponding to a coroutine.</returns>
        private IEnumerator UpdateForAggressive()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minAggressiveSpawnInterval, maxAggressiveSpawnInterval));
                UpdateAggressiveMobs();
            }
        }

        [Server]
        private void UpdateAggressiveMobs()
        {
            _aliveAggressiveMobs = _aliveAggressiveMobs.Where(m => m).ToList();
            
            if (_aliveAggressiveMobs.Count < numberOfAggressiveMobs)
            {
                GameObject mob = SpawnAggressive();
                _aliveAggressiveMobs.Add(mob);
            }
        }
        
        /// <summary>
        /// Spawn across the network an aggressive mob at a randomly chosen spawn position among the given ones.
        /// </summary>
        /// <returns>The spawned aggressive mob's game object.</returns>
        [Server]
        private GameObject SpawnAggressive()
        {
            Vector3 spawnPos = Choose(aggressiveSpawners.OfType<Transform>().ToList()).position;
            GameObject mob = Instantiate(Resources.Load<GameObject>("Prefabs/Mobs/AggressiveMob"), spawnPos, Quaternion.identity);
            NetworkServer.Spawn(mob);
            return mob;
        }

        /// <summary>
        /// Selects and returns a random element from the provided list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list from which to choose a random element.</param>
        /// <returns>A randomly selected element from the list.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided list is empty.</exception>
        private static T Choose<T>(List<T> list)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot choose from an empty list.");

            return list[Random.Range(0, list.Count)];
        }
    }
}