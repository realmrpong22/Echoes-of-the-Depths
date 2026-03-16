using Game.Core;
using Game.Player;
using UnityEngine;

public class AbilityPickup : Pickup
{
    [SerializeField] private AbilityTypes abilityType;
    [SerializeField] private Animator animator;

    private GameObject player;

    private bool waitingConfirm;

    protected override void OnPlayerTriggered(GameObject player)
    {
        this.player = player;

        GetComponent<Collider2D>().enabled = false;

        animator.Play("ChestOpening");
    }

    protected override void Apply(GameObject player)
    {
        player.GetComponent<PlayerAbilities>()
              ?.UnlockAbility(abilityType);
    }

    public void OnChestOpened()
    {
        Apply(player);

        UIManager.Instance.ShowMessage("You got " + abilityType.ToString() + "!", 10f);

        animator.Play("ChestDeleting");
    }

    public void OnChestDeleted()
    {
        Destroy(gameObject);
    }
}