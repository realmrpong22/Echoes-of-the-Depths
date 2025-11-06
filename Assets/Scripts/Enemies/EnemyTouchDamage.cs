using Game.Player;
using System.Xml.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyTouchDamage : MonoBehaviour
{
    [Header("Touch Damage Settings")]
    public int damage = 1;
    public float knockbackForce = 5f;
    public float hitCooldown = 1f;

    private float cooldownTimer;

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.collider);

        if (!collision.collider.CompareTag("Player"))
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector2 normal = collision.contacts[0].normal;
            rb.AddForce(normal * 3f, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision.collider);
    }

    void TryDealDamage(Collider2D other)
    {
        if (cooldownTimer > 0f) return;
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            cooldownTimer = hitCooldown;
            Debug.Log($"[{name}] Touch damage applied ({damage})");

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
