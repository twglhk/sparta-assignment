using UnityEngine;
using UniRx;

namespace TDS.Zombie
{
    public class ZombieModel
    {
        // 기본 스탯
        private readonly ReactiveProperty<int> _maxHp;
        private readonly ReactiveProperty<int> _currentHp;
        private readonly ReactiveProperty<bool> _isDead;
        
        // 읽기 전용 프로퍼티
        public IReadOnlyReactiveProperty<int> MaxHp => _maxHp;
        public IReadOnlyReactiveProperty<int> CurrentHp => _currentHp;
        public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
        
        // 현재 체력 비율 (0 ~ 1)
        public float HpRatio => (float)_currentHp.Value / _maxHp.Value;
        private CompositeDisposable disposables = new CompositeDisposable();
        
        public ZombieModel(int maxHp)
        {
            _maxHp = new ReactiveProperty<int>(maxHp);
            _currentHp = new ReactiveProperty<int>(maxHp);
            _isDead = new ReactiveProperty<bool>(false);
            
            // 체력이 0 이하가 되면 사망 상태로 전환
            _currentHp
                .Where(hp => hp <= 0)
                .Subscribe(_ => _isDead.Value = true)
                .AddTo(disposables);
        }
        
        public void TakeDamage(int damage)
        {
            if (_isDead.Value) return;
            
            _currentHp.Value = Mathf.Max(0, _currentHp.Value - damage);
        }
        
        public void Reset()
        {
            _currentHp.Value = _maxHp.Value;
            _isDead.Value = false;
        }
        
        public void Dispose()
        {
            disposables.Dispose();
        }
    }
} 