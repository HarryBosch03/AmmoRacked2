
using AmmoRacked2.Runtime.Health;
using UnityEngine;

namespace AmmoRacked2.Runtime.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Config/Tank Settings")]
    public class TankSettings : ScriptableObject
    {
        public float moveSpeed = 10.0f;
        public float moveAccelerationTime = 1.0f;
        public float turnSpeedDegrees = 1.0f;
        public float turnAccelerationTime = 0.04f;
        [Range(0.0f, 1.0f)]
        public float tangentialFriction = 0.8f;

        [Space]
        public int maxHealth;

        [Space]
        public Projectile projectilePrefab;
        public DamageArgs damage;
        public float muzzleSpeed;
        public float fireDelay;
        
        [Space]
        public Vector2 leanScale = Vector2.one;
        [Range(0.0f, 1.0f)]
        public float leanSmoothing;
    }
}