using UnityEngine;

/// <summary>
/// 对话播放管理器。
/// 负责接收触发器请求，并把文本交给 DialogueUI 播放。
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);
    }

    /// <summary>
    /// 播放一组文本。正在播放时会忽略新的触发，避免多个文本互相覆盖。
    /// </summary>
    public void PlayDialogue(string[] lines)
    {
        PlayDialogue(lines, false);
    }

    /// <summary>
    /// waitBeforeContinue 为 true 时，用于自动触发文本：每句完整显示后等待一段时间才允许继续。
    /// </summary>
    public void PlayDialogue(string[] lines, bool waitBeforeContinue)
    {
        if (IsPlaying)
            return;

        if (lines == null || lines.Length == 0)
            return;

        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>(true);

        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueManager：没有找到 DialogueUI，无法显示文本。");
            return;
        }

        IsPlaying = true;
        dialogueUI.Play(lines, OnDialogueFinished, waitBeforeContinue);
    }

    private void OnDialogueFinished()
    {
        IsPlaying = false;
    }
}
