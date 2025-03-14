using UnityEngine;

namespace TDS.Zombie
{
    [CreateAssetMenu(fileName = "ZombieData", menuName = "TDS/Zombie Data")]
    public class ZombieData : ScriptableObject
    {
        [Header("Movement")]
        public float Speed = 2f;
        public float FloatForce = 5f;
        public float MaxFloatSpeed = 7f;
        public float CustomGravity = 15f;

        [Header("Raycast Settings")]
        public float RaycastDistance = 1f;
        public float UpperRaycastDistance = 1.5f;
        public float RaycastHeightOffset = 0.5f;
        public float FrontRaycastAngle = -15f;
        public float BackRaycastAngle = 15f;
        public float UpperRaycastAngle = 45f;

        [Header("Jump Settings")]
        public float FloatDuration = 0.5f;
        public float JumpCooldown = 1.0f;
    }
} 