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
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _followX = true;
        [SerializeField] private bool _followY = false;
        
        private Vector3 _currentVelocity;
        private Camera _camera;
        
        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_target != null, "추적할 타겟이 설정되지 않았습니다.");
            _camera = GetComponent<Camera>();
            Debug.Assert(_camera != null, "Camera 컴포넌트를 찾을 수 없습니다.");
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;
            
            // 현재 위치에서 타겟의 위치로 부드럽게 이동
            Vector3 targetPosition = _target.position + _offset;
            
            // X, Y 축 추적 여부에 따라 현재 카메라 위치 유지
            if (!_followX) targetPosition.x = transform.position.x;
            if (!_followY) targetPosition.y = transform.position.y;
            
            // 부드러운 이동을 위해 SmoothDamp 사용
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _currentVelocity,
                1f / _smoothSpeed
            );
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