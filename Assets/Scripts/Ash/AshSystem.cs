using System;
using UnityEngine;

public class AshSystem : MonoBehaviour
{
    public static AshSystem Instance { get; private set; }

    [Header("Ash Value")]
    [Tooltip("当前灰烬值")]
    public float currentAsh;
    [Tooltip("最大灰烬值")]
    public float maxAsh = 100f;

    public event Action<float, float> OnAshChanged;

    public float CurrentAsh => currentAsh;
    public float MaxAsh => maxAsh;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 AshSystem，请确认玩家身上只挂载一个。");
            return;
        }

        Instance = this;
        ClampAsh();
        NotifyAshChanged();
    }

    public void AddAsh(int amount)
    {
        if (amount <= 0)
            return;

        currentAsh += amount;
        ClampAsh();
        NotifyAshChanged();
    }

    public bool ConsumeAsh(int amount)
    {
        if (!HasEnoughAsh(amount))
            return false;

        currentAsh -= amount;
        ClampAsh();
        NotifyAshChanged();
        return true;
    }

    public bool ConsumeAsh(float amount)
    {
        if (amount <= 0)
            return true;

        if (currentAsh <= 0)
            return false;

        currentAsh -= amount;
        ClampAsh();
        NotifyAshChanged();
        return true;
    }

    public bool HasEnoughAsh(int amount)
    {
        return currentAsh >= amount;
    }

    public float GetAshPercent()
    {
        if (maxAsh <= 0)
            return 0;

        return currentAsh / maxAsh;
    }

    public bool IsEmpty()
    {
        return currentAsh <= 0;
    }

    private void ClampAsh()
    {
        currentAsh = Mathf.Clamp(currentAsh, 0, maxAsh);
    }

    private void NotifyAshChanged()
    {
        OnAshChanged?.Invoke(currentAsh, maxAsh);
    }
}
