using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("伤害参数")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 6f;

    [Header("碰撞层")]
    [SerializeField] private LayerMask destroyOnHitLayer;

    private Rigidbody2D rb;
    private bool launched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 start, Vector2 target, float arcHeight)
    {
        if (rb == null)
        {
            Debug.LogWarning("EnemyProjectile 缺少 Rigidbody2D。");
            return;
        }

        transform.position = start;

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        if (gravity <= 0.01f)
        {
            Debug.LogWarning("投掷物的 Rigidbody2D.gravityScale 不能为 0。");
            return;
        }

        float displacementY = target.y - start.y;
        float displacementX = target.x - start.x;

        float velocityY = Mathf.Sqrt(2f * gravity * arcHeight);
        float timeUp = velocityY / gravity;

        float extraHeight = arcHeight - displacementY;
        if (extraHeight < 0.1f)
        {
            extraHeight = 0.1f;
        }

        float timeDown = Mathf.Sqrt(2f * extraHeight / gravity);
        float totalTime = timeUp + timeDown;

        float velocityX = displacementX / totalTime;

        rb.velocity = new Vector2(velocityX, velocityY);
        launched = true;
    }

    private void Update()
    {
        if (!launched || rb == null)
            return;

        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject);
            return;
        }

        if (((1 << collision.gameObject.layer) & destroyOnHitLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
