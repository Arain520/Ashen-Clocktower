using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Animator animator;

    [Header("攻击参数")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("攻击检测点")]
    [SerializeField] private Transform attackPoint;

    [Header("攻击节奏")]
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackLockTime = 0.4f;

    private float attackTimer;
    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;
    private bool hasDealtDamageThisAttack = false;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (playerMovement != null && !playerMovement.CanControl)
            return;

        if (Input.GetKeyDown(KeyCode.J) && attackTimer <= 0f && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        hasDealtDamageThisAttack = false;
        isAttacking = true;
        attackTimer = attackCooldown;

        if (playerMovement != null)
        {
            playerMovement.AddControlLock();
        }

        // 触发攻击动画
        if (animator != null)
        {
            Debug.Log("触发攻击动画！");
            animator.SetTrigger("attack");
        }
        else
        {
            Debug.Log("没有正确获取动画！");
        }

        //Camera.main.GetComponent<SimpleCameraFollow>().TriggerShake(0.2f);

        yield return new WaitForSeconds(attackLockTime);

        if (playerMovement != null)
        {
            playerMovement.RemoveControlLock();
        }

        isAttacking = false;
    }

    public void AttackHitCheck()
    {
        if (isAttacking)
            return;
        if (hasDealtDamageThisAttack)
            return;

        hasDealtDamageThisAttack= true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        Debug.Log("发动攻击，命中数量：" + hitEnemies.Length);
        if(hitEnemies.Length > 0)
        {
            Camera.main.GetComponent<SimpleCameraFollow>().TriggerShake(0.2f);
        }
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}