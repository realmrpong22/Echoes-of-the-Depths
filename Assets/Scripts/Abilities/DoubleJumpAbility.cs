using UnityEngine;

public class DoubleJumpAbility : AbilityBase
{
    [SerializeField] private float doubleJumpForce = 12f;

    private bool hasDoubleJumped;

    public bool CanDoubleJump()
    {
        // Only when airborne and not used yet
        return !movement.IsGrounded() && !hasDoubleJumped;
    }

    public float ConsumeDoubleJump()
    {
        hasDoubleJumped = true;
        return doubleJumpForce;
    }

    private void Update()
    {
        // Reset when touching ground
        if (movement.IsGrounded())
        {
            hasDoubleJumped = false;
        }
    }
}
