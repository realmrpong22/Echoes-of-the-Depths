using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
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

    // State
    private EnemyState currentState = EnemyState.Idle;
    private int currentHealth;
    private Transform player;
    private bool isFacingRight = true;
    private float attackTimer = 0f;

    // Patrol variables
    private Vector3 patrolStartPosition;
    private bool patrolMovingRight = true;
    private float patrolWaitTimer = 0f;

    // Combat
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (enemyData == null)
        {
            Debug.LogError($"EnemyData not assigned on {gameObject.name}!");
            enabled = false;
            return;
        }

        // Initialize
        currentHealth = enemyData.maxHealth;
        patrolStartPosition = transform.position;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Start in appropriate state
        if (enemyData.enemyType == EnemyType.Ranged)
        {
            currentState = EnemyState.Idle;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

    void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;

        // State machine
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;
            case EnemyState.Patrol:
                PatrolState();
                break;
            case EnemyState.Chase:
                ChaseState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }

        UpdateAnimator();
    }

    void IdleState()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        // Check for player
        if (PlayerInRange(enemyData.detectionRange))
        {
            currentState = EnemyState.Attack;
        }
    }

    void PatrolState()
    {
        // Check for player first
        if (PlayerInRange(enemyData.detectionRange))
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Wait at patrol point
        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // Patrol movement
        float direction = patrolMovingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * enemyData.moveSpeed, rb.velocity.y);

        // Face the direction we're moving
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }

        // Check if reached patrol limit (with small buffer to prevent jitter)
        float distanceFromStart = transform.position.x - patrolStartPosition.x;

        if (patrolMovingRight && distanceFromStart >= enemyData.patrolDistance)
        {
            // Reached right limit
            patrolMovingRight = false;
            patrolWaitTimer = enemyData.patrolWaitTime;
        }
        else if (!patrolMovingRight && distanceFromStart <= -enemyData.patrolDistance)
        {
            // Reached left limit
            patrolMovingRight = true;
            patrolWaitTimer = enemyData.patrolWaitTime;
        }

        // Check for edge/wall (optional, requires groundCheck)
        if (groundCheck != null && !IsGroundAhead() && patrolWaitTimer <= 0f)
        {
            patrolMovingRight = !patrolMovingRight;
            patrolWaitTimer = enemyData.patrolWaitTime;
        }
    }

    void ChaseState()
    {
        if (player == null) return;

        // Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= enemyData.attackRange)
        {
            currentState = EnemyState.Attack;
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // Check if player escaped detection range
        if (distanceToPlayer > enemyData.detectionRange * 1.5f)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Move towards player
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(direction * enemyData.moveSpeed, rb.velocity.y);

        // Face player
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }
    }

    void AttackState()
    {
        if (player == null) return;

        rb.velocity = new Vector2(0, rb.velocity.y);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Player escaped
        if (distanceToPlayer > enemyData.detectionRange)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Player moved away
        if (distanceToPlayer > enemyData.attackRange * 1.5f)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Face player
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }

        // Attack if cooldown ready
        if (attackTimer <= 0f && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        AudioManager.Instance?.PlaySFX(enemyData.attackSFX);

        // Wait for attack animation (adjust timing as needed)
        yield return new WaitForSeconds(0.3f);

        // Deal damage if player still in range
        if (enemyData.enemyType == EnemyType.Melee || enemyData.enemyType == EnemyType.Guardian)
        {
            if (PlayerInRange(enemyData.attackRange))
            {
                DamagePlayer();
            }
        }
        else if (enemyData.enemyType == EnemyType.Ranged)
        {
            ShootProjectile();
        }

        attackTimer = enemyData.attackCooldown;
        isAttacking = false;
    }

    void DamagePlayer()
    {
        if (player == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(enemyData.attackDamage);
        }
    }

    void ShootProjectile()
    {
        if (enemyData.projectilePrefab == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GameObject projectile = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = direction * enemyData.projectileSpeed;
        }

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        AudioManager.Instance?.PlaySFX(enemyData.hurtSFX);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Switch to chase if taking damage during patrol
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Idle)
            {
                currentState = EnemyState.Chase;
            }
        }
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageFlashColor;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        currentState = EnemyState.Idle;

        AudioManager.Instance?.PlaySFX(enemyData.deathSFX);

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        GetComponent<Collider2D>().enabled = false;

        // Drop loot
        if (enemyData.dropItem != null && Random.value <= enemyData.dropChance)
        {
            Instantiate(enemyData.dropItem, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }

    bool PlayerInRange(float range)
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= range;
    }

    bool IsGroundAhead()
    {
        if (groundCheck == null) return true;

        float checkDistance = 0.8f;
        float direction = isFacingRight ? 1f : -1f;

        // Cast ray from groundCheck position in the direction enemy is facing
        Vector2 rayOrigin = groundCheck.position;
        Vector2 rayDirection = new Vector2(direction, -1f).normalized;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDistance, groundLayer);

        return hit.collider != null;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimator()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("IsAttacking", isAttacking);
    }

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
            Vector3 startPos = Application.isPlaying ? patrolStartPosition : transform.position;
            Gizmos.DrawLine(startPos + Vector3.left * enemyData.patrolDistance,
                           startPos + Vector3.right * enemyData.patrolDistance);

            // Draw patrol limits
            Gizmos.DrawWireSphere(startPos + Vector3.left * enemyData.patrolDistance, 0.3f);
            Gizmos.DrawWireSphere(startPos + Vector3.right * enemyData.patrolDistance, 0.3f);
        }

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.8f);

            // Draw ground check detection
            if (Application.isPlaying)
            {
                Gizmos.color = IsGroundAhead() ? Color.green : Color.red;
                Gizmos.DrawSphere(groundCheck.position + Vector3.down * 0.8f, 0.1f);
            }
        }
    }
}

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}