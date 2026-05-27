using UnityEngine;

[RequireComponent(typeof(AshSystem))]
public class PhaseShiftController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode phaseKey = KeyCode.R;

    [Header("Ash Cost")]
    public int minAshToEnter = 20;
    public float ashDrainPerSecond = 10f;

    [Header("State")]
    public bool isAshenPhase;

    private AshSystem ashSystem;

    public bool IsAshenPhase => isAshenPhase;

    private void Awake()
    {
        ashSystem = GetComponent<AshSystem>();
    }

    private void Update()
    {
        HandlePhaseInput();
        DrainAshWhilePhaseActive();
    }

    private void HandlePhaseInput()
    {
        if (!Input.GetKeyDown(phaseKey))
            return;

        if (isAshenPhase)
        {
            ExitAshenPhase();
            return;
        }

        if (ashSystem.HasEnoughAsh(minAshToEnter))
        {
            // 进入灰烬态需要先支付一笔最低灰烬值，避免玩家只剩极少资源也能切相。
            //ashSystem.ConsumeAsh(minAshToEnter);
            EnterAshenPhase();
        }
        else
        {
            Debug.Log("灰烬值不足，无法进入灰烬态。");
        }
    }

    private void DrainAshWhilePhaseActive()
    {
        if (!isAshenPhase)
            return;

        ashSystem.ConsumeAsh(ashDrainPerSecond * Time.deltaTime);

        if (ashSystem.IsEmpty())
        {
            ExitAshenPhase();
        }
    }

    private void EnterAshenPhase()
    {
        if (isAshenPhase)
            return;

        isAshenPhase = true;

        if (AshenWorldManager.Instance != null)
        {
            AshenWorldManager.Instance.SetAshenWorldActive(true);
        }

        AshenPhaseShaderController visualController = GetOrCreateVisualController();
        visualController.SetAshenVisualActive(true);
    }

    private void ExitAshenPhase()
    {
        if (!isAshenPhase)
            return;

        isAshenPhase = false;

        if (AshenWorldManager.Instance != null)
        {
            AshenWorldManager.Instance.SetAshenWorldActive(false);
        }

        AshenPhaseShaderController visualController = GetOrCreateVisualController();
        visualController.SetAshenVisualActive(false);
    }

    private AshenPhaseShaderController GetOrCreateVisualController()
    {
        if (AshenPhaseShaderController.Instance != null)
            return AshenPhaseShaderController.Instance;

        Debug.LogWarning("未找到 AshenPhaseShaderController，已自动创建一个运行时 Shader 视觉控制器。");
        GameObject controllerObject = new GameObject("AshenPhaseShaderController");
        return controllerObject.AddComponent<AshenPhaseShaderController>();
    }
}
