using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PhaseManager : MonoBehaviour
{
    [Header("灰烬态物体")]
    [SerializeField] private GameObject[] ashObjects;  // 灰烬态物体（如平台）

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume postProcessVolume;  // 引用 PostProcessing Volume

    private bool isAshPhase = false;  // 当前是否处于灰烬态
    private bool isChangingPhase = false;  // 防止重复切换
    private float cooldownTime = 1f;  // 冷却时间 3 秒
    private float cooldownTimer = 0f;  // 当前冷却时间

    // 切换相位
    public void TogglePhase()
    {
        // 如果在冷却时间内，则返回
        if (cooldownTimer > 0f)
        {
            Debug.Log("Phase change is on cooldown.");
            return;
        }

        // 如果正在切换相位，直接返回，防止重复调用
        if (isChangingPhase)
            return;

        isChangingPhase = true;  // 标记为正在切换相位

        Debug.Log("Toggling Phase... Current Phase: " + (isAshPhase ? "Ash Phase" : "Normal Phase"));

        if (isAshPhase)
        {
            SetNormalPhase();  // 如果是灰烬态，切换回现实态
        }
        else
        {
            SetAshPhase();  // 如果是现实态，切换到灰烬态
        }

        // 重置冷却时间
        cooldownTimer = cooldownTime;

        // 切换完相位后重置标志，允许下一次切换
        isChangingPhase = false;
    }

    // 切换到现实态
    private void SetNormalPhase()
    {
        Debug.Log("Switching to Normal Phase...");

        // 隐藏灰烬态物体
        foreach (var ashObj in ashObjects)
        {
            ashObj.SetActive(false);  // 隐藏灰烬态物体
            Debug.Log(ashObj.name + " set to inactive.");
        }

        // 禁用 Post-Processing 效果
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;  // 禁用 Post-Processing 滤镜效果
            Debug.Log("Post-Processing Volume disabled.");
        }

        isAshPhase = false;  // 设定当前为现实态
    }

    // 切换到灰烬态
    private void SetAshPhase()
    {
        Debug.Log("Switching to Ash Phase...");

        // 显示所有灰烬态物体
        foreach (var ashObj in ashObjects)
        {
            ashObj.SetActive(true);  // 显示灰烬态物体
            Debug.Log(ashObj.name + " set to active.");
        }

        // 启用 Post-Processing 效果
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = true;  // 启用 Post-Processing 滤镜效果
            Debug.Log("Post-Processing Volume enabled.");
        }

        isAshPhase = true;  // 设定当前为灰烬态
    }

    // 每帧更新冷却时间
    private void Update()
    {
        // 如果冷却时间大于零，减少冷却时间
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 玩家按下 R 键时切换相位
        if (Input.GetKeyDown(KeyCode.R))  // 按 R 键触发切换相位
        {
            Debug.Log("R key pressed. Toggling phase...");
            TogglePhase();
        }
    }

    private void Start()
    {
        // 在 Start 中将灰烬态物体设置为不显示
        foreach (var ashObj in ashObjects)
        {
            ashObj.SetActive(false);  // 初始时隐藏所有灰烬态物体
            Debug.Log(ashObj.name + " initially inactive.");
        }

        // 初始时禁用 Post-Processing
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;  // 禁用 Post-Processing 滤镜效果
            Debug.Log("Post-Processing Volume initially disabled.");
        }
    }
}