using UnityEngine;
using Game.Core;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int maxHealth = 5;
        public int currentHealth { get; private set; }
        private bool invincible;

        private PlayerController pc;

        void Awake()
        {
            pc = GetComponent<PlayerController>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(int dmg)
        {
            //Debug.Log($"TakeDamage called — currentHealth: {currentHealth}");
            if (invincible) return;

            currentHealth -= dmg;
            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            //Debug.Log($"Player took {dmg} damage!");

            StartCoroutine(Invincibility());
            pc.anim.SetTrigger("Hurt");
            AudioManager.Instance?.PlaySFX("PlayerHurt");
            UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);
        }

        private System.Collections.IEnumerator Invincibility()
        {
            invincible = true;
            for (int i = 0; i < 5; i++)
            {
                pc.sprite.enabled = false;
                yield return new WaitForSeconds(0.1f);
                pc.sprite.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
            invincible = false;
        }

        private void Die()
        {
            pc.anim.SetTrigger("Die");
            pc.rb.velocity = Vector2.zero;
            GameManager.Instance.RespawnPlayer();
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }
    }
}
