using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace TDS.Bullet
{
    public sealed class BulletPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private int _defaultCapacity = 20;
        [SerializeField] private int _maxPoolSize = 100;
        
        private IObjectPool<GameObject> _pool;
        private Transform _poolContainer;
        private HashSet<GameObject> _activeBullets;
        
        private void Awake()
        {
            InitializePool();
        }
        
        private void InitializePool()
        {
            _poolContainer = new GameObject("Bullet Pool").transform;
            _poolContainer.SetParent(transform);
            _activeBullets = new HashSet<GameObject>();
            
            _pool = new ObjectPool<GameObject>(
                createFunc: CreateBullet,
                actionOnGet: OnGetBullet,
                actionOnRelease: OnReleaseBullet,
                actionOnDestroy: OnDestroyBullet,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize
            );
        }
        
        private GameObject CreateBullet()
        {
            var bullet = Instantiate(_bulletPrefab, _poolContainer);
            bullet.SetActive(false);
            return bullet;
        }
        
        private void OnGetBullet(GameObject bullet)
        {
            bullet.SetActive(true);
            _activeBullets.Add(bullet);
        }
        
        private void OnReleaseBullet(GameObject bullet)
        {
            bullet.SetActive(false);
            _activeBullets.Remove(bullet);
        }
        
        private void OnDestroyBullet(GameObject bullet)
        {
            Destroy(bullet);
        }
        
        public GameObject GetBullet()
        {
            return _pool.Get();
        }
        
        public void ReturnBullet(GameObject bullet)
        {
            _pool.Release(bullet);
        }
        
        public int GetActiveBulletCount()
        {
            return _activeBullets.Count;
        }
        
        public IObjectPool<GameObject> GetPool()
        {
            return _pool;
        }
        
        private void OnDestroy()
        {
            _pool.Clear();
        }
    }
} 