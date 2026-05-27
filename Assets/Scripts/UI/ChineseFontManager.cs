using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 统一管理 UI 字体：
/// - 纯英文文本保留原字体。
/// - 含中文文本使用指定的中文字体资源。
/// - TMP 推荐使用 fallback，这样英文仍走原字体，中文走中文字体资源。
/// </summary>
public class ChineseFontManager : MonoBehaviour
{
    public static ChineseFontManager Instance { get; private set; }

    [Header("中文字体资源")]
    [SerializeField] private TMP_FontAsset chineseTMPFont;
    [SerializeField] private Font chineseLegacyFont;

    [Header("扫描设置")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool includeInactiveObjects = true;
    [SerializeField] private bool refreshDynamicText = true;
    [SerializeField] private float refreshInterval = 0.5f;

    [Header("TextMeshPro 设置")]
    [SerializeField] private bool addChineseTMPFontAsFallback = true;
    [SerializeField] private bool forceTMPChineseFontWhenContainsChinese = false;

    private readonly Dictionary<TMP_Text, TMP_FontAsset> originalTMPFonts = new Dictionary<TMP_Text, TMP_FontAsset>();
    private readonly Dictionary<Text, Font> originalLegacyFonts = new Dictionary<Text, Font>();
    private readonly HashSet<TMP_FontAsset> registeredFallbackFonts = new HashSet<TMP_FontAsset>();

    private Coroutine refreshCoroutine;
    private bool warnedMissingTMPFont;
    private bool warnedMissingLegacyFont;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyFontsToAllTexts();

        if (refreshDynamicText)
            refreshCoroutine = StartCoroutine(RefreshFontRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (refreshCoroutine != null)
            StopCoroutine(refreshCoroutine);
    }

    /// <summary>
    /// 手动刷新所有场景文本。动态修改 UI 文本后，也可以主动调用这个方法。
    /// </summary>
    public void ApplyFontsToAllTexts()
    {
        ApplyFontsToTMPTexts();
        ApplyFontsToLegacyTexts();
    }

    private IEnumerator RefreshFontRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.1f, refreshInterval));

        while (true)
        {
            ApplyFontsToAllTexts();
            yield return wait;
        }
    }

    private void ApplyFontsToTMPTexts()
    {
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();

        foreach (TMP_Text text in texts)
        {
            if (!ShouldHandleObject(text.gameObject))
                continue;

            if (!originalTMPFonts.ContainsKey(text))
                originalTMPFonts.Add(text, text.font);

            bool hasChinese = ContainsChinese(text.text);
            TMP_FontAsset originalFont = originalTMPFonts[text];

            if (!hasChinese)
            {
                // 英文或数字文本恢复原来的 TMP 字体。
                if (text.font != originalFont)
                    text.font = originalFont;

                continue;
            }

            if (chineseTMPFont == null)
            {
                WarnMissingTMPFont();
                continue;
            }

            if (addChineseTMPFontAsFallback && originalFont != null)
                RegisterTMPFallback(originalFont);

            if (forceTMPChineseFontWhenContainsChinese)
            {
                // 强制模式：整个文本组件都使用中文 TMP 字体。
                text.font = chineseTMPFont;
            }
            else
            {
                // 推荐模式：主字体保持原字体，中文字符通过 fallback 字体显示。
                text.font = originalFont;
            }
        }
    }

    private void ApplyFontsToLegacyTexts()
    {
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();

        foreach (Text text in texts)
        {
            if (!ShouldHandleObject(text.gameObject))
                continue;

            if (!originalLegacyFonts.ContainsKey(text))
                originalLegacyFonts.Add(text, text.font);

            bool hasChinese = ContainsChinese(text.text);

            if (!hasChinese)
            {
                // 旧版 Text 没有 TMP fallback，纯英文恢复原字体。
                Font originalFont = originalLegacyFonts[text];
                if (text.font != originalFont)
                    text.font = originalFont;

                continue;
            }

            if (chineseLegacyFont == null)
            {
                WarnMissingLegacyFont();
                continue;
            }

            // 旧版 Text 不能按字符混用字体，只能整个组件切换为中文字体。
            text.font = chineseLegacyFont;
        }
    }

    private bool ShouldHandleObject(GameObject target)
    {
        if (target == null)
            return false;

        // 只处理已经加载到场景里的对象，避免运行时改到 Project 面板里的 prefab/asset。
        if (!target.scene.IsValid() || string.IsNullOrEmpty(target.scene.name))
            return false;

        if (!includeInactiveObjects && !target.activeInHierarchy)
            return false;

        return true;
    }

    private void RegisterTMPFallback(TMP_FontAsset originalFont)
    {
        if (originalFont == null || chineseTMPFont == null || originalFont == chineseTMPFont)
            return;

        if (registeredFallbackFonts.Contains(originalFont))
            return;

        if (!originalFont.fallbackFontAssetTable.Contains(chineseTMPFont))
            originalFont.fallbackFontAssetTable.Add(chineseTMPFont);

        registeredFallbackFonts.Add(originalFont);
    }

    private bool ContainsChinese(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // 常用中文、扩展中文、兼容中文。
            if ((c >= '\u4e00' && c <= '\u9fff') ||
                (c >= '\u3400' && c <= '\u4dbf') ||
                (c >= '\uf900' && c <= '\ufaff'))
            {
                return true;
            }
        }

        return false;
    }

    private void WarnMissingTMPFont()
    {
        if (warnedMissingTMPFont)
            return;

        Debug.LogWarning("ChineseFontManager：检测到中文 TextMeshPro 文本，但还没有指定中文 TMP Font Asset。");
        warnedMissingTMPFont = true;
    }

    private void WarnMissingLegacyFont()
    {
        if (warnedMissingLegacyFont)
            return;

        Debug.LogWarning("ChineseFontManager：检测到中文旧版 Text 文本，但还没有指定中文 Font。");
        warnedMissingLegacyFont = true;
    }
}
