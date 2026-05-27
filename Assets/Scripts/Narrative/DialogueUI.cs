using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 控制屏幕下方的文本面板。
/// 支持逐字显示，按空格跳过当前句或进入下一句。
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continueHintText;

    [Header("Typing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;
    [SerializeField] private string continueHint = "按空格继续";

    [Header("Auto Trigger Continue")]
    [SerializeField] private KeyCode autoContinueKey = KeyCode.E;
    [SerializeField] private string autoContinueHint = "按 E 继续";
    [SerializeField] private float autoContinueDelay = 3f;

    [Header("Continue Hint Fade")]
    [SerializeField] private bool pulseContinueHint = true;
    [SerializeField] private float hintMinAlpha = 0.25f;
    [SerializeField] private float hintMaxAlpha = 1f;
    [SerializeField] private float hintPulseSpeed = 2f;

    private string[] currentLines;
    private int currentLineIndex;
    private bool isTyping;
    private bool canContinue;
    private bool waitBeforeContinue;
    private KeyCode activeContinueKey;
    private string activeContinueHint;
    private Coroutine typingCoroutine;
    private Coroutine continueDelayCoroutine;
    private Action onFinished;

    private void Awake()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();

        HidePanel();
    }

    private void Update()
    {
        if (currentLines == null)
            return;

        UpdateContinueHintFade();

        if (Input.GetKeyDown(activeContinueKey))
            HandleContinueInput();
    }

    public void Play(string[] lines, Action finishedCallback)
    {
        Play(lines, finishedCallback, false);
    }

    public void Play(string[] lines, Action finishedCallback, bool shouldWaitBeforeContinue)
    {
        // 如果面板在场景里被 SetActive(false)，播放前先重新启用。
        // 后续显示/隐藏仍然交给 CanvasGroup 控制，避免协程无法启动。
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DialogueUI：DialogPanel 或它的父物体处于未激活状态，无法播放文本。请确认 Canvas 和 DialogPanel 的父级是激活的。");
            finishedCallback?.Invoke();
            return;
        }

        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();

        if (dialogueText == null)
        {
            Debug.LogWarning("DialogueUI：没有指定 DialogueText，无法显示文本。");
            finishedCallback?.Invoke();
            return;
        }

        currentLines = lines;
        currentLineIndex = 0;
        onFinished = finishedCallback;
        waitBeforeContinue = shouldWaitBeforeContinue;
        activeContinueKey = waitBeforeContinue ? autoContinueKey : continueKey;
        activeContinueHint = waitBeforeContinue ? autoContinueHint : continueHint;

        ShowPanel();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            FinishDialogue();
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (continueDelayCoroutine != null)
        {
            StopCoroutine(continueDelayCoroutine);
            continueDelayCoroutine = null;
        }

        canContinue = false;
        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        SetContinueHintVisible(false);

        if (line == null)
            line = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        StartContinueReadyProcess();
    }

    private void HandleContinueInput()
    {
        if (!canContinue && !isTyping)
            return;

        if (isTyping)
        {
            // 当前句还没打完时，按继续键直接显示完整句子。
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            StartContinueReadyProcess();
            return;
        }

        currentLineIndex++;
        ShowCurrentLine();
    }

    private void FinishDialogue()
    {
        HidePanel();

        currentLines = null;
        currentLineIndex = 0;
        canContinue = false;
        waitBeforeContinue = false;
        onFinished?.Invoke();
        onFinished = null;
    }

    private void ShowPanel()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        SetContinueHintText(activeContinueHint);
    }

    private void HidePanel()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (continueDelayCoroutine != null)
        {
            StopCoroutine(continueDelayCoroutine);
            continueDelayCoroutine = null;
        }

        isTyping = false;
        canContinue = false;

        if (dialogueText != null)
            dialogueText.text = "";

        SetContinueHintVisible(false);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void StartContinueReadyProcess()
    {
        if (waitBeforeContinue && autoContinueDelay > 0)
        {
            continueDelayCoroutine = StartCoroutine(WaitBeforeShowingContinueHint());
            return;
        }

        SetContinueReady();
    }

    private IEnumerator WaitBeforeShowingContinueHint()
    {
        SetContinueHintVisible(false);
        yield return new WaitForSeconds(autoContinueDelay);
        SetContinueReady();
    }

    private void SetContinueReady()
    {
        canContinue = true;
        SetContinueHintText(activeContinueHint);
        SetContinueHintVisible(true);
    }

    private void SetContinueHintText(string hint)
    {
        if (continueHintText != null)
            continueHintText.text = hint;
    }

    private void SetContinueHintVisible(bool visible)
    {
        if (continueHintText != null)
            continueHintText.gameObject.SetActive(visible);
    }

    private void UpdateContinueHintFade()
    {
        if (!pulseContinueHint || continueHintText == null || !continueHintText.gameObject.activeSelf)
            return;

        float minAlpha = Mathf.Clamp01(hintMinAlpha);
        float maxAlpha = Mathf.Clamp01(hintMaxAlpha);
        float pulse = (Mathf.Sin(Time.unscaledTime * hintPulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
        Color color = continueHintText.color;
        color.a = alpha;
        continueHintText.color = color;
    }
}
