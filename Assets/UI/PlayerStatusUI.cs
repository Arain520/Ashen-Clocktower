using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("数据引用")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAshValue playerAshValue;

    [Header("血量UI")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private GameObject healthIconPrefab;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite emptyHealthSprite;

    [Header("灰烬UI")]
    [SerializeField] private Image ashBarFill;
    [SerializeField] private TextMeshProUGUI ashText;
    [SerializeField] private float ashLerpSpeed = 8f;  // 控制灰烬条平滑过渡的速度

    private List<Image> healthIcons = new List<Image>();
    private float currentDisplayedAshRatio = 0f;  // 当前显示的灰烬值比例

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerAshValue == null)
            playerAshValue = FindObjectOfType<PlayerAshValue>();

        InitHealthIcons();
        RefreshHealthUI();
        RefreshAshUI(true);  // 初始时立即显示灰烬条
    }

    private void Update()
    {
        RefreshHealthUI();
        RefreshAshUI(false);  // 更新灰烬值
    }

    private void InitHealthIcons()
    {
        if (playerHealth == null || healthContainer == null || healthIconPrefab == null)
            return;

        // 清空旧图标
        for (int i = healthContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(healthContainer.GetChild(i).gameObject);
        }

        healthIcons.Clear();

        for (int i = 0; i < playerHealth.MaxHealth; i++)
        {
            GameObject iconObj = Instantiate(healthIconPrefab, healthContainer);
            Image iconImage = iconObj.GetComponent<Image>();

            if (iconImage != null)
            {
                healthIcons.Add(iconImage);
            }
        }
    }

    private void RefreshHealthUI()
    {
        if (playerHealth == null || healthIcons.Count == 0)
            return;

        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (i < playerHealth.CurrentHealth)
            {
                healthIcons[i].sprite = fullHealthSprite;
                healthIcons[i].color = Color.white;
            }
            else
            {
                healthIcons[i].sprite = emptyHealthSprite;
                healthIcons[i].color = new Color(0.35f, 0.35f, 0.35f, 1f);
            }
        }
    }

    private void RefreshAshUI(bool instant)
    {
        if (playerAshValue == null || ashBarFill == null)
            return;

        float targetRatio = 0f;

        if (playerAshValue.MaxAshValue > 0f)
        {
            targetRatio = playerAshValue.CurrentAshValue / playerAshValue.MaxAshValue;
        }

        if (instant)
        {
            currentDisplayedAshRatio = targetRatio;
        }
        else
        {
            // 平滑过渡灰烬值
            currentDisplayedAshRatio = Mathf.Lerp(
                currentDisplayedAshRatio,
                targetRatio,
                Time.deltaTime * ashLerpSpeed
            );
        }

        ashBarFill.fillAmount = currentDisplayedAshRatio;

        if (ashText != null)
        {
            ashText.text = Mathf.RoundToInt(playerAshValue.CurrentAshValue) + " / " +
                           Mathf.RoundToInt(playerAshValue.MaxAshValue);
        }
    }
}