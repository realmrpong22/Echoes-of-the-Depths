using UnityEngine;
using Game.Core;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int maxHealth = 5;
        private int currentHealth;
        private bool invincible;

        private PlayerController pc;

        void Awake()
        {
            pc = GetComponent<PlayerController>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(int dmg)
        {
            if (invincible) return;

            currentHealth -= dmg;
            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            StartCoroutine(Invincibility());
            pc.anim.SetTrigger("Hurt");
            AudioManager.Instance?.PlaySFX("PlayerHurt");
            UIManager.Instance?.UpdateHealthBar();
        }

        private System.Collections.IEnumerator Invincibility()
        {
            invincible = true;
            yield return new WaitForSeconds(1f);
            invincible = false;
        }

        private void Die()
        {
            pc.anim.SetTrigger("Die");
            pc.rb.velocity = Vector2.zero;
            GameManager.Instance.RespawnPlayer();
        }
    }
}
