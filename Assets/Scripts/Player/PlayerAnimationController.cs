using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D rb;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (animator == null || playerController == null)
            return;

        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        PlayerController.PlayerState state = playerController.CurrentState;

        animator.SetBool("isRunning", state == PlayerController.PlayerState.Move);

        if (playerMovement != null)
        {
            animator.SetBool("isGrounded", playerMovement.IsGrounded);
        }

        if (rb != null)
        {
            animator.SetFloat("yVelocity", rb.velocity.y);
        }

        animator.SetBool("isJumping", state == PlayerController.PlayerState.Jump);
        animator.SetBool("isFalling", state == PlayerController.PlayerState.Fall);
        animator.SetBool("isHurt", state == PlayerController.PlayerState.Hurt);
        animator.SetBool("isDead", state == PlayerController.PlayerState.Dead);
        animator.SetBool("isAttacking", state == PlayerController.PlayerState.Attack);
    }
}