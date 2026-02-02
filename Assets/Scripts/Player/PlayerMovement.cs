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
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool isJumping;
        private float inputX;
        private PlayerInputHandler input;


        void Awake() 
        {
            pc = GetComponent<PlayerController>();
            input = GetComponent<PlayerInputHandler>();
        } 

        public void HandleInput()
        {
            inputX = input.Move.x;

            if (input.JumpPressed)
                jumpBufferCounter = jumpBufferTime;

            if (input.JumpReleased && pc.rb.velocity.y > 0)
               pc.rb.velocity = new Vector2(pc.rb.velocity.x, pc.rb.velocity.y * 0.5f);
        }

        public void ApplyMovement()
        {
            pc.rb.velocity = new Vector2(inputX * moveSpeed, pc.rb.velocity.y);

            if (inputX != 0)
                pc.sprite.flipX = inputX > 0;

            if (IsGrounded())
                coyoteCounter = coyoteTime;
            else
                coyoteCounter -= Time.fixedDeltaTime;

            if (jumpBufferCounter > 0)
                jumpBufferCounter -= Time.fixedDeltaTime;

            if (jumpBufferCounter > 0 && coyoteCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0;
            }

            pc.anim.SetBool("isGrounded", IsGrounded());
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

        public bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
        }
    }
}
