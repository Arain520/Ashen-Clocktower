using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
//这里继承的是MonoBehaviour，所以Update一直在刷新
{
    #region AshCost
    [Header("Ash Cost")]
    [SerializeField] private bool useAshCost = true;
    [SerializeField] private int ashCost = 10;
    #endregion
    #region Cooldown
    [Header("Skill Cooldown")]
    //每个技能类的冷却时长
    public float cooldown;
    //技能冷却的计时器
    protected float cooldownTimer;
    #endregion

    protected virtual void Update()
    {
        //随时间递减，每秒减1单位
        cooldownTimer -= Time.deltaTime;
    }

    public virtual bool CanUseSkill()
    {
        if(cooldownTimer < 0)
            return true;
        else
            return false;
    }

    public virtual void RefreshCooldown()
    {
        //恢复冷却时间
        cooldownTimer = cooldown;
    }

    public bool TryConsumeAshCost()
    {
        // 如果此技能不需要消耗灰烬，则完全沿用原技能逻辑。
        if (!useAshCost)
            return true;

        AshSystem ashSystem = AshSystem.Instance;

        if (ashSystem == null)
        {
            Debug.Log("灰烬系统不存在，无法释放需要灰烬的技能");
            return false;
        }

        if (!ashSystem.HasEnoughAsh(ashCost))
        {
            Debug.Log("灰烬值不足，无法释放技能");
            return false;
        }

        // 只有确认技能即将释放时才真正扣除灰烬。
        ashSystem.ConsumeAsh(ashCost);
        return true;
    }
}
