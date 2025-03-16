using UnityEngine;
using UnityEngine.Pool;
using TDS.Zombie;

namespace TDS.Bullet
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private BulletData _data;
        
        private Rigidbody2D _rb;
        private Vector2 _direction;
        private float _distanceTraveled;
        private Vector2 _startPosition;
        private IObjectPool<GameObject> _pool;
        private static readonly int ZOMBIE_LAYER = 6;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            Debug.Assert(_data != null, "BulletData가 설정되지 않았습니다.");
        }
        
        public void Initialize(Vector2 direction, IObjectPool<GameObject> pool)
        {
            _pool = pool;
            _direction = direction.normalized;
            _startPosition = transform.position;
            _distanceTraveled = 0.0f;
            
            // 총알 회전
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 속도 설정
            _rb.velocity = _direction * _data.Speed;
        }
        
        private void LateUpdate()
        {
            // 이동 거리 체크
            _distanceTraveled = Vector2.Distance(_startPosition, transform.position);
            if (_distanceTraveled > _data.MaxDistance)
            {
                ReturnToPool();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 좀비 레이어 체크
            if (other.gameObject.layer != ZOMBIE_LAYER)
            {
                return;
            }

            bool isZombieHealth = other.TryGetComponent(out ZombieHealth zombieHealth);
            Debug.Assert(isZombieHealth, "ZombieHealth가 없습니다.");
            
            // 랜덤 데미지 계산
            int damage = Random.Range(_data.MinDamage, _data.MaxDamage + 1);
            zombieHealth.TakeDamage(damage);
            
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            _rb.velocity = Vector2.zero;
            _pool.Release(gameObject);
        }
    }
} 