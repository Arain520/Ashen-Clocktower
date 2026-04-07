using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Dive,
        Return
    }

    [Header("引用")]
    [SerializeField] private Transform player;

    [Header("侦测参数")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 3f;

    [Header("巡游参数")]
    [SerializeField] private float patrolRadius = 2f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolChangeInterval = 2f;

    [Header("追击参数")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0f, 1.5f);

    [Header("俯冲参数")]
    [SerializeField] private float diveSpeed = 6f;
    [SerializeField] private float diveDuration = 0.5f;

    [Header("返回参数")]
    [SerializeField] private float returnSpeed = 3f;

    private State currentState = State.Patrol;

    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private Vector3 returnPoint;
    private Vector3 diveTarget;

    private float patrolTimer;
    private float diveTimer;

    private bool canMove = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = transform.position;
        ChooseNewPatrolTarget();
    }

    private void Update()
    {
        if (player == null)
            return;

        if (!canMove)
            return;

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                CheckDetectPlayer();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Dive:
                UpdateDive();
                break;

            case State.Return:
                UpdateReturn();
                break;
        }

        FaceTarget();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void UpdatePatrol()
    {
        patrolTimer += Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            patrolTarget,
            patrolSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, patrolTarget) < 0.1f || patrolTimer >= patrolChangeInterval)
        {
            ChooseNewPatrolTarget();
        }
    }

    private void ChooseNewPatrolTarget()
    {
        patrolTimer = 0f;

        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    private void CheckDetectPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectRange)
        {
            currentState = State.Chase;
            returnPoint = transform.position;
        }
    }

    private void UpdateChase()
    {
        Vector3 targetPosition = player.position + (Vector3)attackOffset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            chaseSpeed * Time.deltaTime
        );

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Dive;
            diveTimer = 0f;
            diveTarget = player.position;
        }

        if (distanceToPlayer > detectRange * 1.5f)
        {
            currentState = State.Return;
        }
    }

    private void UpdateDive()
    {
        diveTimer += Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            diveTarget,
            diveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, diveTarget) < 0.1f || diveTimer >= diveDuration)
        {
            currentState = State.Return;
        }
    }

    private void UpdateReturn()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            returnPoint,
            returnSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, returnPoint) < 0.1f)
        {
            currentState = State.Patrol;
            ChooseNewPatrolTarget();
        }
    }

    private void FaceTarget()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;

        if (player.position.x > transform.position.x)
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            startPosition == Vector3.zero ? transform.position : startPosition,
            patrolRadius
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(returnPoint, 0.15f);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(diveTarget, 0.15f);
    }
}