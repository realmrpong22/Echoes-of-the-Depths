using Game.Player;
using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField] private int healAmount = 1;

    protected override void Apply(GameObject player)
    {
        player.GetComponent<PlayerHealth>()
              ?.Heal(healAmount);
    }
}