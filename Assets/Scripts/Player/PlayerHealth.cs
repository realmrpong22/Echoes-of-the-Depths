using UnityEngine;
using System.Collections;
using Game.Core;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int maxHealth = 5;
        public int currentHealth { get; private set; }

        [SerializeField] private float respawnInvulnDuration = 2f;
        private bool invincible;

        private bool isDead;

        private PlayerController pc;

        void Awake()
        {
            pc = GetComponent<PlayerController>();
            currentHealth = maxHealth;
            isDead = false;
        }

        public void TakeDamage(int dmg)
        {
            //Debug.Log($"TakeDamage called — currentHealth: {currentHealth}");
            if (isDead || invincible) return;

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

        public void Die()
        {
            isDead = true;


            pc.rb.velocity = Vector2.zero;
            pc.enabled = false;
            pc.anim.SetTrigger("Dead");

            //GetComponent<PlayerRespawn>()?.Respawn();
            currentHealth = maxHealth;
        }

        public void OnDeathAnimationFinished()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            ScreenFader fader = FindObjectOfType<ScreenFader>();

            yield return fader.FadeOut();

            PlayerRespawn respawn = GetComponent<PlayerRespawn>();
            respawn?.Respawn();

            currentHealth = maxHealth;

            Animator anim = GetComponent<Animator>();
            anim.ResetTrigger("Death");
            anim.Play("Idle", 0, 0f);

            GetComponent<PlayerController>().enabled = true;

            StartCoroutine(Invincibility());

            yield return fader.FadeIn();

            isDead = false;
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }
    }
}
