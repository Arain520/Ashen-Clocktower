using System.Collections;
using UnityEngine;

public class FlyingEnemyHealth : MonoBehaviour, IDamageable
{
    [Header("飞天怪血量")]
    [SerializeField] private int maxHealth = 2;

    [Header("受击参数")]
    [SerializeField] private float knockbackDistance = 0.5f;
    [SerializeField] private float knockbackTime = 0.15f;

    private int currentHealth;
    private bool isHurt = false;

    private SpriteFlash spriteFlash;
    private FlyingEnemyAI flyingEnemyAI;

    private void Awake()
    {
        spriteFlash = GetComponent<SpriteFlash>();
        flyingEnemyAI = GetComponent<FlyingEnemyAI>();
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

        if (flyingEnemyAI != null)
        {
            flyingEnemyAI.SetCanMove(false);
        }

        Vector3 startPos = transform.position;
        float dir = transform.position.x > attackerPosition.x ? 1f : -1f;
        Vector3 targetPos = startPos + new Vector3(dir * knockbackDistance, 0.2f, 0f);

        float timer = 0f;
        while (timer < knockbackTime)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, timer / knockbackTime);
            yield return null;
        }

        if (flyingEnemyAI != null)
        {
            flyingEnemyAI.SetCanMove(true);
        }

        isHurt = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}