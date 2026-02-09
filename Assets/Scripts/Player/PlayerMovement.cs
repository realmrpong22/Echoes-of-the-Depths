using UnityEngine;
using Game.Core;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float jumpForce = 16f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.1f;

        [Header("Checks")]
        public Transform groundCheck;
        public LayerMask groundLayer;

        private PlayerController pc;
        private PlayerAbilities abilities;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private float inputX;
        private PlayerInputHandler input;

        public float facingDirection { get; private set; } = 1f;

        void Awake() 
        {
            pc = GetComponent<PlayerController>();
            abilities = GetComponent<PlayerAbilities>();
            input = GetComponent<PlayerInputHandler>();
        } 

        public void HandleInput()
        {
            inputX = input.Move.x;

            if (inputX != 0)
                facingDirection = Mathf.Sign(input.Move.x);

            if (input.JumpPressed)
                jumpBufferCounter = jumpBufferTime;

            if (input.JumpReleased && pc.rb.velocity.y > 0 && coyoteCounter > 0)
               pc.rb.velocity = new Vector2(pc.rb.velocity.x, pc.rb.velocity.y * 0.5f);

            if (input.DashPressed && abilities.Dash.CanDash())
            {
                float dir = input.Move.x != 0
                    ? Mathf.Sign(input.Move.x)
                    : facingDirection;

                abilities.Dash.StartDash(dir);
            }
        }

        public void ApplyMovement()
        {
            bool grounded = IsGrounded();

            if (abilities.Dash.isActive)
            {
                pc.rb.velocity = abilities.Dash.Velocity;
                return;
            }

            pc.rb.velocity = new Vector2(inputX * moveSpeed, pc.rb.velocity.y);

            if (inputX != 0)
                pc.sprite.flipX = inputX > 0;

            if (grounded)
                coyoteCounter = coyoteTime;
            else
                coyoteCounter -= Time.fixedDeltaTime;

            if (jumpBufferCounter > 0)
                jumpBufferCounter -= Time.fixedDeltaTime;

            if (jumpBufferCounter > 0)
            {
                // Normal jump (ground / coyote)
                if (coyoteCounter > 0)
                {
                    Jump();
                    jumpBufferCounter = 0;
                }
                // Double jump (airborne)
                else if (abilities.DoubleJump != null && abilities.DoubleJump.CanDoubleJump())
                {
                    DoubleJump();
                    jumpBufferCounter = 0;
                }
            }


            pc.anim.SetBool("isGrounded", grounded);
            pc.anim.SetBool("isRunning", Mathf.Abs(inputX) > 0.1f);
            pc.anim.SetBool("isFalling", pc.rb.velocity.y < -0.1f);
            pc.anim.SetFloat("verticalSpeed", pc.rb.velocity.y);
        }

        private void Jump()
        {
            pc.rb.velocity = new Vector2(pc.rb.velocity.x, jumpForce);
            pc.anim.SetTrigger("Jump");
            AudioManager.Instance?.PlaySFX("Jump");
        }

        private void DoubleJump()
        {
            float force = abilities.DoubleJump.ConsumeDoubleJump();
            pc.rb.velocity = new Vector2(pc.rb.velocity.x, 0f);
            pc.rb.velocity += Vector2.up * force;

            pc.anim.SetTrigger("Jump");
            AudioManager.Instance?.PlaySFX("Jump");
        }

        public bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
        }
    }
}
