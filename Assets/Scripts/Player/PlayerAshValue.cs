using UnityEngine;

public class PlayerAshValue : MonoBehaviour
{
    [Header("灰烬值")]
    [SerializeField] private float ashValue = 0f;  // 当前的灰烬值
    [SerializeField] private float maxAshValue = 100f;  // 最大灰烬值

    public float AshValue => ashValue;
    public float MaxAshValue => maxAshValue;

    // 增加灰烬值
    public void AddAshValue(float amount)
    {
        ashValue = Mathf.Min(ashValue + amount, maxAshValue);  // 最大值限制
        Debug.Log("Added Ash Value. Current: " + ashValue);
    }

    // 使用灰烬值进行回血
    public void HealWithAshValue(float healAmount)
    {
        // 可以用灰烬值来恢复生命（未来使用）
        // 假设玩家最大生命是 100
        float healingAmount = Mathf.Min(healAmount, ashValue);  // 最大值限制
        ashValue -= healingAmount;  // 使用灰烬值恢复生命
        Debug.Log("Healed with Ash Value. Remaining: " + ashValue);
    }
}
