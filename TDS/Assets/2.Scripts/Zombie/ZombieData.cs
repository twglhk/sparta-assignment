using UnityEngine;

namespace TDS.Zombie
{
    [CreateAssetMenu(fileName = "ZombieData", menuName = "TDS/Zombie Data")]
    public class ZombieData : ScriptableObject
    {
        [Header("Stats")]
        public int MaxHp = 100;

        [Header("Movement")]
        public float Speed = 2f;
        public float FloatSpeed = 5f;  // 부유 시 회전 속도

        [Header("Raycast Settings")]
        public float RaycastDistance = 1f;
        public float UpperRaycastDistance = 1.5f;
        public float RaycastHeightOffset = 0.5f;
        public float FrontRaycastAngle = -15f;
        public float BackRaycastAngle = 15f;
        public float UpperRaycastAngle = 45f;

        [Header("Float Settings")]
        public float FloatDuration = 0.5f;
        public float FloatCooldown = 1.0f;
        public float FloatHeightOffset = 1.0f;  // 타겟 위치의 수직 오프셋
    }
} 