using UnityEngine;
using UniRx;

namespace TDS.Zombie
{
    public class ZombieHealth : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ZombieRoot _root;
        [SerializeField] private ZombieData _data;
        
        private ZombieModel _model;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        public IReadOnlyReactiveProperty<int> CurrentHp => _model?.CurrentHp;
        public IReadOnlyReactiveProperty<bool> IsDead => _model?.IsDead;
        public float HpRatio => _model?.HpRatio ?? 0f;
        
        private void Awake()
        {
            Debug.Assert(_data != null, "ZombieData가 설정되지 않았습니다.");
            InitializeModel();
            SetupSubscriptions();
        }
        
        private void InitializeModel()
        {
            _model = new ZombieModel(_data.MaxHp);
        }
        
        private void SetupSubscriptions()
        {
            // 체력 변화 감지
            _model.CurrentHp
                .Subscribe(OnHpChanged)
                .AddTo(_disposables);
            
            // 사망 상태 감지
            _model.IsDead
                .Where(isDead => isDead)
                .Subscribe(_ => OnDeath())
                .AddTo(_disposables);
        }

        private void OnHpChanged(int newHp)
        {
            // 체력 변화 시 필요한 처리
        }
        
        private void OnDeath()
        {
            _root.Pool.Release(gameObject);
            _model?.Reset();
        }
        
        public void TakeDamage(int damage)
        {
            int finalDamage = Mathf.Max(0, damage);
            _model.TakeDamage(finalDamage);
        }
        
        private void OnDestroy()
        {
            _disposables.Dispose();
            _model?.Dispose();
        }
    }
} 