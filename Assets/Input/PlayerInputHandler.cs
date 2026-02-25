using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset actions;

    public Vector2 Move { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool DiePressed { get; private set; }

    public event System.Action OnDash;

    private InputAction move;
    private InputAction jump;
    private InputAction attack;
    private InputAction dash;
    private InputAction interact;
    private InputAction die;

    void Awake()
    {
        var player = actions.FindActionMap("Player");

        move = player.FindAction("Move");
        jump = player.FindAction("Jump");
        attack = player.FindAction("Attack");
        dash = player.FindAction("Dash");
        interact = player.FindAction("Interact");
        die = player.FindAction("Die");
    }

    void OnEnable()
    {
        move.Enable();
        jump.Enable();
        attack.Enable();
        dash.Enable();
        interact.Enable();
        die.Enable();
    }

    void OnDisable()
    {
        move.Disable();
        jump.Disable();
        attack.Disable();
        dash.Disable();
        interact.Disable();
        die.Disable();
    }

    void Update()
    {
        Move = move.ReadValue<Vector2>();

        JumpPressed = jump.WasPressedThisFrame();
        JumpReleased = jump.WasReleasedThisFrame();

        AttackPressed = attack.WasPressedThisFrame();

        DashPressed = dash.WasPressedThisFrame();

        InteractPressed = interact.WasPressedThisFrame();

        DiePressed = die.WasPressedThisFrame();
    }
}
