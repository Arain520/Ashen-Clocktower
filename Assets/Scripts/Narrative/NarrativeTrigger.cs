using UnityEngine;

/// <summary>
/// 世界观文本触发器。
/// 挂在带 Collider2D 的触发区域上，用来播放灰烬残响、区域描述或谜题提示。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NarrativeTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "钟声停下的那一刻，灰烬开始倒流。",
        "这里曾经有一座桥。现在只剩下灰。"
    };

    [Header("Trigger Settings")]
    [SerializeField] private string triggerId;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool requireInteractKey;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool requireAshenPhase;

    private bool playerInRange;
    private bool hasTriggered;
    private bool isShowingInteractToolTip;
    private PhaseShiftController playerPhaseShiftController;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange || !requireInteractKey)
            return;

        UpdateInteractToolTipVisibility();

        if (Input.GetKeyDown(interactKey))
            TryPlayDialogue();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = true;
        playerPhaseShiftController = collision.GetComponent<PhaseShiftController>();

        if (playerPhaseShiftController == null)
            playerPhaseShiftController = collision.GetComponentInParent<PhaseShiftController>();

        if (requireInteractKey)
            UpdateInteractToolTipVisibility();

        if (!requireInteractKey)
            TryPlayDialogue();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = false;
        playerPhaseShiftController = null;

        HideInteractToolTip();
    }

    private void TryPlayDialogue()
    {
        // 交互式文本允许反复查看；自动触发文本才使用 triggerOnce 限制。
        if (!requireInteractKey && triggerOnce && hasTriggered)
            return;

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("NarrativeTrigger：没有配置任何文本内容。");
            return;
        }

        if (!CanTriggerInCurrentPhase(true))
            return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("NarrativeTrigger：场景中没有 DialogueManager，无法播放世界观文本。");
            return;
        }

        if (DialogueManager.Instance.IsPlaying)
            return;

        HideInteractToolTip();

        bool waitBeforeContinue = !requireInteractKey;
        DialogueManager.Instance.PlayDialogue(dialogueLines, waitBeforeContinue);

        if (!requireInteractKey)
            hasTriggered = true;
    }

    private bool CanTriggerInCurrentPhase(bool showWarning)
    {
        if (!requireAshenPhase)
            return true;

        if (playerPhaseShiftController == null)
        {
            if (showWarning)
                Debug.LogWarning("NarrativeTrigger：该文本需要灰烬态，但玩家身上没有 PhaseShiftController。");

            return false;
        }

        return playerPhaseShiftController.IsAshenPhase;
    }

    private void UpdateInteractToolTipVisibility()
    {
        bool shouldShow = playerInRange && requireInteractKey && CanTriggerInCurrentPhase(false);

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
            shouldShow = false;

        if (shouldShow)
            ShowInteractToolTip();
        else
            HideInteractToolTip();
    }

    private void ShowInteractToolTip()
    {
        if (isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance == null)
        {
            Debug.LogWarning("NarrativeTrigger：场景中没有 UI_MainScene，无法显示交互提示。");
            return;
        }

        UI_MainScene.instance.SetWhetherShowInteractToolTip(true);
        isShowingInteractToolTip = true;
    }

    private void HideInteractToolTip()
    {
        if (!isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance != null)
            UI_MainScene.instance.SetWhetherShowInteractToolTip(false);

        isShowingInteractToolTip = false;
    }
}
