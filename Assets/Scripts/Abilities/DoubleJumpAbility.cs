using UnityEngine;

public class DoubleJumpAbility : AbilityBase
{
    [SerializeField] private float doubleJumpForce = 12f;

    private bool hasDoubleJumped;

    public bool isActive { get; private set; }

    public bool CanDoubleJump()
    {
        // Only when airborne and not used yet
        return isActive && !movement.IsGrounded() && !hasDoubleJumped;
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

    public void Unlock()
    {
        isActive = true;
    }
}
