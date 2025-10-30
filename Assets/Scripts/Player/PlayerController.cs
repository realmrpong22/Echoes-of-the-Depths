using UnityEngine;
using Game.Core; // For GameManager, UIManager, AudioManager

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector] public Rigidbody2D rb;
        [HideInInspector] public Animator anim;
        [HideInInspector] public SpriteRenderer sprite;

        public PlayerMovement Movement { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerAbilities Abilities { get; private set; }
        public PlayerHealth Health { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();

            Movement = GetComponent<PlayerMovement>();
            Combat = GetComponent<PlayerCombat>();
            Abilities = GetComponent<PlayerAbilities>();
            Health = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            Movement.HandleInput();
            Abilities.HandleInput();
            Combat.HandleInput();
        }

        void FixedUpdate()
        {
            Movement.ApplyMovement();
        }
    }
}
