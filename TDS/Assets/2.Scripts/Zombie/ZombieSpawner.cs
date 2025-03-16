using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TDS.Zombie
{
    public class ZombieSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform _tower;
        [SerializeField] private float _spawnDistance = 15f;
        [SerializeField] private float _spawnHeight = 0f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private int _maxZombies = 10;
        [SerializeField] private float _spawnYOffset = 1f;
        
        [Header("Spawn Range")]
        [SerializeField] private float _minSpawnY = -4f;
        [SerializeField] private float _maxSpawnY = 4f;
        
        private ZombiePool _zombiePool;
        private CancellationTokenSource _cts;
        
        private void Start()
        {
            _zombiePool = GetComponent<ZombiePool>();
            Debug.Assert(_zombiePool != null, "ZombiePool 컴포넌트가 없습니다.");
            Debug.Assert(_tower != null, "타워가 설정되지 않았습니다.");
            
            _cts = new CancellationTokenSource();
            StartSpawning().Forget();
        }
        
        private async UniTaskVoid StartSpawning()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_zombiePool.GetActiveZombieCount() < _maxZombies)
                {
                    SpawnZombie();
                }
                
                await UniTask.Delay((int)(_spawnInterval * 1000), cancellationToken: _cts.Token);
            }
        }
        
        private void SpawnZombie()
        {
            // 타워의 현재 위치에서 오른쪽으로 일정 거리에 스폰
            float spawnX = _tower.position.x + _spawnDistance;
            float spawnY = Random.Range(_minSpawnY, _maxSpawnY);
            
            Vector3 spawnPosition = new Vector3(spawnX, spawnY + _spawnYOffset, _spawnHeight);
            
            GameObject zombie = _zombiePool.GetZombie();
            if (zombie != null)
            {
                zombie.transform.position = spawnPosition;
                zombie.SetActive(true);
            }
        }
        
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_tower == null) return;
            
            // 스폰 영역 시각화
            Gizmos.color = Color.red;
            Vector3 spawnCenter = new Vector3(_tower.position.x + _spawnDistance, 
                (_minSpawnY + _maxSpawnY) * 0.5f + _spawnYOffset, _spawnHeight);
            Vector3 spawnSize = new Vector3(0.5f, _maxSpawnY - _minSpawnY, 0.1f);
            Gizmos.DrawWireCube(spawnCenter, spawnSize);
        }
#endif
    }
} 