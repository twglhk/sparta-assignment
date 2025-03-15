using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace TDS.Map
{
    /// <summary>
    /// 맵의 무한 스크롤을 관리하는 컴포넌트
    /// </summary>
    public sealed class MapScroller : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private Transform[] _mapSegments;
        [SerializeField] private SpriteRenderer[] _mapRenderers;
        [SerializeField] private Transform _tower;
        [SerializeField] private float _mapOverlap = 0.1f; // 맵 중첩 크기
        
        [Header("Check Settings")]
        [SerializeField] private int _checkIntervalMs = 1000; // 위치 체크 간격 (밀리초)
        
        private List<float> _mapLengths;
        private int _currentMapIndex;
        private Vector3[] _initialPositions;
        private CancellationTokenSource _cts;
        
        private void Start()
        {
            InitializeComponents();

            _cts = new CancellationTokenSource();
            StartPositionCheck().Forget();
        }
        
        private void InitializeComponents()
        {
            Debug.Assert(_mapSegments != null && _mapSegments.Length >= 2, "최소 2개의 맵 세그먼트가 필요합니다.");
            Debug.Assert(_mapRenderers != null && _mapRenderers.Length >= 2, "최소 2개의 맵 렌더러가 필요합니다.");
            Debug.Assert(_mapSegments.Length == _mapRenderers.Length, "맵 세그먼트와 렌더러의 개수가 일치하지 않습니다.");
            Debug.Assert(_tower != null, "타워가 설정되지 않았습니다.");
            
            // 각 맵의 길이 계산
            _mapLengths = new List<float>(_mapRenderers.Length);
            foreach (var renderer in _mapRenderers)
            {
                Debug.Assert(renderer != null, "맵 렌더러가 없습니다.");
                _mapLengths.Add(renderer.bounds.size.x);
            }
            
            // 초기 위치 저장
            _initialPositions = new Vector3[_mapSegments.Length];
            for (int i = 0; i < _mapSegments.Length; i++)
            {
                _initialPositions[i] = _mapSegments[i].position;
            }
            
            // 맵들을 순차적으로 배치
            ArrangeMapSegments();
        }

        private void ArrangeMapSegments()
        {
            Vector3 currentPosition = _mapSegments[0].position;

            // 첫 번째 맵은 그대로 두고, 나머지 맵들을 순차적으로 배치
            for (int i = 1; i < _mapSegments.Length; i++)
            {
                float previousMapLength = _mapLengths[i - 1];
                float effectiveLength = previousMapLength - _mapOverlap;
                currentPosition += Vector3.right * effectiveLength;
                _mapSegments[i].position = currentPosition;
            }
        }
        
        private async UniTaskVoid StartPositionCheck()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                CheckAndUpdateMapPosition();
                await UniTask.Delay(_checkIntervalMs, cancellationToken: _cts.Token);
            }
        }
        
        private void CheckAndUpdateMapPosition()
        {
            // 다음 맵의 중앙 위치 계산
            int nextMapIndex = (_currentMapIndex + 1) % _mapSegments.Length;
            float nextMapLength = _mapLengths[nextMapIndex];
            float nextMapCenterX = _mapSegments[nextMapIndex].position.x + (nextMapLength * 0.5f);
            
            // 타워가 다음 맵의 중앙을 지났는지 확인
            if (_tower.position.x >= nextMapCenterX)
            {
                // 마지막으로 보이는 맵의 인덱스 계산
                int visibleMapCount = _mapSegments.Length - 1;
                int lastVisibleMapIndex = (nextMapIndex + visibleMapCount - 1) % _mapSegments.Length;
                
                // 마지막 보이는 맵의 길이를 기준으로 다음 위치 계산
                float lastMapLength = _mapLengths[lastVisibleMapIndex];
                float effectiveLength = lastMapLength - _mapOverlap;
                
                // 현재 맵(이전 맵)을 마지막 보이는 맵 뒤로 이동
                Vector3 nextPosition = _mapSegments[lastVisibleMapIndex].position + Vector3.right * effectiveLength;
                _mapSegments[_currentMapIndex].position = nextPosition;
                
                // 현재 맵 인덱스 업데이트
                _currentMapIndex = nextMapIndex;
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_mapSegments == null || _mapRenderers == null || 
                _mapSegments.Length == 0 || _mapRenderers.Length == 0) return;
            
            // 맵 중앙 지점 시각화
            for (int i = 0; i < _mapSegments.Length; i++)
            {
                if (_mapSegments[i] == null || _mapRenderers[i] == null) continue;
                
                float mapLength = _mapRenderers[i].bounds.size.x;
                float mapCenterX = _mapSegments[i].position.x + (mapLength * 0.5f);
                Vector3 centerPosition = new Vector3(mapCenterX, _mapSegments[i].position.y, _mapSegments[i].position.z);
                
                // 현재 맵은 녹색, 다음 맵은 파란색, 나머지는 노란색으로 표시
                Color gizmoColor = Color.yellow;
                if (i == _currentMapIndex) gizmoColor = Color.green;
                else if (i == (_currentMapIndex + 1) % _mapSegments.Length) gizmoColor = Color.blue;
                
                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(centerPosition + Vector3.up * 5f, centerPosition + Vector3.down * 5f);
            }
        }
#endif

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
} 