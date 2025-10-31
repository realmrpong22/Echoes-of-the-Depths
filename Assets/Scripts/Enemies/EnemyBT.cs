using UnityEngine;
using BehaviorTree;
using Game.Core;
using Game.Player;
using System.Collections.Generic;

namespace Game.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBT : MonoBehaviour
    {
        [Header("Enemy Data")]
        [Tooltip("ScriptableObject defining this enemy's stats")]
        public EnemyData enemyData;

        [Header("Debug")]
        public bool showPathDebug = false;
        public List<Vector3> currentPath;

        [Header("Detection")]
        [Tooltip("Layer to detect player on")]
        public LayerMask playerLayer;

        [Header("Ground Check (for Melee/Guardian)")]
        [Tooltip("Point to check if ground ahead")]
        public Transform groundCheck;

        [Tooltip("Layer for ground detection")]
        public LayerMask groundLayer;

        [Header("Visual Feedback")]
        [Tooltip("Sprite renderer to flash on damage")]
        public SpriteRenderer spriteRenderer;

        [Tooltip("Color to flash when damaged")]
        public Color damageFlashColor = Color.red;

        private Rigidbody2D rb;
        private Animator anim;
        private Collider2D col;

        private Node rootNode;
        private Blackboard blackboard;

        private Transform player;
        private bool isFacingRight = true;
        private EnemyPerception perception;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            col = GetComponent<Collider2D>();

            if (enemyData == null)
            {
                Debug.LogError($"EnemyData not assigned on {gameObject.name}!");
                enabled = false;
                return;
            }

            blackboard = new Blackboard();
            InitializeBlackboard();
            perception = new EnemyPerception(transform, enemyData, groundLayer);

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                blackboard.SetValue(BlackboardKeys.Player, player);
            }

            rootNode = BuildBehaviorTree();
            rootNode.Initialize(transform, rb, blackboard);
        }

        void Update()
        {
            if (rootNode == null) return;

            // Update blackboard data each frame
            UpdateBlackboard();

            // Evaluate behavior tree
            rootNode.Evaluate();

            // Update animator
            UpdateAnimator();
        }

        public Rigidbody2D GetRigidbody() => rb;
        public Animator GetAnimator() => anim;
        public LayerMask GetGroundLayer() => groundLayer;

        void InitializeBlackboard()
        {
            // Initialize state
            blackboard.SetValue(BlackboardKeys.IsDead, false);
            blackboard.SetValue(BlackboardKeys.ReturningToPatrol, false);
            currentHealth = enemyData.maxHealth;

            // Initialize patrol
            blackboard.SetValue(BlackboardKeys.PatrolStartPosition, transform.position);
            blackboard.SetValue(BlackboardKeys.PatrolMovingRight, true);
            blackboard.SetValue(BlackboardKeys.PatrolWaitTimer, 0f);

            // Initialize combat
            blackboard.SetValue(BlackboardKeys.AttackTimer, 0f);
            blackboard.SetValue(BlackboardKeys.IsAttacking, false);
        }

        void UpdateBlackboard()
        {
            if (player != null && perception != null)
                perception.UpdatePerception(blackboard, player);

            // existing cooldowns & patrol timers
            float attackTimer = blackboard.GetValue<float>(BlackboardKeys.AttackTimer);
            if (attackTimer > 0f)
                blackboard.SetValue(BlackboardKeys.AttackTimer, attackTimer - Time.deltaTime);

            float patrolWaitTimer = blackboard.GetValue<float>(BlackboardKeys.PatrolWaitTimer);
            if (patrolWaitTimer > 0f)
                blackboard.SetValue(BlackboardKeys.PatrolWaitTimer, patrolWaitTimer - Time.deltaTime);
        }


        Node BuildBehaviorTree()
        {
            switch (enemyData.enemyType)
            {
                case EnemyType.Melee:
                    return EnemyTreeBuilder.BuildMeleeTree(this, enemyData);

                case EnemyType.Ranged:
                    return EnemyTreeBuilder.BuildRangedTree(this, enemyData);

                case EnemyType.Guardian:
                    return EnemyTreeBuilder.BuildGuardianTree(this, enemyData);

                case EnemyType.Air:
                    return EnemyTreeBuilder.BuildAirTree(this, enemyData);

                default:
                    Debug.LogError($"Unknown enemy type: {enemyData.enemyType}");
                    return EnemyTreeBuilder.BuildMeleeTree(this, enemyData);
            }
        }

        void UpdateAnimator()
        {
            if (anim == null) return;

            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            anim.SetBool("IsAttacking", blackboard.GetValue<bool>(BlackboardKeys.IsAttacking));
        }

        #region Public Methods for Behavior Tree Actions

        public void Move(float direction)
        {
            rb.velocity = new Vector2(direction * enemyData.moveSpeed, rb.velocity.y);

            // Face the direction we're moving
            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            {
                Flip();
            }
        }

        public void Stop()
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        public void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        public void FacePlayer()
        {
            if (player == null) return;

            float direction = player.position.x > transform.position.x ? 1f : -1f;
            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            {
                Flip();
            }
        }

        public bool IsGroundAhead()
        {
            if (groundCheck == null) return true;

            float checkDistance = 0.8f;
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);
            return hit.collider != null;
        }

        public void PerformMeleeAttack()
        {
            if (player == null) return;

            blackboard.SetValue(BlackboardKeys.IsAttacking, true);

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            AudioManager.Instance?.PlaySFX(enemyData.attackSFX);

            // Check if player is in range and deal damage
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= enemyData.attackRange)
            {
                var target = player.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(enemyData.attackDamage);
            }

            blackboard.SetValue(BlackboardKeys.AttackTimer, enemyData.fireRate);

            // Reset attacking flag after short delay
            Invoke(nameof(ResetAttacking), 0.5f);
        }

        public void ShootProjectile()
        {
            if (enemyData.projectilePrefab == null || player == null) return;

            blackboard.SetValue(BlackboardKeys.IsAttacking, true);

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            AudioManager.Instance?.PlaySFX(enemyData.attackSFX);

            Vector2 direction = (player.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
            GameObject projectile = Instantiate(enemyData.projectilePrefab, spawnPos, Quaternion.identity);

            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.velocity = direction * enemyData.projectileSpeed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            blackboard.SetValue(BlackboardKeys.AttackTimer, enemyData.attackCooldown);

            Invoke(nameof(ResetAttacking), 0.5f);
        }

        private bool isAiming = false;

        public void AimAndShoot(float delay)
        {
            if (isAiming) return;
            StartCoroutine(AimAndShootCoroutine(delay));
        }

        private System.Collections.IEnumerator AimAndShootCoroutine(float delay)
        {
            isAiming = true;
            blackboard.SetValue(BlackboardKeys.IsAttacking, true);

            // Optional: play aim animation or change color
            if (anim != null)
                anim.SetTrigger("Aim");

            // Wait before shooting
            yield return new WaitForSeconds(delay);

            // Actually fire
            ShootProjectile();

            blackboard.SetValue(BlackboardKeys.AttackTimer, enemyData.fireRate);
            blackboard.SetValue(BlackboardKeys.IsAttacking, false);
            isAiming = false;
        }


        void ResetAttacking()
        {
            blackboard.SetValue(BlackboardKeys.IsAttacking, false);
        }
        #endregion

        #region Damage & Death (Unified)

        [SerializeField] private int maxHealth = 3;
        private int currentHealth;
        private bool isDead;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            AudioManager.Instance?.PlaySFX(enemyData.hurtSFX);
            StartCoroutine(FlashOnHit());

            if (currentHealth <= 0)
            {
                blackboard?.SetValue(BlackboardKeys.IsDead, true);
                Die();
            }
            else
            {
                anim?.SetTrigger("Hurt");
            }
        }

        private System.Collections.IEnumerator FlashOnHit()
        {
            if (spriteRenderer == null) yield break;

            Color original = spriteRenderer.color;
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            anim?.SetTrigger("Die");
            AudioManager.Instance?.PlaySFX(enemyData.deathSFX);
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            col.enabled = false;

            // Blackboard update for Behavior Tree awareness
            blackboard?.SetValue(BlackboardKeys.IsDead, true);

            // Drop loot
            if (enemyData.dropItem != null && Random.value <= enemyData.dropChance)
            {
                Instantiate(enemyData.dropItem, transform.position, Quaternion.identity);
            }

            // Disable BT processing
            rootNode = null;
            enabled = false;

            Destroy(gameObject, 1.5f);
        }

        #endregion

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showPathDebug || currentPath == null || currentPath.Count < 2)
                return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawSphere(currentPath[i], 0.1f);
            }

            Gizmos.DrawSphere(currentPath[^1], 0.12f); // endpoint
        }
#endif
    }
}