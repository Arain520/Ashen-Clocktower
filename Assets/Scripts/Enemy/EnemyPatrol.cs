using UnityEngine;

public class EnemyPatrolChase : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Transform player;

    [Header("巡逻参数")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool movingRight = true;

    [Header("追击参数")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float faceTargetTolerance = 0.1f;

    [Header("检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("转向冷却")]
    [SerializeField] private float turnCooldown = 0.5f;  // 冷却时间 0.5 秒
    private float turnCooldownTimer = 0f;  // 当前冷却计时器

    private Rigidbody2D rb;
    private float startX;
    private bool isChasing = false;

    private bool canMove = true;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;
    }

    private void Update()
    {
        if (player == null)
            return;

        CheckChaseState();

        // 每帧更新冷却计时器
        if (turnCooldownTimer > 0f)
        {
            turnCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        if (player == null)
            return;

        if (isChasing)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    private void CheckChaseState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isChasing = distanceToPlayer <= chaseRange;
    }
    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }
    private void Patrol()
    {
        float direction = movingRight ? 1f : -1f;

        bool hasGround = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            checkDistance,
            groundLayer
        );

        bool hitWall = Physics2D.Raycast(
            wallCheck.position,
            movingRight ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );

        bool reachedRightLimit = transform.position.x >= startX + patrolDistance;
        bool reachedLeftLimit = transform.position.x <= startX - patrolDistance;

        if ((!hasGround || hitWall || reachedRightLimit || reachedLeftLimit) && turnCooldownTimer <= 0f)
        {
            // 只有冷却时间结束后才会触发转向
            movingRight = !movingRight;
            direction = movingRight ? 1f : -1f;
            turnCooldownTimer = turnCooldown;  // 设置冷却计时器
        }

        SetFacing(movingRight);
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
    }

    private void Chase()
    {
        float deltaX = player.position.x - transform.position.x;

        // 玩家几乎就在正上方/正下方时，不要疯狂切方向
        if (Mathf.Abs(deltaX) > faceTargetTolerance)
        {
            movingRight = deltaX > 0f;
        }

        float direction = movingRight ? 1f : -1f;

        // 追击时只检测墙，不检测边缘
        bool hitWall = Physics2D.Raycast(
            wallCheck.position,
            movingRight ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );

        SetFacing(movingRight);

        if (hitWall)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        }
    }

    private void SetFacing(bool faceRight)
    {
        Vector3 scale = transform.localScale;

        if (faceRight)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * checkDistance
            );
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 dir = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + dir * checkDistance
            );
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(startX - patrolDistance, transform.position.y, 0f),
            new Vector3(startX + patrolDistance, transform.position.y, 0f)
        );
    }
}