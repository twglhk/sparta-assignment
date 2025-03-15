using UnityEngine;
using System.Collections.Generic;

namespace TDS.Tower
{
    /// <summary>
    /// 타워의 이동을 제어하는 컴포넌트
    /// </summary>
    public sealed class TowerMoveController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private TowerData _towerData;
        [SerializeField] private BoxCollider2D _zombieDetector;
        
        [Header("Detection Settings")]
        [SerializeField] private LayerMask _zombieLayer;
        
        private readonly HashSet<GameObject> _zombiesInTrigger = new();
        private bool _isBlocked;

        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_rigidbody != null, "Rigidbody2D가 할당되지 않았습니다.");
            Debug.Assert(_towerData != null, "TowerData가 할당되지 않았습니다.");
            Debug.Assert(_zombieDetector != null, "ZombieDetector가 할당되지 않았습니다.");
            
            _rigidbody.gravityScale = 0f;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            _rigidbody.interpolation = RigidbodyInterpolation2D.None;
            
            // 좀비 감지용 트리거 설정
            _zombieDetector.isTrigger = true;
        }
        
        private void FixedUpdate()
        {
            // 좀비가 앞에 있으면 이동하지 않음
            if (_isBlocked) return;
            
            // 단순하게 현재 위치에서 오른쪽으로 이동
            float moveAmount = _towerData.MoveSpeed * Time.fixedDeltaTime;
            Vector2 newPosition = _rigidbody.position + (Vector2.right * moveAmount);
            _rigidbody.MovePosition(newPosition);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsZombieLayer(other.gameObject.layer)) return;
            
            // 이미 등록된 좀비가 아닐 경우에만 추가
            if (!_zombiesInTrigger.Contains(other.gameObject))
            {
                _zombiesInTrigger.Add(other.gameObject);
                UpdateBlockedState();
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsZombieLayer(other.gameObject.layer)) return;
            
            // 등록된 좀비일 경우에만 제거
            if (_zombiesInTrigger.Contains(other.gameObject))
            {
                _zombiesInTrigger.Remove(other.gameObject);
                UpdateBlockedState();
            }
        }
        
        private void UpdateBlockedState()
        {
            _isBlocked = _zombiesInTrigger.Count > 0;
        }
        
        /// <summary>
        /// 주어진 레이어가 좀비 레이어인지 확인합니다.
        /// </summary>
        private bool IsZombieLayer(int layer)
        {
            return (_zombieLayer.value & (1 << layer)) != 0;
        }
        
        private void OnDestroy()
        {
            _zombiesInTrigger.Clear();
        }
    }
} 