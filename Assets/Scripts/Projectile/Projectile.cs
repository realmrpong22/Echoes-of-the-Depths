using UnityEngine;
using Game.Player;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 10;

    void Start()
    {
        Debug.Log($"Projectile spawned at {Time.time}");
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Projectile collided with: {other.name}");
            // Deal damage to player
            var target = other.GetComponent<IDamageable>();
            if (target != null)
                target.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Hit wall or ground
            Destroy(gameObject);
        }
    }
}
