using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TDS.Zombie
{
    public sealed class ZombieMoveController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private ZombieData _zombieData;

        private LayerMask _zombieLayer;
        private RaycastHit2D[] _frontHits;
        private RaycastHit2D[] _backHits;
        private RaycastHit2D[] _frontUpperHits;  // 전방 상단 체크용
        private CancellationTokenSource _cts;
        private bool _isFloating;           // 현재 부력이 적용 중인지
        private bool _canStartNewJump = false;  // 시작 시 false로 변경
        private float _jumpTimer = 1.0f;        // 시작 시 쿨다운이 완료된 상태로 시작

        private const int MAX_HITS = 2;
        private const float X_MOVE_DIRECTION = -1.0f;
        private const float JUMP_THRESHOLD = 0.1f;
        private const int SELF_INDEX = 1;
        private const float COOLDOWN_CHECK_INTERVAL = 0.1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_zombieData == null)
            {
                Debug.LogError($"ZombieData is not assigned to {gameObject.name}!");
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _zombieData == null) return;

            Vector2 rayStart = (Vector2)transform.position + Vector2.up * _zombieData.RaycastHeightOffset;
            Vector2 baseDirection = Vector2.right * (_rigidbody.velocity.x > 0 ? 1 : -1);

            Vector2 frontDirection = Quaternion.Euler(0, 0, _zombieData.FrontRaycastAngle) * baseDirection;
            Vector2 backDirection = Quaternion.Euler(0, 0, _zombieData.BackRaycastAngle) * -baseDirection;
            Vector2 frontUpperDirection = Quaternion.Euler(0, 0, _zombieData.UpperRaycastAngle) * baseDirection;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayStart, frontDirection * _zombieData.RaycastDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayStart, backDirection * _zombieData.RaycastDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(rayStart, frontUpperDirection * _zombieData.UpperRaycastDistance);
        }
#endif

        private void Start()
        {
            _rigidbody.gravityScale = 0f;  // 기본 중력 비활성화
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _zombieLayer = 1 << gameObject.layer;

            _frontHits = new RaycastHit2D[MAX_HITS];
            _backHits = new RaycastHit2D[MAX_HITS];
            _frontUpperHits = new RaycastHit2D[MAX_HITS];

            _cts = new CancellationTokenSource();
            StartJumpCheck().Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            CheckJumpCondition();
            ApplyCustomGravity();
        }

        private void ApplyMovement()
        {
            Vector2 velocity = _rigidbody.velocity;
            
            // 부유 중이고 앞에 장애물이 있을 때는 수평 속도 감소
            if (_isFloating && CheckFrontObstacle())
            {
                velocity.x = _zombieData.Speed * X_MOVE_DIRECTION * 0.7f;
            }
            else
            {
                velocity.x = _zombieData.Speed * X_MOVE_DIRECTION;
            }
            
            _rigidbody.velocity = velocity;
        }

        private bool CheckFrontObstacle()
        {
            Vector2 rayStart = (Vector2)transform.position + Vector2.up * _zombieData.RaycastHeightOffset;
            Vector2 baseDirection = Vector2.right * (_rigidbody.velocity.x > 0 ? 1 : -1);
            Vector2 frontDirection = Quaternion.Euler(0, 0, _zombieData.FrontRaycastAngle) * baseDirection;
            
            int frontHitCount = Physics2D.RaycastNonAlloc(rayStart, frontDirection, _frontHits, _zombieData.RaycastDistance, _zombieLayer);
            return frontHitCount > SELF_INDEX;
        }

        private void CheckJumpCondition()
        {
            Vector2 rayStart = (Vector2)transform.position + Vector2.up * _zombieData.RaycastHeightOffset;
            Vector2 baseDirection = Vector2.right * (_rigidbody.velocity.x > 0 ? 1 : -1);

            Vector2 frontDirection = Quaternion.Euler(0, 0, _zombieData.FrontRaycastAngle) * baseDirection;
            Vector2 backDirection = Quaternion.Euler(0, 0, _zombieData.BackRaycastAngle) * -baseDirection;
            Vector2 frontUpperDirection = Quaternion.Euler(0, 0, _zombieData.UpperRaycastAngle) * baseDirection;

            int frontHitCount = Physics2D.RaycastNonAlloc(rayStart, frontDirection, _frontHits, _zombieData.RaycastDistance, _zombieLayer);
            int backHitCount = Physics2D.RaycastNonAlloc(rayStart, backDirection, _backHits, _zombieData.RaycastDistance, _zombieLayer);
            int frontUpperHitCount = Physics2D.RaycastNonAlloc(rayStart, frontUpperDirection, _frontUpperHits, _zombieData.UpperRaycastDistance, _zombieLayer);

            bool hasZombieInFront = frontHitCount > SELF_INDEX;
            bool hasZombieInBack = backHitCount > SELF_INDEX;
            bool hasZombieAbove = frontUpperHitCount > SELF_INDEX;

            if (hasZombieInFront && !hasZombieInBack && !hasZombieAbove && _canStartNewJump)
            {
                StartJump();
            }

            if (_isFloating)
            {
                if (_rigidbody.velocity.y < _zombieData.MaxFloatSpeed)
                {
                    Vector2 floatDirection = hasZombieInFront ? 
                        (Vector2.up * 0.9f + Vector2.right * X_MOVE_DIRECTION * 0.1f).normalized : 
                        Vector2.up;

                    _rigidbody.AddForce(floatDirection * _zombieData.FloatForce, ForceMode2D.Force);
                }
            }
        }

        private void StartJump()
        {
            _isFloating = true;
            _canStartNewJump = false;
            _jumpTimer = 0f;
        }

        private void ApplyCustomGravity()
        {
            float gravityMultiplier = _isFloating ? 0.5f : 1.0f;
            _rigidbody.AddForce(Vector2.down * (_zombieData.CustomGravity * gravityMultiplier), ForceMode2D.Force);
        }

        private async UniTaskVoid StartJumpCheck()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (!_canStartNewJump)
                {
                    _jumpTimer += Time.fixedDeltaTime;

                    if (_jumpTimer >= _zombieData.FloatDuration && _isFloating)
                    {
                        _isFloating = false;
                    }

                    if (_jumpTimer >= _zombieData.JumpCooldown)
                    {
                        _canStartNewJump = true;
                    }
                }
                await UniTask.WaitForFixedUpdate(_cts.Token);
            }
        }
    }
}