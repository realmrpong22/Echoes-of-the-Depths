using UnityEngine;
using BehaviorTree;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBT : MonoBehaviour
{
    [Header("Enemy Data")]
    [Tooltip("ScriptableObject defining this enemy's stats")]
    public EnemyData enemyData;

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

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    // Behavior Tree
    private Node rootNode;
    private Blackboard blackboard;

    // Cached references
    private Transform player;
    private bool isFacingRight = true;

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

        // Initialize blackboard
        blackboard = new Blackboard();
        InitializeBlackboard();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            blackboard.SetValue(BlackboardKeys.Player, player);
        }

        // Build behavior tree based on enemy type
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

    void InitializeBlackboard()
    {
        // Initialize state
        blackboard.SetValue(BlackboardKeys.IsDead, false);
        blackboard.SetValue(BlackboardKeys.CurrentHealth, enemyData.maxHealth);

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
        // Update player position and distance
        if (player != null)
        {
            blackboard.SetValue(BlackboardKeys.PlayerPosition, player.position);
            float distance = Vector2.Distance(transform.position, player.position);
            blackboard.SetValue(BlackboardKeys.PlayerDistance, distance);
        }

        // Update timers
        float attackTimer = blackboard.GetValue<float>(BlackboardKeys.AttackTimer);
        if (attackTimer > 0f)
        {
            blackboard.SetValue(BlackboardKeys.AttackTimer, attackTimer - Time.deltaTime);
        }

        float patrolWaitTimer = blackboard.GetValue<float>(BlackboardKeys.PatrolWaitTimer);
        if (patrolWaitTimer > 0f)
        {
            blackboard.SetValue(BlackboardKeys.PatrolWaitTimer, patrolWaitTimer - Time.deltaTime);
        }
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
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(enemyData.attackDamage);
            }
        }

        blackboard.SetValue(BlackboardKeys.AttackTimer, enemyData.attackCooldown);

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
        GameObject projectile = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);

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

    void ResetAttacking()
    {
        blackboard.SetValue(BlackboardKeys.IsAttacking, false);
    }

    public void Die()
    {
        blackboard.SetValue(BlackboardKeys.IsDead, true);

        AudioManager.Instance?.PlaySFX(enemyData.deathSFX);

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        if (col != null)
        {
            col.enabled = false;
        }

        // Drop loot
        if (enemyData.dropItem != null && Random.value <= enemyData.dropChance)
        {
            Instantiate(enemyData.dropItem, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }

    #endregion

    #region Damage System

    public void TakeDamage(int damage)
    {
        if (blackboard.GetValue<bool>(BlackboardKeys.IsDead)) return;

        int currentHealth = blackboard.GetValue<int>(BlackboardKeys.CurrentHealth);
        currentHealth -= damage;
        blackboard.SetValue(BlackboardKeys.CurrentHealth, currentHealth);

        AudioManager.Instance?.PlaySFX(enemyData.hurtSFX);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            // Die action will be handled by behavior tree
            blackboard.SetValue(BlackboardKeys.IsDead, true);
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageFlashColor;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        // Patrol range
        if (enemyData.enemyType == EnemyType.Melee || enemyData.enemyType == EnemyType.Guardian)
        {
            Gizmos.color = Color.blue;
            Vector3 startPos = Application.isPlaying
                ? blackboard.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition)
                : transform.position;

            Gizmos.DrawLine(startPos + Vector3.left * enemyData.patrolDistance,
                           startPos + Vector3.right * enemyData.patrolDistance);

            Gizmos.DrawWireSphere(startPos + Vector3.left * enemyData.patrolDistance, 0.3f);
            Gizmos.DrawWireSphere(startPos + Vector3.right * enemyData.patrolDistance, 0.3f);
        }

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.8f);
        }
    }

    #endregion
}