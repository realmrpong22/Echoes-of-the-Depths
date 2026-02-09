using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public DashAbility Dash { get; private set; }
    public DoubleJumpAbility DoubleJump { get; private set; }

    private void Awake()
    {
        Dash = GetComponent<DashAbility>();
        DoubleJump = GetComponent<DoubleJumpAbility>();
    }

    public void UnlockAbility(AbilityTypes type)
    {
        switch (type)
        {
            case AbilityTypes.Dash:
                Dash?.Unlock();
                break;

            case AbilityTypes.DoubleJump:
                DoubleJump?.Unlock();
                break;
        }
    }
}
