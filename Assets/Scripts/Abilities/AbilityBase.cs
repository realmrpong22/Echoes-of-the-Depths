using UnityEngine;
using Game.Player;

public abstract class AbilityBase : MonoBehaviour
{
    protected PlayerInputHandler input;
    protected PlayerMovement movement;
    protected PlayerController controller;

    protected virtual void Awake()
    {
        input = GetComponentInParent<PlayerInputHandler>();
        movement = GetComponentInParent<PlayerMovement>();
        controller = GetComponentInParent<PlayerController>();
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
}
