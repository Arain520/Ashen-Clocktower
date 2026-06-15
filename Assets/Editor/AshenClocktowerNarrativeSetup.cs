using UnityEditor;
using UnityEngine;
using System.IO;

public static class AshenClocktowerNarrativeSetup
{
    [MenuItem("Tools/灰烬钟塔/安装黑屏叙事流程")]
    public static void InstallNarrativeFlow()
    {
        FullscreenTextSequence introSequence = FindOrCreateSequence(
            "IntroBlackScreenSequence",
            true,
            false,
            new[]
            {
                "钟声停下之后，灰烬开始倒流。",
                "被焚毁的旧日，在黑暗里等待下一次呼吸。",
                "醒来吧。"
            });

        FullscreenTextSequence endingSequence = FindOrCreateSequence(
            "EndingBlackScreenSequence",
            false,
            false,
            new[]
            {
                "石像回应了最后一缕灰烬。",
                "钟塔没有再次响起。",
                "但你听见了远处的风。"
            });

        EndingStatueTrigger endingStatue = Object.FindObjectOfType<EndingStatueTrigger>();
        if (endingStatue == null)
        {
            GameObject statueObject = new GameObject("Ending_Interactive_Statue");
            statueObject.transform.position = GetSuggestedStatuePosition();

            SpriteRenderer renderer = statueObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetOrCreatePlaceholderStatueSprite();
            renderer.color = new Color(0.55f, 0.52f, 0.48f, 1f);
            renderer.sortingOrder = 3;

            BoxCollider2D collider = statueObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 3f);

            endingStatue = statueObject.AddComponent<EndingStatueTrigger>();
            Selection.activeGameObject = statueObject;
        }

        SerializedObject serializedStatue = new SerializedObject(endingStatue);
        serializedStatue.FindProperty("endingSequence").objectReferenceValue = endingSequence;
        serializedStatue.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(introSequence);
        EditorUtility.SetDirty(endingSequence);
        EditorUtility.SetDirty(endingStatue);

        Debug.Log("已安装黑屏叙事流程：开场自动播放，结尾石像按 E 互动后播放。请把 Ending_Interactive_Statue 移到关卡终点，并替换成正式石像美术。");
    }

    private static FullscreenTextSequence FindOrCreateSequence(string objectName, bool playOnStart, bool loadMainMenuWhenFinished, string[] lines)
    {
        GameObject existingObject = GameObject.Find(objectName);
        FullscreenTextSequence sequence = existingObject != null ? existingObject.GetComponent<FullscreenTextSequence>() : null;

        if (sequence == null)
        {
            GameObject sequenceObject = existingObject != null ? existingObject : new GameObject(objectName);
            sequence = sequenceObject.AddComponent<FullscreenTextSequence>();
        }

        SerializedObject serializedSequence = new SerializedObject(sequence);
        serializedSequence.FindProperty("playOnStart").boolValue = playOnStart;
        serializedSequence.FindProperty("playOnStartOnlyForNewGame").boolValue = playOnStart;
        serializedSequence.FindProperty("loadMainMenuWhenFinished").boolValue = loadMainMenuWhenFinished;
        serializedSequence.FindProperty("forceCompactWhiteStyle").boolValue = true;
        serializedSequence.FindProperty("continuePromptDelay").floatValue = 3f;
        serializedSequence.FindProperty("continueKey").intValue = (int)KeyCode.E;
        serializedSequence.FindProperty("continueHint").stringValue = "按 E 继续";
        serializedSequence.FindProperty("lineFadeDuration").floatValue = 0.45f;
        serializedSequence.FindProperty("textColor").colorValue = Color.white;
        serializedSequence.FindProperty("hintColor").colorValue = new Color(1f, 1f, 1f, 0.72f);
        serializedSequence.FindProperty("fontSize").intValue = 16;
        serializedSequence.FindProperty("hintFontSize").intValue = 22;
        serializedSequence.FindProperty("textPadding").vector2Value = new Vector2(24f, 120f);

        SerializedProperty linesProperty = serializedSequence.FindProperty("lines");
        linesProperty.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
            linesProperty.GetArrayElementAtIndex(i).stringValue = lines[i];

        serializedSequence.ApplyModifiedPropertiesWithoutUndo();
        return sequence;
    }

    private static Vector3 GetSuggestedStatuePosition()
    {
        Player player = Object.FindObjectOfType<Player>();
        if (player != null)
            return player.transform.position + new Vector3(4f, 0f, 0f);

        return Vector3.zero;
    }

    private static Sprite GetOrCreatePlaceholderStatueSprite()
    {
        const string folderPath = "Assets/Generated";
        const string assetPath = folderPath + "/EndingStatuePlaceholder.png";

        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existingSprite != null)
            return existingSprite;

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Generated");

        Texture2D texture = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color stone = new Color(0.55f, 0.52f, 0.48f, 1f);
        Color darkStone = new Color(0.34f, 0.32f, 0.3f, 1f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool body = x >= 9 && x <= 22 && y >= 8 && y <= 39;
                bool head = x >= 11 && x <= 20 && y >= 34 && y <= 44;
                bool baseBlock = x >= 5 && x <= 26 && y >= 2 && y <= 9;
                bool edge = x == 9 || x == 22 || y == 8 || y == 39 || y == 2 || y == 9;

                if (body || head || baseBlock)
                    texture.SetPixel(x, y, edge ? darkStone : stone);
                else
                    texture.SetPixel(x, y, clear);
            }
        }

        texture.filterMode = FilterMode.Point;
        File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath);

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
