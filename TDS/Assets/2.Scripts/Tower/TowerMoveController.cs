using UnityEngine;

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

        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_rigidbody != null, "Rigidbody2D가 할당되지 않았습니다.");
            Debug.Assert(_towerData != null, "TowerData가 할당되지 않았습니다.");
            
            _rigidbody.gravityScale = 0f;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }
        
        private void FixedUpdate()
        {
            MoveTower();
        }
        
        private void MoveTower()
        {
            Vector2 newPosition = _rigidbody.position + Vector2.right * (_towerData.MoveSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(newPosition);
        }
    }
} 