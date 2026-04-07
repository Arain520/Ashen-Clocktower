using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Fall,
        Attack,
        Hurt,
        Dead
    }

    [Header("组件引用")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Rigidbody2D rb;

    [Header("当前状态（调试用）")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    public PlayerState CurrentState => currentState;

    public bool IsDead => currentState == PlayerState.Dead;
    public bool IsHurt => currentState == PlayerState.Hurt;
    public bool IsAttacking => currentState == PlayerState.Attack;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (combat == null)
            combat = GetComponent<PlayerCombat>();

        if (health == null)
            health = GetComponent<PlayerHealth>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (health != null && health.IsDead)
        {
            currentState = PlayerState.Dead;
            return;
        }

        if (health != null && health.IsHurt)
        {
            currentState = PlayerState.Hurt;
            return;
        }

        if (combat != null && combat.IsAttacking)
        {
            currentState = PlayerState.Attack;
            return;
        }

        if (movement == null || rb == null)
        {
            currentState = PlayerState.Idle;
            return;
        }

        if (!movement.IsGrounded)
        {
            currentState = rb.velocity.y > 0.1f ? PlayerState.Jump : PlayerState.Fall;
            return;
        }

        if (Mathf.Abs(movement.MoveInput) > 0.01f)
        {
            currentState = PlayerState.Move;
            return;
        }

        currentState = PlayerState.Idle;
    }
}