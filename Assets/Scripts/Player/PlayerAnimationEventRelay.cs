using UnityEngine;

public class PlayerAnimationEventRelay : MonoBehaviour
{
    [Header("转发目标")]
    [SerializeField] private PlayerCombat playerCombat;

    private void Awake()
    {
        if (playerCombat == null)
        {
            playerCombat = GetComponentInParent<PlayerCombat>();
        }
    }

    public void AttackHitCheck()
    {
        if (playerCombat == null)
        {
            Debug.LogWarning("PlayerAnimationEventRelay 未找到 PlayerCombat，无法转发 AttackHitCheck。");
            return;
        }

        playerCombat.AttackHitCheck();
    }
}