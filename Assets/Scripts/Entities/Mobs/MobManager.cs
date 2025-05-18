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
        
        [SerializeField] private Transform aggressiveSpawners;
        [SerializeField] private uint numberOfAggressiveMobs = 3;
        [SerializeField] private float minAggressiveSpawnInterval = 10f;
        [SerializeField] private float maxAggressiveSpawnInterval = 120f;
        private List<GameObject> _aliveAggressiveMobs = new();
        
        [Header("Aggressive mobs settings")]
        [SerializeField] private Transform passiveSpawners;
        [SerializeField] private uint numberOfPassiveMobs = 20;

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

            foreach (Transform tr in aggressiveSpawners)
                if (!NavMesh.SamplePosition(tr.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    throw new ArgumentException($"The given aggressive mob spawner '{tr.gameObject.name}' is not close enough to the navmesh.");
            
            foreach (Transform tr in passiveSpawners)
                if (!NavMesh.SamplePosition(tr.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    throw new ArgumentException($"The given passive mob spawner '{tr.gameObject.name}' is not close enough to the navmesh.");
            
            SpawnPassiveMobs();
            StartCoroutine(LowFrequencyUpdate());
        }

        [Server]
        private void SpawnPassiveMobs()
        {
            for (int i = 0; i < numberOfPassiveMobs; i++)
                _ = SpawnPassive();
        }

        [Server]
        private GameObject SpawnPassive()
        {
            Vector3 spawnPos = Choose(passiveSpawners.OfType<Transform>().ToList()).position;
            GameObject mob = Instantiate(Resources.Load<GameObject>("Prefabs/Mobs/PassiveMob"), spawnPos, Quaternion.identity);
            NetworkServer.Spawn(mob);
            return mob;
        }

        private IEnumerator LowFrequencyUpdate()
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
        
        [Server]
        private GameObject SpawnAggressive()
        {
            Vector3 spawnPos = Choose(passiveSpawners.OfType<Transform>().ToList()).position;
            GameObject mob = Instantiate(Resources.Load<GameObject>("Prefabs/Mobs/AggressiveMob"), spawnPos, Quaternion.identity);
            NetworkServer.Spawn(mob);
            mob.transform.position = Choose(aggressiveSpawners.OfType<Transform>().ToList()).position;
            return mob;
        }

        private static T Choose<T>(List<T> list)
        {
            if (list.Count == 0)
                throw new Exception("Cannot choose from an empty list.");

            return list[Random.Range(0, list.Count)];
        }
    }
}