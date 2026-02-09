using System.Collections;
using UnityEngine;

public class DashAbility : AbilityBase
{
    [Header("Dash")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.4f;

    public bool isActive { get; private set; }
    public Vector2 Velocity { get; private set; }

    bool canDash = true;
    bool dashedInAir;

    public bool CanDash()
    {
        if (!(canDash && isActive))
            return false;

        if (!movement.IsGrounded() && dashedInAir)
            return false;

        return true;
    }

    public void StartDash(float facingDir)
    {
        if (!CanDash())
            return;

        StartCoroutine(DashRoutine(facingDir));
    }

    IEnumerator DashRoutine(float dir)
    {
        canDash = false;
        isActive = true;

        if (!movement.IsGrounded())
            dashedInAir = true;

        Velocity = new Vector2(dir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        isActive = false;
        Velocity = Vector2.zero;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Update()
    {
        if (movement.IsGrounded())
            dashedInAir = false;
    }

    public void Unlock()
    {
        isActive = true;
    }
}
