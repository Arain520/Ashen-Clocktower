using UnityEngine;

/// <summary>
/// 玩家灰烬回血控制器：
/// 按下指定按键后，消耗灰烬值恢复一部分生命。
/// </summary>
public class AshHealController : MonoBehaviour
{
    [Header("Heal Input")]
    [SerializeField] private KeyCode healKey = KeyCode.L;

    [Header("Ash Cost")]
    [SerializeField] private bool useAshCost = true;
    [SerializeField] private int ashCost = 30;

    [Header("Heal Amount")]
    [SerializeField] private int healAmount = 30;

    private AshSystem ashSystem;
    private PlayerStats playerStats;

    private void Awake()
    {
        ashSystem = GetComponent<AshSystem>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (Input.GetKeyDown(healKey))
            TryHeal();
    }

    private void TryHeal()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("AshHealController：没有找到 PlayerStats，无法回血。");
            return;
        }

        if (playerStats.currentHealth >= playerStats.GetFinalMaxHealth())
        {
            Debug.Log("生命值已满，不需要消耗灰烬回血。");
            return;
        }

        if (useAshCost)
        {
            if (ashSystem == null)
                ashSystem = GetComponent<AshSystem>();

            if (ashSystem == null)
            {
                Debug.LogWarning("AshHealController：没有找到 AshSystem，无法消耗灰烬回血。");
                return;
            }

            if (!ashSystem.HasEnoughAsh(ashCost))
            {
                Debug.Log("灰烬值不足，无法回复生命。");
                return;
            }

            //确认可以回血后再消耗灰烬，避免满血时浪费资源。
            ashSystem.ConsumeAsh(ashCost);
        }

        bool healed = playerStats.RestoreHealthBy(healAmount);

        if (healed)
            Debug.Log("消耗灰烬回复生命值。");
    }
}
