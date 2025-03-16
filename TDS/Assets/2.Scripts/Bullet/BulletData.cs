using UnityEngine;

namespace TDS.Bullet
{
    [CreateAssetMenu(fileName = "BulletData", menuName = "TDS/Bullet Data")]
    public sealed class BulletData : ScriptableObject
    {
        [Header("Movement")]
        public float Speed = 20f;
        public float MaxDistance = 30f;  // 최대 날아갈 수 있는 거리
        
        [Header("Damage")]
        public int MinDamage = 10;
        public int MaxDamage = 20;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            MinDamage = Mathf.Max(1, MinDamage);
            MaxDamage = Mathf.Max(MinDamage, MaxDamage);
        }
#endif
    }
} 