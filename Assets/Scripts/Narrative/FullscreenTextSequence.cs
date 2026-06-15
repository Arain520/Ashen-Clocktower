using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FullscreenTextSequence : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField] private bool playOnStart;
    [SerializeField] private bool playOnStartOnlyForNewGame = true;
    [SerializeField] private string sequenceId = "intro";
    [TextArea(2, 4)]
    [SerializeField] private string[] lines =
    {
        "钟声停下之后，灰烬开始倒流。",
        "被焚毁的旧日，在黑暗里等待下一次呼吸。",
        "醒来吧。"
    };
    [SerializeField] private float firstLineDelay = 0.5f;
    [SerializeField] private float continuePromptDelay = 3f;
    [SerializeField] private KeyCode continueKey = KeyCode.E;
    [SerializeField] private string continueHint = "按 E 继续";
    [SerializeField] private float lineFadeDuration = 0.45f;
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private bool pauseGameDuringPlayback = true;

    [Header("Finish")]
    [SerializeField] private bool loadMainMenuWhenFinished;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private bool forceCompactWhiteStyle = true;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textLabel;
    [SerializeField] private TextMeshProUGUI continueHintLabel;
    [SerializeField] private int sortingOrder = 32766;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color hintColor = new Color(1f, 1f, 1f, 0.72f);
    [SerializeField] private int fontSize = 16;
    [SerializeField] private int hintFontSize = 22;
    [SerializeField] private Vector2 textPadding = new Vector2(24f, 120f);

    private Coroutine playbackCoroutine;
    private float previousTimeScale = 1f;
    private const int ForcedFontSize = 12;
    private const int ForcedHintFontSize = 16;
    private static readonly Vector2 ForcedTextPadding = new Vector2(12f, 100f);

    public bool IsPlaying => playbackCoroutine != null;
    public string SequenceId => sequenceId;

    private void Awake()
    {
        EnsureUI();
        HideImmediate();
    }

    private void Start()
    {
        if (!playOnStart)
            return;

        if (playOnStartOnlyForNewGame && !NarrativePlaybackFlags.ConsumeNewGameIntroRequest())
            return;

        Play();
    }

    public void Play()
    {
        if (IsPlaying)
            return;

        playbackCoroutine = StartCoroutine(PlaySequence());
    }

    public void Play(string[] overrideLines)
    {
        if (overrideLines != null && overrideLines.Length > 0)
            lines = overrideLines;

        Play();
    }

    private IEnumerator PlaySequence()
    {
        if (pauseGameDuringPlayback)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        EnsureUI();
        textLabel.text = "";
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (firstLineDelay > 0f)
            yield return new WaitForSecondsRealtime(firstLineDelay);

        if (lines != null)
        {
            textLabel.text = "";

            for (int i = 0; i < lines.Length; i++)
            {
                yield return FadeInLine(lines[i], i > 0);
                SetContinueHintVisible(false);

                if (continuePromptDelay > 0f)
                    yield return new WaitForSecondsRealtime(continuePromptDelay);

                SetContinueHintVisible(true);
                yield return WaitForContinueInput();
            }
        }

        float timer = 0f;
        float startAlpha = canvasGroup.alpha;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = fadeDuration <= 0f ? 1f : timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        HideImmediate();

        if (pauseGameDuringPlayback)
            Time.timeScale = previousTimeScale;

        playbackCoroutine = null;

        if (loadMainMenuWhenFinished && !string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    private void EnsureUI()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("FullscreenTextCanvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        canvas.sortingOrder = sortingOrder;

        canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();

        Image background = canvas.GetComponentInChildren<Image>(true);
        if (background == null)
        {
            GameObject backgroundObject = new GameObject("BlackScreen");
            backgroundObject.transform.SetParent(canvas.transform, false);
            background = backgroundObject.AddComponent<Image>();

            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
        }

        background.color = backgroundColor;

        if (textLabel == null)
        {
            GameObject textObject = new GameObject("SequenceText");
            textObject.transform.SetParent(background.transform, false);
            textLabel = textObject.AddComponent<TextMeshProUGUI>();
        }

        textLabel.color = forceCompactWhiteStyle ? Color.white : textColor;
        textLabel.fontSize = forceCompactWhiteStyle ? ForcedFontSize : fontSize;
        textLabel.alignment = TextAlignmentOptions.Center;
        textLabel.enableWordWrapping = true;
        textLabel.richText = true;

        RectTransform textRect = textLabel.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        Vector2 effectiveTextPadding = forceCompactWhiteStyle ? ForcedTextPadding : textPadding;
        textRect.offsetMin = effectiveTextPadding;
        textRect.offsetMax = -effectiveTextPadding;

        if (continueHintLabel == null)
        {
            GameObject hintObject = new GameObject("ContinueHint");
            hintObject.transform.SetParent(background.transform, false);
            continueHintLabel = hintObject.AddComponent<TextMeshProUGUI>();
        }

        continueHintLabel.text = continueHint;
        continueHintLabel.color = forceCompactWhiteStyle ? new Color(1f, 1f, 1f, 0.72f) : hintColor;
        continueHintLabel.fontSize = forceCompactWhiteStyle ? ForcedHintFontSize : hintFontSize;
        continueHintLabel.alignment = TextAlignmentOptions.Center;
        continueHintLabel.richText = true;

        RectTransform hintRect = continueHintLabel.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.offsetMin = new Vector2(0f, 90f);
        hintRect.offsetMax = new Vector2(0f, 140f);
        SetContinueHintVisible(false);
    }

    private void HideImmediate()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (textLabel != null)
            textLabel.text = "";

        SetContinueHintVisible(false);
    }

    private IEnumerator WaitForContinueInput()
    {
        while (!Input.GetKeyDown(continueKey))
            yield return null;
    }

    private IEnumerator FadeInLine(string line, bool appendSeparator)
    {
        string existingText = textLabel.text;
        string separator = appendSeparator ? "\n\n" : "";
        string lineText = line ?? "";
        float timer = 0f;

        while (timer < lineFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = lineFadeDuration <= 0f ? 1f : timer / lineFadeDuration;
            int visibleCharacterCount = Mathf.CeilToInt(lineText.Length * Mathf.Clamp01(progress));
            textLabel.text = existingText + separator + lineText.Substring(0, visibleCharacterCount);
            yield return null;
        }

        textLabel.text = existingText + separator + lineText;
    }

    private void SetContinueHintVisible(bool visible)
    {
        if (continueHintLabel != null)
            continueHintLabel.gameObject.SetActive(visible);
    }
}
