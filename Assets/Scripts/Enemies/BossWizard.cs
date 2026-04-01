using System.Collections;
using UnityEngine;
using Game.Player;

public class BossWizard : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] teleportPoints;
    [SerializeField] private Collider2D hitboxCollider;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool phase2 = false;
    [SerializeField] private bool isDead = false;

    private int currentPointIndex = -1;

    private void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        StartCoroutine(BossLoop());
    }

    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            Transform nextPoint = GetRandomPoint();

            yield return MoveToWaypoint(nextPoint);

            FacePlayer();

            yield return new WaitForSeconds(0.5f);

            yield return Attack();

            if (phase2)
            {
                yield return new WaitForSeconds(0.3f);
                yield return Attack();
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    private IEnumerator MoveToWaypoint(Transform targetPoint)
    {
        hitboxCollider.enabled = false;

        animator.SetTrigger("Hide");
        yield return new WaitForSeconds(0.4f);

        animator.SetBool("IsRunning", true);

        while (Vector2.Distance(transform.position, targetPoint.position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        animator.SetBool("IsRunning", false);

        animator.SetTrigger("Emerge");
        yield return new WaitForSeconds(0.4f);

        hitboxCollider.enabled = true;
    }

    private IEnumerator Attack()
    {
        if (IsPlayerOnRight())
        {
            animator.SetTrigger("AttackRight");
        }
        else
        {
            animator.SetTrigger("AttackLeft");
        }

        yield return new WaitForSeconds(0.35f);

        FireProjectile();

        yield return new WaitForSeconds(0.6f);
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
            return;

        GameObject projectileObj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Vector2 direction = (player.position - firePoint.position).normalized;

        BossProjectile projectile = projectileObj.GetComponent<BossProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction, projectileSpeed);
        }
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    private Transform GetRandomPoint()
    {
        if (teleportPoints.Length == 0)
            return transform;

        int newIndex = currentPointIndex;

        while (newIndex == currentPointIndex)
        {
            newIndex = Random.Range(0, teleportPoints.Length);
        }

        currentPointIndex = newIndex;
        return teleportPoints[currentPointIndex];
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (IsPlayerOnRight())
        {
            animator.SetTrigger("HurtRight");
        }
        else
        {
            animator.SetTrigger("HurtLeft");
        }

        if (currentHealth <= maxHealth / 2)
        {
            phase2 = true;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        StopAllCoroutines();

        animator.SetTrigger("Die");

        // unlock doors here
        // spawn reward here
        // stop boss music here

        Destroy(gameObject, 2f);
    }

    private bool IsPlayerOnRight()
    {
        if (player == null)
            return true;

        return player.position.x > transform.position.x;
    }
}