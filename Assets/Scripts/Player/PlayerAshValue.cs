using UnityEngine;

public class PlayerAshValue : MonoBehaviour
{
    [Header("灰烬值")]
    [SerializeField] public float ashValue = 0f;
    [SerializeField] private float maxAshValue = 100f;

    public float CurrentAshValue => ashValue;
    public float MaxAshValue => maxAshValue;

    public void AddAshValue(float amount)
    {
        ashValue = Mathf.Clamp(ashValue + amount, 0f, maxAshValue);
        Debug.Log("当前灰烬值：" + ashValue);
    }

    public bool ConsumeAshValue(float amount)
    {
        if (ashValue < amount)
            return false;

        ashValue -= amount;
        return true;
    }
}
