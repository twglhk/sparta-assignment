using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TDS.Zombie
{
    /// <summary>
    /// 좀비의 이동과 장애물 타기 동작을 제어하는 컴포넌트
    /// </summary>
    public sealed class ZombieMoveController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private ZombieData _zombieData;

        private LayerMask _zombieLayer;
        private RaycastHit2D[] _frontHits;
        private RaycastHit2D[] _backHits;
        private RaycastHit2D[] _frontUpperHits;
        private CancellationTokenSource _cts;
        
        // 부유 상태 관련 변수들
        private bool _isFloating;
        private bool _canStartFloating;
        private float _floatCooldownTimer;
        private float _floatProgressTimer;
        private Vector2 _floatStartPosition;
        private Vector2 _floatTargetPosition;

        // 상수 정의
        private const int MAX_RAYCAST_HITS = 2;
        private const int RAYCAST_SELF_INDEX = 1;
        private const float MOVE_DIRECTION = -1f;
        private const float FLOAT_HEIGHT_MULTIPLIER = 0.5f;
        private const float POSITION_SMOOTHING_MULTIPLIER = 10f;
        private const float HALF_PI = Mathf.PI * 0.5f;
        private const float INITIAL_COOLDOWN = 1f;

        private static readonly Vector2 BaseRaycastDirection = Vector2.left;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_zombieData == null)
            {
                Debug.LogError($"[{nameof(ZombieMoveController)}] ZombieData is not assigned to {gameObject.name}!");
            }
        }

        private void OnDrawGizmos()
        {
            if (_zombieData == null) return;

            Vector2 rayStart = GetRaycastOrigin();

            // 레이캐스트 방향 계산
            Vector2 frontDir = CalculateRaycastDirection(_zombieData.FrontRaycastAngle);
            Vector2 backDir = CalculateRaycastDirection(_zombieData.BackRaycastAngle, true);
            Vector2 upperDir = CalculateRaycastDirection(_zombieData.UpperRaycastAngle);

            // 레이캐스트 시각화
            DrawRaycastGizmo(rayStart, frontDir, _zombieData.RaycastDistance, Color.red);
            DrawRaycastGizmo(rayStart, backDir, _zombieData.RaycastDistance, Color.blue);
            DrawRaycastGizmo(rayStart, upperDir, _zombieData.UpperRaycastDistance, Color.yellow);
        }

        private void DrawRaycastGizmo(Vector2 start, Vector2 direction, float distance, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(start, direction * distance);
        }
#endif

        private void Start()
        {
            InitializeComponents();
            StartFloatingCheck().Forget();
        }

        private void InitializeComponents()
        {
            _rigidbody.gravityScale = 1f;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _zombieLayer = 1 << gameObject.layer;

            _frontHits = new RaycastHit2D[MAX_RAYCAST_HITS];
            _backHits = new RaycastHit2D[MAX_RAYCAST_HITS];
            _frontUpperHits = new RaycastHit2D[MAX_RAYCAST_HITS];

            _canStartFloating = false;
            _floatCooldownTimer = INITIAL_COOLDOWN;
            _cts = new CancellationTokenSource();
        }

        private void FixedUpdate()
        {
            CheckFloatingCondition();
            
            if (_isFloating)
            {
                _floatProgressTimer += Time.fixedDeltaTime;
                ApplyFloatingMovement();
            }
            else
            {
                ApplyNormalMovement();
            }
        }

        private void ApplyNormalMovement()
        {
            Vector2 velocity = _rigidbody.velocity;
            velocity.x = _zombieData.Speed * MOVE_DIRECTION;
            _rigidbody.velocity = velocity;
        }

        private void ApplyFloatingMovement()
        {
            float progress = Mathf.Clamp01(_floatProgressTimer / _zombieData.FloatDuration);
            float easedProgress = CalculateEaseInOut(progress);
            
            // 수평 이동과 수직 이동을 분리하여 계산
            float arcHeight = CalculateArcHeight(easedProgress, _floatStartPosition, _floatTargetPosition);
            Vector2 horizontalPosition = Vector2.Lerp(_floatStartPosition, _floatTargetPosition, easedProgress);
            Vector2 targetPosition = horizontalPosition + Vector2.up * arcHeight;
            
            // 부드러운 이동을 위한 보간
            Vector2 smoothedPosition = Vector2.Lerp(
                _rigidbody.position, 
                targetPosition, 
                Time.fixedDeltaTime * POSITION_SMOOTHING_MULTIPLIER
            );
            
            _rigidbody.MovePosition(smoothedPosition);

            if (progress >= 1f)
            {
                CompleteFloating();
            }
        }

        private float CalculateArcHeight(float progress, Vector2 start, Vector2 end)
        {
            float distance = Vector2.Distance(start, end);
            return Mathf.Sin(progress * Mathf.PI) * distance * FLOAT_HEIGHT_MULTIPLIER;
        }

        private float CalculateEaseInOut(float t)
        {
            return t < 0.5f ? 
                2f * t * t : 
                1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
        }

        private void CompleteFloating()
        {
            _isFloating = false;
            _rigidbody.gravityScale = 1f;
        }

        private void CheckFloatingCondition()
        {
            if (!_canStartFloating) return;

            Vector2 rayStart = GetRaycastOrigin();
            Vector2 frontDir = CalculateRaycastDirection(_zombieData.FrontRaycastAngle);
            Vector2 backDir = CalculateRaycastDirection(_zombieData.BackRaycastAngle, true);
            Vector2 upperDir = CalculateRaycastDirection(_zombieData.UpperRaycastAngle);

            var raycastResults = PerformRaycasts(rayStart, frontDir, backDir, upperDir);

            if (ShouldStartFloating(raycastResults))
            {
                StartFloating(_frontHits[1].transform.position);
            }
        }

        private Vector2 GetRaycastOrigin()
        {
            return (Vector2)transform.position + Vector2.up * _zombieData.RaycastHeightOffset;
        }

        private Vector2 CalculateRaycastDirection(float angle, bool isReversed = false)
        {
            Vector2 direction = Quaternion.Euler(0, 0, angle) * BaseRaycastDirection;
            return isReversed ? -direction : direction;
        }

        private (bool front, bool back, bool upper) PerformRaycasts(
            Vector2 origin, 
            Vector2 frontDir, 
            Vector2 backDir, 
            Vector2 upperDir)
        {
            int frontHits = Physics2D.RaycastNonAlloc(origin, frontDir, _frontHits, _zombieData.RaycastDistance, _zombieLayer);
            int backHits = Physics2D.RaycastNonAlloc(origin, backDir, _backHits, _zombieData.RaycastDistance, _zombieLayer);
            int upperHits = Physics2D.RaycastNonAlloc(origin, upperDir, _frontUpperHits, _zombieData.UpperRaycastDistance, _zombieLayer);

            return (
                frontHits > RAYCAST_SELF_INDEX,
                backHits > RAYCAST_SELF_INDEX,
                upperHits > RAYCAST_SELF_INDEX
            );
        }

        private bool ShouldStartFloating((bool front, bool back, bool upper) raycastResults)
        {
            return raycastResults.front && !raycastResults.back && !raycastResults.upper;
        }

        private void StartFloating(Vector2 targetPos)
        {
            _isFloating = true;
            _canStartFloating = false;
            _floatProgressTimer = 0f;
            _floatCooldownTimer = 0f;
            
            _floatStartPosition = transform.position;
            _floatTargetPosition = targetPos + Vector2.up * _zombieData.FloatHeightOffset;
            
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.gravityScale = 0f;
        }

        private async UniTaskVoid StartFloatingCheck()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await UniTask.WaitForFixedUpdate(_cts.Token);

                if (!_canStartFloating && !_isFloating)
                {
                    _floatCooldownTimer += Time.fixedDeltaTime;
                    if (_floatCooldownTimer >= _zombieData.FloatCooldown)
                    {
                        _canStartFloating = true;
                        _rigidbody.gravityScale = 1f;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}