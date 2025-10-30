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
            var target = other.GetComponentInParent<Game.Player.IDamageable>();
            if (target == null)
            {
                Debug.Log("No IDamageable found in parent chain.");
            }
            else
            {
                Debug.Log($"Found IDamageable on: {target}");
                target.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Hit wall or ground
            Destroy(gameObject);
        }
    }
}
