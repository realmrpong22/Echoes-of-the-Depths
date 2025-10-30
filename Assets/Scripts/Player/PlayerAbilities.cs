using UnityEngine;
using Game.Core;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAbilities : MonoBehaviour
    {
        [Header("Dash Settings")]
        public float dashForce = 15f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;

        private PlayerController pc;
        private bool isDashing;
        private float dashTimer, cooldownTimer;

        void Awake() => pc = GetComponent<PlayerController>();

        public void HandleInput()
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

            if (Input.GetButtonDown("Fire2") && cooldownTimer <= 0 && !isDashing)
                StartCoroutine(Dash());
        }

        private System.Collections.IEnumerator Dash()
        {
            isDashing = true;
            cooldownTimer = dashCooldown;

            float dir = pc.sprite.flipX ? -1f : 1f;
            pc.anim.SetTrigger("Dash");
            AudioManager.Instance?.PlaySFX("Dash");

            float elapsed = 0;
            while (elapsed < dashDuration)
            {
                pc.rb.velocity = new Vector2(dir * dashForce, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            isDashing = false;
        }

        public void UpdateAbilities() { }
    }
}
