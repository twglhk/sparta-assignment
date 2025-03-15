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
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private bool _followX = true;
        [SerializeField] private bool _followY = false;
        
        private Vector3 _currentVelocity;
        private Camera _camera;
        private Vector3 _targetPosition;
        
        private void Start()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_target != null, "추적할 타겟이 설정되지 않았습니다.");
            _camera = GetComponent<Camera>();
            Debug.Assert(_camera != null, "Camera 컴포넌트를 찾을 수 없습니다.");
            
            // 초기 위치 설정
            _targetPosition = _target.position + _offset;
            transform.position = _targetPosition;
        }
        
        private void FixedUpdate()
        {   
            // 타겟 위치 업데이트 (FixedUpdate에서 계산)
            _targetPosition = _target.position + _offset;
            
            // X, Y 축 추적 여부에 따라 현재 카메라 위치 유지
            if (!_followX) _targetPosition.x = transform.position.x;
            if (!_followY) _targetPosition.y = transform.position.y;
        }
        
        private void LateUpdate()
        {
            Vector3 newPosition = Vector3.SmoothDamp(
                transform.position,
                _targetPosition,
                ref _currentVelocity,
                _smoothTime
            );

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