using System.Collections;
using UnityEngine;

public class DashAbility : AbilityBase
{
    [Header("Dash")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.4f;

    public bool isActive { get; private set; }
    public bool isUnlocked { get; private set; }
    public Vector2 Velocity { get; private set; }

    bool canDash = true;
    bool dashedInAir;

    public bool CanDash()
    {
        if (!isUnlocked) return false;
        if (!canDash) return false;
        if (isActive) return false;

        if (!movement.IsGrounded() && dashedInAir) return false;

        return true;
    }

    public void StartDash(float facingDir)
    {
        if (!CanDash())
            return;

        float dashDir;

        float inputX = input.Move.x;

        if (Mathf.Abs(inputX) > 0.1f)
            dashDir = Mathf.Sign(inputX);
        else
            dashDir = -facingDir;

        StartCoroutine(DashRoutine(dashDir));
    }

    IEnumerator DashRoutine(float dir)
    {
        canDash = false;
        isActive = true;

        if (!movement.IsGrounded())
            dashedInAir = true;

        float timer = 0f;

        while (timer < dashDuration)
        {
            float t = timer / dashDuration;

            // Ease out (fast → slow)
            float currentSpeed = Mathf.Lerp(dashSpeed, 0f, t);

            Velocity = new Vector2(dir * currentSpeed, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

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
        isUnlocked = true;
    }
}
