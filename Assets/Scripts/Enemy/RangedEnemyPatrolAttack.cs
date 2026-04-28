using System.Collections;
using UnityEngine;

public class RangedEnemyPatrolAttack : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("巡逻参数")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 6f;
    [SerializeField] private bool movingRight = true;

    [Header("索敌 / 攻击参数")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackStopTime = 0.4f;

    [Header("抛物线参数")]
    [SerializeField] private float arcHeight = 2.5f;
    [SerializeField] private Vector2 aimOffset = new Vector2(0f, 0.8f);

    [Header("检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("转向冷却")]
    [SerializeField] private float turnCooldown = 0.5f;

    private Rigidbody2D rb;
    private float startX;
    private float turnCooldownTimer;
    private float attackCooldownTimer;

    private bool canMove = true;
    private bool isAttacking = false;
    private bool playerDetected = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (turnCooldownTimer > 0f)
        {
            turnCooldownTimer -= Time.deltaTime;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        UpdateDetectState();

        if (playerDetected)
        {
            FacePlayer();

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (!isAttacking && attackCooldownTimer <= 0f && distanceToPlayer <= attackRange)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private void FixedUpdate()
    {
        if (!canMove || isAttacking)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        Patrol();
    }

    private void UpdateDetectState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        playerDetected = distanceToPlayer <= detectRange;
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
            movingRight = !movingRight;
            direction = movingRight ? 1f : -1f;
            turnCooldownTimer = turnCooldown;
        }

        SetFacing(movingRight);
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        rb.velocity = new Vector2(0f, rb.velocity.y);

        yield return new WaitForSeconds(attackStopTime * 0.5f);

        FireProjectile();

        yield return new WaitForSeconds(attackStopTime * 0.5f);

        isAttacking = false;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("远程敌人缺少 projectilePrefab / firePoint / player 引用。");
            return;
        }

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
        Rigidbody2D projectileRb = projectileObj.GetComponent<Rigidbody2D>();

        if (projectile == null || projectileRb == null)
        {
            Debug.LogWarning("投掷物预制体缺少 EnemyProjectile 或 Rigidbody2D。");
            return;
        }

        Vector2 start = firePoint.position;
        Vector2 target = (Vector2)player.position + aimOffset;

        projectile.Launch(start, target, arcHeight);
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        bool faceRight = player.position.x > transform.position.x;
        movingRight = faceRight;
        SetFacing(faceRight);
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

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                new Vector3(startX - patrolDistance, transform.position.y, 0f),
                new Vector3(startX + patrolDistance, transform.position.y, 0f)
            );
        }
    }
}