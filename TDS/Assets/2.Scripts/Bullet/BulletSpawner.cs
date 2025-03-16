using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using TDS.Common;

namespace TDS.Bullet
{
    /// <summary>
    /// 총알 발사 클래스
    /// </summary>
    public sealed class BulletSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private int _maxBullets = 100;
        
        [Header("Auto Target Settings")]
        [SerializeField] private float _detectionRadius = 10f;
        [SerializeField] private float _fireInterval = 0.5f;
        [SerializeField] private bool _autoFire = true;
        [SerializeField] private int _maxDetectableZombies = 20;
        
        private BulletPool _bulletPool;
        private CancellationTokenSource _cts;
        private readonly Collider2D[] _zombieColliders;
        
        public BulletSpawner()
        {
            _zombieColliders = new Collider2D[_maxDetectableZombies];
        }
        
        private void Start()
        {
            _bulletPool = GetComponent<BulletPool>();
            Debug.Assert(_bulletPool != null, "BulletPool 컴포넌트가 없습니다.");
            Debug.Assert(_firePoint != null, "발사 위치가 설정되지 않았습니다.");
            
            if (_autoFire)
            {
                _cts = new CancellationTokenSource();
                StartAutoFire().Forget();
            }
        }
        
        private async UniTaskVoid StartAutoFire()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                TryFireAtNearestZombie();
                await UniTask.Delay((int)(_fireInterval * 1000), cancellationToken: _cts.Token);
            }
        }
        
        private void TryFireAtNearestZombie()
        {
            var nearestZombie = FindNearestZombieOrNull();
            if (nearestZombie == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)nearestZombie.position - (Vector2)_firePoint.position).normalized;
            SpawnBullet(direction);
        }
        
        private Transform FindNearestZombieOrNull()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                _firePoint.position,
                _detectionRadius,
                _zombieColliders,
                Layers.ZOMBIE_MASK
            );
            
            if (hitCount == 0)
            {
                return null;
            }
            
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            
            for (int i = 0; i < hitCount; i++)
            {
                var collider = _zombieColliders[i];
                if (collider == null) continue;
                
                float distance = Vector2.Distance(_firePoint.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider.transform;
                }
                
                // 다음 검사를 위해 배열 초기화
                _zombieColliders[i] = null;
            }
            
            return nearest;
        }
        
        public GameObject SpawnBullet(Vector2 direction)
        {
            if (_bulletPool.GetActiveBulletCount() >= _maxBullets)
            {
                return null;
            }
            
            var bullet = _bulletPool.GetBullet();
            bullet.transform.position = _firePoint.position;
            
            bool isBullet = bullet.TryGetComponent(out Bullet bulletComponent);
            Debug.Assert(isBullet, "Bullet 컴포넌트가 없습니다.");
            bulletComponent.Initialize(direction, _bulletPool.GetPool());
            
            return bullet;
        }
        
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 최소값 제한
            _maxDetectableZombies = Mathf.Max(1, _maxDetectableZombies);
        }
        
        private void OnDrawGizmos()
        {
            if (_firePoint == null) return;
            
            // 발사 위치 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, 0.2f);
            
            // 감지 범위 시각화
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(_firePoint.position, _detectionRadius);
        }
#endif
    }
} 