using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃参数")]
    [SerializeField] private float jumpForce = 12f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("辅助参数")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    private Rigidbody2D rb;

    private float moveInput;
    private bool isGrounded;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private int controlLockCount = 0;

    public bool CanControl => controlLockCount == 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        Debug.Log("找到 Rigidbody2D: " + (rb != null));
        Debug.Log("Animator 引用: " + (animator != null ? animator.gameObject.name : "没有拖入 Animator"));
    }

    private void Update()
    {
        CheckGround();
        UpdateCoyoteTime();

        if (CanControl)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            UpdateJumpBuffer();
            TryJump();
            Flip();
        }
        else
        {
            moveInput = 0f;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (CanControl)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void TryJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;

        // 默认角色原始朝向是向左
        if (moveInput > 0)
        {
            localScale.x = -Mathf.Abs(localScale.x);
        }
        else if (moveInput < 0)
        {
            localScale.x = Mathf.Abs(localScale.x);
        }

        transform.localScale = localScale;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isRunning = moveInput != 0f && CanControl;

        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    public void AddControlLock()
    {
        controlLockCount++;
    }

    public void RemoveControlLock()
    {
        controlLockCount--;

        if (controlLockCount < 0)
        {
            controlLockCount = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}