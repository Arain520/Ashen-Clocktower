using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("玩家血量")]
    [SerializeField] private int maxHealth = 5;

    [Header("受伤参数")]
    [SerializeField] private float invincibleTime = 1f;
    [SerializeField] private float knockbackForceX = 6f;
    [SerializeField] private float knockbackForceY = 4f;
    [SerializeField] private float hurtLockTime = 0.3f;

    private int currentHealth;
    private bool isInvincible = false;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private SpriteFlash spriteFlash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteFlash = GetComponent<SpriteFlash>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;
        Debug.Log("玩家受伤，当前血量：" + currentHealth);

        if (spriteFlash != null)
        {
            spriteFlash.PlayFlash();
        }

        StartCoroutine(HurtRoutine(attackerPosition));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtRoutine(Vector2 attackerPosition)
    {
        isInvincible = true;

        if (playerMovement != null)
        {
            playerMovement.AddControlLock();
        }

        float knockbackDirection = transform.position.x > attackerPosition.x ? 1f : -1f;
        Vector2 knockback = new Vector2(knockbackDirection * knockbackForceX, knockbackForceY);

        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        yield return new WaitForSeconds(hurtLockTime);

        if (playerMovement != null)
        {
            playerMovement.RemoveControlLock();
        }

        float remainInvincibleTime = Mathf.Max(0f, invincibleTime - hurtLockTime);
        yield return new WaitForSeconds(remainInvincibleTime);

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("玩家死亡");
        gameObject.SetActive(false);
    }
}