using UnityEngine;

namespace TDS.Cam
{
    /// <summary>
    /// 타워를 추적하는 카메라 컨트롤러
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new(0f, 0f, -10f);
        
        [Header("Follow Settings")]
        [SerializeField] private bool _followX = true;
        [SerializeField] private bool _followY = false;
        
        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_target != null, "추적할 타겟이 설정되지 않았습니다.");
            
            // 초기 위치 설정
            Vector3 targetPosition = _target.position + _offset;
            transform.position = targetPosition;
        }
        
        private void Update()
        {
            if (_target == null) return;

            Vector3 newPosition = transform.position;
            
            // X, Y 축 추적 여부에 따라 위치 업데이트
            if (_followX) newPosition.x = _target.position.x + _offset.x;
            if (_followY) newPosition.y = _target.position.y + _offset.y;
            
            // Z 값은 항상 offset 값 유지
            newPosition.z = _offset.z;
            
            // 위치 즉시 적용
            transform.position = newPosition;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 에디터에서 오프셋 변경 시 z값 유지
            _offset.z = -10f;
        }
#endif
    }
} 