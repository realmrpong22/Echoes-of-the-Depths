using UnityEngine;
using Game.Core;
using Game.AI;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        public int attackDamage = 1;
        public float attackRange = 1f;
        public float attackCooldown = 0.3f;
        public LayerMask enemyLayer;

        private PlayerController pc;
        private float attackTimer;

        void Awake() => pc = GetComponent<PlayerController>();

        public void HandleInput()
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                return;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                PerformAttack();
                attackTimer = attackCooldown;
            }
        }

        private void PerformAttack()
        {
            pc.anim.SetTrigger("Attack");
            AudioManager.Instance?.PlaySFX("PlayerAttack");

            Vector2 attackPos = (Vector2)transform.position + Vector2.right * (pc.sprite.flipX ? -1 : 1) * attackRange * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

            foreach (var hit in hits)
            {
                var enemyHealth = hit.GetComponent<EnemyBT>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (pc == null) return;
            Vector2 attackPos = (Vector2)transform.position + Vector2.right * (pc.sprite.flipX ? -1 : 1) * attackRange * 0.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPos, attackRange);
        }
    }
}
