using UnityEngine;
using UnityEngine.Pool;
using TDS.Zombie;
using TDS.Common;

namespace TDS.Bullet
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private BulletData _data;
        
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private Vector2 _direction;
        private float _distanceTraveled;
        private Vector2 _startPosition;
        private IObjectPool<GameObject> _pool;
        private bool _isReturning;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            Debug.Assert(_data != null, "BulletData가 설정되지 않았습니다.");
        }
        
        public void Initialize(Vector2 direction, IObjectPool<GameObject> pool)
        {
            _pool = pool;
            _direction = direction.normalized;
            _startPosition = transform.position;
            _distanceTraveled = 0.0f;
            _isReturning = false;
            
            // 컴포넌트 활성화
            _rb.simulated = true;
            _collider.enabled = true;
            
            // 총알 회전
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 속도 설정
            _rb.velocity = _direction * _data.Speed;
        }
        
        private void LateUpdate()
        {
            if (_isReturning) return;
            
            // 이동 거리 체크
            _distanceTraveled = Vector2.Distance(_startPosition, transform.position);
            if (_distanceTraveled > _data.MaxDistance)
            {
                ReturnToPool();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 이미 반환 중이거나 컴포넌트가 비활성화된 상태면 무시
            if (_isReturning || !_collider.enabled) return;
            
            // 좀비 레이어 체크
            if (other.gameObject.layer != Layers.ZOMBIE)
            {
                return;
            }

            bool isZombieHealth = other.TryGetComponent(out ZombieHealth zombieHealth);
            if (!isZombieHealth)
            {
                Debug.LogWarning("ZombieHealth가 없습니다.");
                return;
            }
            
            // 충돌 처리 전 물리 컴포넌트 비활성화
            DisablePhysics();
            
            // 랜덤 데미지 계산
            int damage = Random.Range(_data.MinDamage, _data.MaxDamage + 1);
            zombieHealth.TakeDamage(damage);
            
            ReturnToPool();
        }
        
        private void DisablePhysics()
        {
            _rb.simulated = false;
            _collider.enabled = false;
            _rb.velocity = Vector2.zero;
        }
        
        private void ReturnToPool()
        {
            if (_isReturning) return;
            
            _isReturning = true;
            DisablePhysics();
            
            if (_pool != null)
            {
                _pool.Release(gameObject);
            }
        }
        
        private void OnDisable()
        {
            DisablePhysics();
            _isReturning = false;
        }
        
        private void OnEnable()
        {
            // 활성화될 때는 물리 컴포넌트를 초기화만 하고,
            // Initialize에서 실제로 활성화
            DisablePhysics();
        }
    }
} 