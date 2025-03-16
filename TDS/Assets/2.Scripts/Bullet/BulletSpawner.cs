using UnityEngine;

namespace TDS.Bullet
{
    public sealed class BulletSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private int _maxBullets = 100;
        
        private BulletPool _bulletPool;
        
        private void Start()
        {
            _bulletPool = GetComponent<BulletPool>();
            Debug.Assert(_bulletPool != null, "BulletPool 컴포넌트가 없습니다.");
            Debug.Assert(_firePoint != null, "발사 위치가 설정되지 않았습니다.");
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
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_firePoint == null) return;
            
            // 발사 위치 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, 0.2f);
        }
#endif
    }
} 