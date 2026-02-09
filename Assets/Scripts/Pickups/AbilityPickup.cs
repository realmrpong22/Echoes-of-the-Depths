using UnityEngine;

public class AbilityPickup : Pickup
{
    [SerializeField] private AbilityType abilityType;

    protected override void Apply(GameObject player)
    {
        player.GetComponent<PlayerAbilities>()
              ?.UnlockAbility(abilityType);
    }
}