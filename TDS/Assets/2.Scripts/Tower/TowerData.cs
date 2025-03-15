using UnityEngine;

namespace TDS.Tower
{
    [CreateAssetMenu(fileName = "TowerData", menuName = "TDS/Tower Data")]
    public sealed class TowerData : ScriptableObject
    {
        [Header("Movement")]
        public float MoveSpeed = 2f;
        
        [Header("Combat")]
        public float AttackRange = 5f;
        public float AttackRate = 1f;
        public int AttackDamage = 10;
    }
} 