using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1f, 2f);
    [SerializeField] private float wallJumpTime = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attackAction;

    private Vector2 moveInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canDoubleJump;
    private bool isDashing;
    private bool canDash = true;
    private bool isInvincible;
    private float wallJumpTimer;
    private bool jumpPressed;
    private bool jumpHeld;

    private int currentHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        attackAction = playerInput.actions["Attack"];
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHealth = currentHealth;
            GameManager.Instance.maxHealth = maxHealth;
        }

        Debug.Log("Player initialized with New Input System");
    }

    void OnEnable()
    {
        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
        dashAction.performed += OnDashPerformed;
        attackAction.performed += OnAttackPerformed;
    }

    void OnDisable()
    {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;
        dashAction.performed -= OnDashPerformed;
        attackAction.performed -= OnAttackPerformed;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        moveInput = moveAction.ReadValue<Vector2>();

        CheckGround();
        CheckWall();

        HandleWallSlide();

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.fixedDeltaTime;
            return;
        }

        if (!isDashing)
        {
            Move();
            ApplyBetterJump();
        }
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
        jumpHeld = true;
        HandleJump();
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpHeld = false;
    }

    void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.HasAbility("Dash") && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack();
    }

    void Move()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDif * accelRate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (moveInput.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void ApplyBetterJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !jumpHeld)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
        }
    }

    void CheckWall()
    {
        isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
    }

    void HandleJump()
    {
        if (isTouchingWall && !isGrounded && GameManager.Instance.HasAbility("WallJump"))
        {
            WallJump();
        }
        else if (isGrounded)
        {
            Jump();
        }
        else if (canDoubleJump && GameManager.Instance.HasAbility("DoubleJump"))
        {
            DoubleJump();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        AudioManager.Instance?.PlaySFX("PlayerJump");
    }

    void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        canDoubleJump = false;
        AudioManager.Instance?.PlaySFX("PlayerDoubleJump");
    }

    void WallJump()
    {
        float jumpDirection = isFacingRight ? -1f : 1f;
        Vector2 force = new Vector2(wallJumpDirection.x * jumpDirection, wallJumpDirection.y) * wallJumpForce;

        rb.velocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        wallJumpTimer = wallJumpTime;

        canDoubleJump = true;

        AudioManager.Instance?.PlaySFX("PlayerWallJump");
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && GameManager.Instance.HasAbility("WallJump"))
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
        rb.gravityScale = 0f;

        AudioManager.Instance?.PlaySFX("PlayerDash");

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = 1f;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        anim.SetTrigger("Attack");

        AudioManager.Instance?.PlaySFX("PlayerAttack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Hit enemy: {enemy.name}");

            /*var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(attackDamage);
            }*/
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHealth = currentHealth;
        }

        UIManager.Instance?.UpdateHealthBar();
        UIManager.Instance?.ShowDamageEffect();

        AudioManager.Instance?.PlaySFX("PlayerHurt");

        StartCoroutine(InvincibilityCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        float flashDuration = 0.1f;
        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashDuration);
            elapsed += flashDuration;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player died");
        AudioManager.Instance?.PlaySFX("PlayerDeath");

        UIManager.Instance?.FadeOut(1f);

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(2f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }

        currentHealth = maxHealth;

        UIManager.Instance?.FadeIn(1f);
    }

    void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("VelocityY", rb.velocity.y);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsWallSliding", isWallSliding);
        anim.SetBool("IsDashing", isDashing);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}