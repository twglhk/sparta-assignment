using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace TDS.Zombie
{
    public sealed class ZombiePool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject _zombiePrefab;
        [SerializeField] private int _defaultCapacity = 20;
        [SerializeField] private int _maxPoolSize = 50;
        
        private IObjectPool<GameObject> _pool;
        private Transform _poolContainer;
        private HashSet<GameObject> _activeZombies;
        
        private void Awake()
        {
            InitializePool();
        }
        
        private void InitializePool()
        {
            _poolContainer = new GameObject("Zombie Pool").transform;
            _poolContainer.SetParent(transform);
            _activeZombies = new HashSet<GameObject>();
            
            _pool = new ObjectPool<GameObject>(
                createFunc: CreateZombie,
                actionOnGet: OnGetZombie,
                actionOnRelease: OnReleaseZombie,
                actionOnDestroy: OnDestroyZombie,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize
            );
        }
        
        private GameObject CreateZombie()
        {
            var zombie = Instantiate(_zombiePrefab, _poolContainer);
            zombie.GetComponent<ZombieRoot>().Initialize(_pool);
            zombie.SetActive(false);
            return zombie;
        }
        
        private void OnGetZombie(GameObject zombie)
        {
            zombie.SetActive(true);
            _activeZombies.Add(zombie);
        }
        
        private void OnReleaseZombie(GameObject zombie)
        {
            zombie.GetComponent<ZombieRoot>().Reset();
            zombie.SetActive(false);
            _activeZombies.Remove(zombie);
        }
        
        private void OnDestroyZombie(GameObject zombie)
        {
            Destroy(zombie);
        }
        
        public GameObject GetZombie()
        {
            return _pool.Get();
        }
        
        public void ReturnZombie(GameObject zombie)
        {
            _pool.Release(zombie);
        }
        
        public int GetActiveZombieCount()
        {
            return _activeZombies.Count;
        }
        
        private void OnDestroy()
        {
            _pool.Clear();
        }
    }
} 