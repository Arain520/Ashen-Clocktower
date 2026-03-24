using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("敌人血量")]
    [SerializeField] private int maxHealth = 3;

    [Header("受击参数")]
    [SerializeField] private float knockbackForceX = 4f;
    [SerializeField] private float knockbackForceY = 2f;
    [SerializeField] private float hurtTime = 0.2f;

    private int currentHealth;
    private bool isHurt = false;

    private Rigidbody2D rb;
    private EnemyPatrolChase enemyPatrolChase;
    private SpriteFlash spriteFlash;

    public bool IsHurt => isHurt;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyPatrolChase = GetComponent<EnemyPatrolChase>();
        spriteFlash = GetComponent<SpriteFlash>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " 受到伤害，当前血量：" + currentHealth);

        if (spriteFlash != null)
        {
            spriteFlash.PlayFlash();
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HurtRoutine(attackerPosition));
    }

    private IEnumerator HurtRoutine(Vector2 attackerPosition)
    {
        isHurt = true;

        if (enemyPatrolChase != null)
        {
            enemyPatrolChase.SetCanMove(false);
        }

        float direction = transform.position.x > attackerPosition.x ? 1f : -1f;

        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(direction * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(hurtTime);

        if (enemyPatrolChase != null)
        {
            enemyPatrolChase.SetCanMove(true);
        }

        isHurt = false;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡");
        Destroy(gameObject);
    }
}