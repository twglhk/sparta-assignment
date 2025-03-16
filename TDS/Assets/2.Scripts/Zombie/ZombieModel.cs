using UnityEngine;
using UniRx;

namespace TDS.Zombie
{
    public class ZombieModel
    {
        // 기본 스탯
        private readonly ReactiveProperty<float> _maxHp;
        private readonly ReactiveProperty<float> _currentHp;
        private readonly ReactiveProperty<bool> _isDead;
        
        // 읽기 전용 프로퍼티
        public IReadOnlyReactiveProperty<float> MaxHp => _maxHp;
        public IReadOnlyReactiveProperty<float> CurrentHp => _currentHp;
        public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
        
        // 현재 체력 비율 (0 ~ 1)
        public float HpRatio => _currentHp.Value / _maxHp.Value;
        private CompositeDisposable disposables = new CompositeDisposable();
        
        public ZombieModel(float maxHp)
        {
            _maxHp = new ReactiveProperty<float>(maxHp);
            _currentHp = new ReactiveProperty<float>(maxHp);
            _isDead = new ReactiveProperty<bool>(false);
            
            // 체력이 0 이하가 되면 사망 상태로 전환
            _currentHp
                .Where(hp => hp <= 0)
                .Subscribe(_ => _isDead.Value = true)
                .AddTo(disposables);
        }
        
        public void TakeDamage(float damage)
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