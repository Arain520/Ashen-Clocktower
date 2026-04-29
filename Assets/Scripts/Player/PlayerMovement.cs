using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃参数")]
    [SerializeField] private float jumpForce = 12f;  // 初始跳跃力度
    [SerializeField] private float maxJumpForce = 18f;  // 最大跳跃力度
    [SerializeField] private float jumpHoldFactor = 2f;  // 跳跃按住时的增益比例（越大按住时间跳得越高）

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
    private float jumpTimeHeld = 0f;  // 记录按下跳跃键的时间
    private bool isJumping = false;  // 是否正在跳跃

    private int controlLockCount = 0;

    public bool CanControl => controlLockCount == 0;

    public float MoveInput => moveInput;
    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("找到 Rigidbody2D: " + (rb != null));
    }

    private void Update()
    {
        CheckGround();
        UpdateCoyoteTime();

        if (CanControl)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            UpdateJumpBuffer();
            HandleJumpInput();  // 使用新的跳跃输入逻辑
            Flip();
        }
        else
        {
            moveInput = 0f;
        }
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

    private void HandleJumpInput()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            // 按住跳跃键时，增加跳跃力度
            if (Input.GetButton("Jump"))  // 持续按住跳跃键
            {
                isJumping = true;
                jumpTimeHeld += Time.deltaTime;  // 增加按住时间
                float dynamicJumpForce = Mathf.Lerp(jumpForce, maxJumpForce, jumpTimeHeld * jumpHoldFactor);  // 动态增加跳跃力度
                rb.velocity = new Vector2(rb.velocity.x, dynamicJumpForce);
            }
            else if (isJumping)  // 松开时直接跳跃
            {
                Jump();
                jumpTimeHeld = 0f;  // 重置按住跳跃的时间
                isJumping = false;
            }

            jumpBufferCounter = 0f;  // 重置跳跃缓存
            coyoteTimeCounter = 0f;  // 重置短暂允许跳跃的时间
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);  // 直接给一个初始的跳跃速度
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