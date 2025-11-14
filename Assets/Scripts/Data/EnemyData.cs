using UnityEngine;

namespace Game.AI
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Data/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Enemy type name")]
        public string enemyName = "Basic Enemy";

        [Tooltip("Enemy type for behavior")]
        public EnemyType enemyType = EnemyType.Melee;

        [Header("Stats")]
        [Tooltip("Maximum health")]
        public int maxHealth = 50;

        [Tooltip("Damage dealt to player")]
        public int attackDamage = 10;

        [Tooltip("Movement speed")]
        public float moveSpeed = 3f;

        [Header("Combat")]
        [Tooltip("How far enemy can detect player")]
        public float detectionRange = 5f;

        [Tooltip("How close to get before attacking")]
        public float attackRange = 1f;

        [Tooltip("Time between attacks")]
        public float attackCooldown = 1.5f;

        [Tooltip("Vision cone width in degrees (e.g., 90 = 45° left/right)")]
        [Range(0f, 180f)]
        public float viewAngle = 90f;

        [Header("Patrol (Melee/Guardian only)")]
        [Tooltip("Distance to patrol left/right")]
        public float patrolDistance = 3f;

        [Tooltip("Time to wait at patrol points")]
        public float patrolWaitTime = 1f;

        [Header("Ranged (Ranged type only)")]
        [Tooltip("Projectile prefab to shoot")]
        public GameObject projectilePrefab;

        [Tooltip("Projectile speed")]
        public float projectileSpeed = 8f;

        [Tooltip("Time between shots")]
        public float fireRate = 2f;

        [Tooltip("Delay before firing projectile (for aiming animation)")]
        public float aimDelay = 0.4f;

        [Header("Flight Settings (Flying only)")]
        public float patrolRadius = 3f;
        public float hoverHeight = 2f;
        public float verticalSpeed = 2f;

        [Header("Loot")]
        [Tooltip("Item to drop on death (optional)")]
        public GameObject dropItem;

        [Tooltip("Chance to drop item (0-1)")]
        [Range(0f, 1f)]
        public float dropChance = 0.3f;

        [Header("Audio")]
        [Tooltip("Sound when taking damage")]
        public string hurtSFX = "EnemyHurt";

        [Tooltip("Sound when dying")]
        public string deathSFX = "EnemyDeath";

        [Tooltip("Sound when attacking")]
        public string attackSFX = "EnemyAttack";

        [Header("AI Perception")]
        public bool ignoreVisionBlockers = false;

        public float optimalRange = 6f;
        public float retreatRange = 2.5f;
        public float engageRange = 8f;
    }

    public enum EnemyType
    {
        Melee,
        Ranged,
        Air,
        Guardian,
        MeleePatrol
    }
}