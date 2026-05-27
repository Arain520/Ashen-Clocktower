using UnityEngine;
using UnityEngine.UI;

public class AshValueUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AshSystem ashSystem;
    [SerializeField] private Image ashFillImage;

    [Header("Fill")]
    [SerializeField] private float fillSmoothSpeed = 8f;

    private float targetFillAmount;

    private void Start()
    {
        if (ashSystem == null)
            ashSystem = AshSystem.Instance;

        if (ashSystem == null)
        {
            Debug.LogWarning("AshValueUI 没有找到 AshSystem，灰烬值 UI 无法更新。");
            return;
        }

        ashSystem.OnAshChanged += HandleAshChanged;
        SetTargetFill(ashSystem.CurrentAsh, ashSystem.MaxAsh);
        SetFillImmediately(targetFillAmount);
    }

    private void Update()
    {
        if (ashSystem == null || ashFillImage == null)
            return;

        ashFillImage.fillAmount = Mathf.Lerp(
            ashFillImage.fillAmount,
            targetFillAmount,
            fillSmoothSpeed * Time.deltaTime
        );
    }

    private void OnDestroy()
    {
        if (ashSystem != null)
            ashSystem.OnAshChanged -= HandleAshChanged;
    }

    private void HandleAshChanged(float currentAsh, float maxAsh)
    {
        SetTargetFill(currentAsh, maxAsh);
    }

    private void SetTargetFill(float currentAsh, float maxAsh)
    {
        if (maxAsh <= 0)
        {
            targetFillAmount = 0f;
            return;
        }

        targetFillAmount = Mathf.Clamp01(currentAsh / maxAsh);
    }

    private void SetFillImmediately(float fillAmount)
    {
        if (ashFillImage != null)
            ashFillImage.fillAmount = fillAmount;
    }
}
