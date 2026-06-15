using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AshenMapGeneratorWindow : EditorWindow
{
    private const string RootName = "Generated_MapRoot";
    private const float MapScale = 1f;

    private enum ExitDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private bool includeLabels = true;
    private bool includeAshPlatforms = true;
    private bool includeTriggers = true;
    private bool includeAbilityGates = true;
    private bool includeEnemies = true;
    private bool includeSavePoints = true;
    private bool includePortals = true;
    private bool includePositionLabels = true;

    private Sprite placeholderSprite;
    private Transform root;
    private MapCounters counters;

    [MenuItem("Tools/灰烬钟塔/地图生成器")]
    public static void OpenWindow()
    {
        GetWindow<AshenMapGeneratorWindow>("灰烬钟塔地图生成器");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("《灰烬钟塔》完整地图蓝图生成器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("该工具只生成可编辑的关卡原型对象，不会删除现有场景内容。所有生成物都会放在 Generated_MapRoot 下。当前版本按正常尺寸生成，并补充墙壁、背景块、出口、传送门和关卡细节色块。", MessageType.Info);

        EditorGUILayout.Space(8);
        includeLabels = EditorGUILayout.Toggle("包含文字标注", includeLabels);
        includeAshPlatforms = EditorGUILayout.Toggle("包含灰烬态平台", includeAshPlatforms);
        includeTriggers = EditorGUILayout.Toggle("包含触发器占位", includeTriggers);
        includeAbilityGates = EditorGUILayout.Toggle("包含能力门占位", includeAbilityGates);
        includeEnemies = EditorGUILayout.Toggle("包含敌人占位", includeEnemies);
        includeSavePoints = EditorGUILayout.Toggle("包含篝火存档点占位", includeSavePoints);
        includePortals = EditorGUILayout.Toggle("包含传送门/出口占位", includePortals);
        includePositionLabels = EditorGUILayout.Toggle("显示占位坐标标注", includePositionLabels);

        EditorGUILayout.Space(12);

        if (GUILayout.Button("生成完整地图蓝图", GUILayout.Height(34)))
            GenerateFullMapWithConfirm();

        if (GUILayout.Button("删除已生成地图", GUILayout.Height(28)))
            ClearGeneratedMapWithConfirm();

        if (GUILayout.Button("重新生成地图", GUILayout.Height(28)))
            RegenerateMap();
    }

    /// <summary>
    /// 生成完整地图前，先检查是否已有旧地图。
    /// </summary>
    private void GenerateFullMapWithConfirm()
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "生成完整地图蓝图",
                "场景中已经存在 Generated_MapRoot。是否删除旧地图并重新生成？",
                "删除并生成",
                "取消");

            if (!replace)
                return;

            Undo.DestroyObjectImmediate(oldRoot);
        }

        GenerateFullMap();
    }

    /// <summary>
    /// 删除已经生成的地图根节点，不影响其他场景对象。
    /// </summary>
    private void ClearGeneratedMapWithConfirm()
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot == null)
        {
            EditorUtility.DisplayDialog("删除已生成地图", "当前场景中没有找到 Generated_MapRoot。", "确定");
            return;
        }

        bool clear = EditorUtility.DisplayDialog(
            "删除已生成地图",
            "确定删除 Generated_MapRoot 以及它下面的所有生成对象吗？",
            "删除",
            "取消");

        if (!clear)
            return;

        Undo.DestroyObjectImmediate(oldRoot);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// 删除旧地图后立即重新生成。
    /// </summary>
    private void RegenerateMap()
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
            Undo.DestroyObjectImmediate(oldRoot);

        GenerateFullMap();
    }

    /// <summary>
    /// 生成完整地图骨架：区域、平台、能力点、触发点、敌人和 Boss 区域。
    /// </summary>
    private void GenerateFullMap()
    {
        counters = new MapCounters();
        placeholderSprite = LoadPlaceholderSprite();

        GameObject rootObject = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootObject, "Generate Ashen Clocktower Map");
        root = rootObject.transform;

        GenerateIntroPath();
        GenerateBrokenBridgeTutorial();
        GenerateCourtyard();
        GenerateTowerHallHub();
        GenerateGearCorridor();
        GenerateAshFoundry();
        GenerateFrostBellRoom();
        GenerateMemorialHall();
        GenerateTowerClimbRoute();
        GenerateBossRoom();

        Selection.activeGameObject = rootObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        PrintSummary();
    }

    /// <summary>
    /// 创建区域父物体，所有区域对象都归类在自己的区域下。
    /// </summary>
    private Transform CreateArea(string areaObjectName, string label, Vector2 labelPosition)
    {
        GameObject area = new GameObject(areaObjectName);
        Undo.RegisterCreatedObjectUndo(area, "Create Map Area");
        area.transform.SetParent(root);
        counters.areaCount++;

        if (includeLabels)
            CreateAreaLabel(area.transform, label, labelPosition);

        return area.transform;
    }

    /// <summary>
    /// Area 01：开场到第一个篝火，负责基础移动和氛围建立。
    /// </summary>
    private void GenerateIntroPath()
    {
        Transform area = CreateArea("Area_01_荒凉小径", "荒凉小径", new Vector2(-62, 8));
        CreateAreaBlockout(area, "荒凉小径", new Vector2(-66, 3), new Vector2(48, 18), Colors.BackHaunt);
        CreateRoomFillDetails(area, "荒凉小径", new Vector2(-66, 3), new Vector2(48, 18), Colors.DetailHaunt, Colors.DetailStone);

        CreatePlatform(area, "Platform_荒凉小径_地面_01_Haunt", new Vector2(-80, -1), new Vector2(28, 2), Colors.GroundHaunt);
        CreatePlatform(area, "Platform_荒凉小径_台阶_01_Haunt", new Vector2(-66, 3), new Vector2(8, 1), Colors.GroundHaunt);
        CreatePlatform(area, "Platform_荒凉小径_出口坡道_01_Haunt", new Vector2(-51, 1), new Vector2(12, 1), Colors.GroundHaunt);
        CreatePlatform(area, "Platform_荒凉小径_上层短台_01_Haunt", new Vector2(-76, 5.5f), new Vector2(7, 1), Colors.GroundHaunt);
        CreatePlatform(area, "Platform_荒凉小径_上层短台_02_Haunt", new Vector2(-61, 7.2f), new Vector2(6, 1), Colors.GroundHaunt);
        CreatePlatform(area, "Platform_荒凉小径_碎石跳台_01_Haunt", new Vector2(-54, 4.5f), new Vector2(4.5f, 0.8f), Colors.GroundHaunt);
        CreateDetailBlock(area, "Detail_荒凉小径_远景残墙_01", new Vector2(-82, 4), new Vector2(3, 7), Colors.DetailHaunt);
        CreateDetailBlock(area, "Detail_荒凉小径_远景残墙_02", new Vector2(-72, 6), new Vector2(2, 5), Colors.DetailHaunt);
        CreateDetailBlock(area, "Detail_荒凉小径_碎石堆_01", new Vector2(-58, 1.2f), new Vector2(4, 1), Colors.DetailStone);
        CreateExitMarker(area, "Exit_荒凉小径_前往断桥教学区", new Vector2(-46, 1.8f), "前往断桥教学区", ExitDirection.Right);

        if (includeSavePoints)
            CreateSavePoint(area, "SavePoint_篝火_荒凉小径", new Vector2(-68, 1.2f));

        if (includeTriggers)
        {
            CreateNarrativeTrigger(area, "NarrativeTrigger_开场落地点", new Vector2(-82, 2), new Vector2(5, 4), false, false,
                "钟声停下的那一刻，灰烬开始倒流。",
                "你在荒凉小径醒来，远处的钟塔没有任何回响。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_篝火旁", new Vector2(-68, 2), new Vector2(4, 4), true, false,
                "这团火还没有熄灭。",
                "它像是在等某个迟到的人。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_第一次击杀小怪后", new Vector2(-55, 2), new Vector2(5, 4), true, false,
                "敌人倒下时，灰烬短暂地聚集在你身边。",
                "战斗会让灰烬重新流动。");
        }

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_小怪_荒凉小径_01", new Vector2(-60, 1), false);
            CreateEnemyPlaceholder(area, "Enemy_小怪_荒凉小径_02", new Vector2(-48, 2), false);
            CreateEnemyWave(area, "荒凉小径_巡逻", new Vector2(-79, 1), new Vector2(-73, 6.5f), new Vector2(-66, 4.2f), new Vector2(-56, 5.5f));
        }
    }

    /// <summary>
    /// Area 02：断桥教学区，用灰烬态旧桥展示核心机制。
    /// </summary>
    private void GenerateBrokenBridgeTutorial()
    {
        Transform area = CreateArea("Area_02_断桥教学区", "断桥教学区", new Vector2(-33, 10));
        CreateAreaBlockout(area, "断桥教学区", new Vector2(-32, 4), new Vector2(36, 20), Colors.BackTower);
        CreateRoomFillDetails(area, "断桥教学区", new Vector2(-32, 4), new Vector2(36, 20), Colors.DetailStone, Colors.DetailAshGhost);

        CreatePlatform(area, "Platform_断桥左岸_01", new Vector2(-42, -1), new Vector2(12, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_断桥右岸_01", new Vector2(-23, -1), new Vector2(14, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_断桥观察台_01", new Vector2(-39, 5), new Vector2(5, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_断桥教学区_上层训练台_01", new Vector2(-43, 8), new Vector2(5, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_断桥教学区_右岸回看台_01", new Vector2(-20, 6.5f), new Vector2(5, 0.9f), Colors.GroundTower);
        CreateDetailBlock(area, "Detail_断桥教学区_断裂桥墩_01", new Vector2(-35, -0.2f), new Vector2(1.4f, 4), Colors.DetailStone);
        CreateDetailBlock(area, "Detail_断桥教学区_断裂桥墩_02", new Vector2(-28.5f, -0.2f), new Vector2(1.4f, 4), Colors.DetailStone);
        CreateDetailBlock(area, "Detail_断桥教学区_背景旧桥影_01", new Vector2(-31, 3.2f), new Vector2(9, 0.6f), Colors.DetailAshGhost);
        CreateExitMarker(area, "Exit_断桥教学区_返回荒凉小径", new Vector2(-45, 1.8f), "返回荒凉小径", ExitDirection.Left);
        CreateExitMarker(area, "Exit_断桥教学区_前往钟塔前庭", new Vector2(-18, 1.8f), "前往钟塔前庭", ExitDirection.Right);

        if (includeAshPlatforms)
        {
            CreateAshPlatform(area, "AshPlatform_断桥旧桥_01", new Vector2(-33, 1.4f), new Vector2(5, 0.7f));
            CreateAshPlatform(area, "AshPlatform_断桥旧桥_02", new Vector2(-28, 2.4f), new Vector2(4, 0.7f));
            CreateAshPlatform(area, "AshPlatform_断桥旧桥_03_上层提示", new Vector2(-31, 5.2f), new Vector2(3.8f, 0.7f));
        }

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要灰烬态_断桥", "灰烬态", new Vector2(-31, 1.2f), new Vector2(8, 5), Colors.GateAsh);

        CreateAbilityPickup(area, "Ability_灰烬态", "灰烬态", new Vector2(-41, 1.5f), Colors.AbilityAsh);

        if (includeTriggers)
        {
            CreateNarrativeTrigger(area, "NarrativeTrigger_断桥前", new Vector2(-39, 2), new Vector2(5, 4), true, false,
                "前方的桥已经断裂。",
                "但灰烬里也许还留着它过去的形状。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_灰烬态信息物品", new Vector2(-41, 2.5f), new Vector2(4, 4), true, false,
                "按 R 进入灰烬态。",
                "过去的结构会在灰烬中短暂显现。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_第一次通过灰烬桥后", new Vector2(-21, 2), new Vector2(5, 4), false, false,
                "旧桥在你脚下消散。",
                "灰烬态并不是幻觉，而是一段被烧焦的过去。");
        }
    }

    /// <summary>
    /// Area 03：钟塔前庭，获得投掷宝剑并引导玩家打开塔门。
    /// </summary>
    private void GenerateCourtyard()
    {
        Transform area = CreateArea("Area_03_钟塔前庭", "钟塔前庭", new Vector2(1, 12));
        CreateAreaBlockout(area, "钟塔前庭", new Vector2(5, 5), new Vector2(42, 24), Colors.BackTower);
        CreateRoomFillDetails(area, "钟塔前庭", new Vector2(5, 5), new Vector2(42, 24), Colors.DetailWindow, Colors.DetailStone);

        CreatePlatform(area, "Platform_钟塔前庭_地面_01", new Vector2(-7, -1), new Vector2(28, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔前庭_塔门台阶_01", new Vector2(8, 4), new Vector2(8, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔前庭_远程机关台_01", new Vector2(17, 7), new Vector2(6, 1), Colors.Mechanism);
        CreatePlatform(area, "Platform_钟塔前庭_左侧阳台_01", new Vector2(-11, 5.2f), new Vector2(6, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔前庭_中央战斗台_01", new Vector2(2, 3.4f), new Vector2(7, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔前庭_塔门上沿_01", new Vector2(19, 11.5f), new Vector2(5, 0.9f), Colors.GroundTower);
        CreateWallBlock(area, "Wall_钟塔前庭_塔门主体_01", new Vector2(20, 5), new Vector2(4, 12), Colors.WallTower);
        CreateDetailBlock(area, "Detail_钟塔前庭_远景钟塔窗_01", new Vector2(8, 9), new Vector2(2, 4), Colors.DetailWindow);
        CreateDetailBlock(area, "Detail_钟塔前庭_远景钟塔窗_02", new Vector2(13, 10), new Vector2(2, 5), Colors.DetailWindow);
        CreateExitMarker(area, "Exit_钟塔前庭_返回断桥教学区", new Vector2(-18, 1.8f), "返回断桥教学区", ExitDirection.Left);
        CreatePortalPlaceholder(area, "Portal_钟塔前庭_进入钟塔大厅", new Vector2(21.5f, 4.2f), "钟塔大厅入口");

        CreateAbilityPickup(area, "Ability_投掷宝剑", "投掷宝剑", new Vector2(-2, 1.5f), Colors.AbilitySword);

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要投掷宝剑_塔门", "投掷宝剑", new Vector2(18, 2.5f), new Vector2(2, 6), Colors.GateSword);

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_小怪_钟塔前庭_01", new Vector2(2, 1), false);
            CreateEnemyPlaceholder(area, "Enemy_小怪_钟塔前庭_02", new Vector2(7, 1), false);
            CreateEnemyPlaceholder(area, "Enemy_小怪_钟塔前庭_03", new Vector2(12, 1), false);
            CreateEnemyPlaceholder(area, "Enemy_精英_钟塔前庭_01", new Vector2(15, 1.2f), true);
            CreateEnemyWave(area, "钟塔前庭_增援", new Vector2(-11, 6.2f), new Vector2(-3, 1.2f), new Vector2(4, 4.5f), new Vector2(18, 8.2f));
        }

        if (includeTriggers)
            CreateNarrativeTrigger(area, "NarrativeTrigger_获得投掷宝剑", new Vector2(-2, 3), new Vector2(5, 4), true, false,
                "这把剑曾被挂在钟塔门前。",
                "它仍记得飞回主人手中的路径。");
    }

    /// <summary>
    /// Area 04：中心 Hub，连接三条分支和主塔登顶路线。
    /// </summary>
    private void GenerateTowerHallHub()
    {
        Transform area = CreateArea("Area_04_钟塔大厅", "钟塔大厅", new Vector2(35, 13));
        CreateAreaBlockout(area, "钟塔大厅", new Vector2(31, 12), new Vector2(42, 36), Colors.BackTower);
        CreateRoomFillDetails(area, "钟塔大厅", new Vector2(31, 12), new Vector2(42, 36), Colors.DetailClock, Colors.DetailChain);

        CreatePlatform(area, "Platform_钟塔大厅_主地面_01", new Vector2(30, -1), new Vector2(26, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_左上入口_01", new Vector2(18, 9), new Vector2(10, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_右上入口_01", new Vector2(45, 9), new Vector2(10, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_登顶入口_01", new Vector2(30, 15), new Vector2(8, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_左中层跳台_01", new Vector2(23, 5.5f), new Vector2(6, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_右中层跳台_01", new Vector2(38, 5.5f), new Vector2(6, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_中央钟摆台_01", new Vector2(31, 10.5f), new Vector2(5.5f, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_钟塔大厅_左上回路台_01", new Vector2(22, 18.5f), new Vector2(5, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_钟塔大厅_右上回路台_01", new Vector2(40, 18.5f), new Vector2(5, 0.9f), Colors.GroundTower);
        CreateDetailBlock(area, "Detail_钟塔大厅_中央大钟轮廓_01", new Vector2(31, 13), new Vector2(9, 9), Colors.DetailClock);
        CreateDetailBlock(area, "Detail_钟塔大厅_左侧立柱_01", new Vector2(14, 8), new Vector2(2, 22), Colors.WallTower);
        CreateDetailBlock(area, "Detail_钟塔大厅_右侧立柱_01", new Vector2(49, 8), new Vector2(2, 22), Colors.WallTower);
        CreatePortalPlaceholder(area, "Portal_钟塔大厅_返回钟塔前庭", new Vector2(19, 1.8f), "钟塔前庭塔门");
        CreateExitMarker(area, "Exit_钟塔大厅_齿轮廊道入口", new Vector2(17, 10.8f), "前往齿轮廊道", ExitDirection.Left);
        CreateExitMarker(area, "Exit_钟塔大厅_熔灰工坊入口", new Vector2(46, 10.8f), "前往熔灰工坊", ExitDirection.Right);
        CreateExitMarker(area, "Exit_钟塔大厅_主塔登顶入口", new Vector2(30, 17.2f), "前往主塔登顶路线", ExitDirection.Up);

        if (includeSavePoints)
            CreateSavePoint(area, "SavePoint_篝火_钟塔大厅", new Vector2(24, 1.2f));

        if (includeAshPlatforms)
        {
            CreateAshPlatform(area, "AshPlatform_大厅上方提示_01", new Vector2(31, 20), new Vector2(5, 0.7f));
            CreateAshPlatform(area, "AshPlatform_大厅上方提示_02", new Vector2(36, 25), new Vector2(5, 0.7f));
            CreateAshPlatform(area, "AshPlatform_大厅上方提示_03", new Vector2(26, 25), new Vector2(4.5f, 0.7f));
            CreateAshPlatform(area, "AshPlatform_大厅上方提示_04", new Vector2(31, 30), new Vector2(4.5f, 0.7f));
        }

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要旧日赐福_主塔登顶", "旧日赐福", new Vector2(31, 22), new Vector2(8, 12), Colors.GateBlessing);

        if (includeTriggers)
        {
            CreateNarrativeTrigger(area, "NarrativeTrigger_第一次进入钟塔大厅", new Vector2(25, 2.5f), new Vector2(6, 5), false, false,
                "钟塔大厅向上延伸，像一口倒置的井。",
                "塔顶的钟室被灰烬态平台锁在更高处。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_发现灰烬态登顶时间不足", new Vector2(33, 16), new Vector2(6, 5), true, false,
                "这些平台通向塔顶。",
                "但现在的灰烬还不足以支撑完整登顶。");
        }
    }

    /// <summary>
    /// Area 05：齿轮廊道，冲刺能力分支。
    /// </summary>
    private void GenerateGearCorridor()
    {
        Transform area = CreateArea("Area_05_齿轮廊道_冲刺", "齿轮廊道：冲刺", new Vector2(0, 30));
        CreateAreaBlockout(area, "齿轮廊道", new Vector2(-8, 25), new Vector2(46, 24), Colors.BackMechanism);
        CreateRoomFillDetails(area, "齿轮廊道", new Vector2(-8, 25), new Vector2(46, 24), Colors.DetailMechanism, Colors.DetailChain);

        CreatePlatform(area, "Platform_齿轮廊道_入口_01", new Vector2(2, 18), new Vector2(16, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_齿轮廊道_移动平台占位_01", new Vector2(-10, 24), new Vector2(8, 1), Colors.Mechanism);
        CreatePlatform(area, "Platform_齿轮廊道_冲刺奖励台_01", new Vector2(-22, 28), new Vector2(10, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_齿轮廊道_上层齿轮台_01", new Vector2(5, 29.5f), new Vector2(5, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_齿轮廊道_下层冲刺落脚_01", new Vector2(-2, 21.6f), new Vector2(5, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_齿轮廊道_奖励前跳台_01", new Vector2(-17, 31.5f), new Vector2(4.5f, 0.9f), Colors.Mechanism);
        CreateDanger(area, "Hazard_齿轮廊道_齿轮陷阱_01", new Vector2(-3, 20.5f), new Vector2(5, 1));
        CreateDetailBlock(area, "Detail_齿轮廊道_大齿轮占位_01", new Vector2(-2, 27), new Vector2(5, 5), Colors.DetailMechanism);
        CreateDetailBlock(area, "Detail_齿轮廊道_大齿轮占位_02", new Vector2(-15, 21), new Vector2(4, 4), Colors.DetailMechanism);
        CreateDetailBlock(area, "Detail_齿轮廊道_链条占位_01", new Vector2(8, 27), new Vector2(1, 12), Colors.DetailChain);
        CreateExitMarker(area, "Exit_齿轮廊道_返回钟塔大厅", new Vector2(9, 20.2f), "返回钟塔大厅", ExitDirection.Right);
        CreateAbilityPickup(area, "Ability_冲刺", "冲刺", new Vector2(-22, 31), Colors.AbilityDash);

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要冲刺_宽裂隙", "冲刺", new Vector2(-9, 22), new Vector2(8, 4), Colors.GateDash);

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_机械类_齿轮廊道_01", new Vector2(4, 20), false);
            CreateEnemyPlaceholder(area, "Enemy_机械类_齿轮廊道_02", new Vector2(-6, 25), false);
            CreateEnemyPlaceholder(area, "Enemy_机械类_齿轮廊道_03", new Vector2(-18, 30), false);
            CreateEnemyWave(area, "齿轮廊道_机械增援", new Vector2(5, 30.6f), new Vector2(-2, 22.6f), new Vector2(-10, 25.2f), new Vector2(-22, 30.2f));
        }

        if (includeTriggers)
            CreateNarrativeTrigger(area, "NarrativeTrigger_获得冲刺", new Vector2(-22, 33), new Vector2(5, 4), false, false,
                "齿轮重新转动。",
                "你的脚步短暂追上了钟声。");
    }

    /// <summary>
    /// Area 06：熔灰工坊，火魔法分支。
    /// </summary>
    private void GenerateAshFoundry()
    {
        Transform area = CreateArea("Area_06_熔灰工坊_火魔法", "熔灰工坊：火之魔法", new Vector2(65, 30));
        CreateAreaBlockout(area, "熔灰工坊", new Vector2(67, 24), new Vector2(42, 28), Colors.BackFoundry);
        CreateRoomFillDetails(area, "熔灰工坊", new Vector2(67, 24), new Vector2(42, 28), Colors.DetailForge, Colors.DetailLavaGlow);

        CreatePlatform(area, "Platform_熔灰工坊_主地面_01", new Vector2(65, 18), new Vector2(24, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_熔灰工坊_上层炉台_01", new Vector2(75, 26), new Vector2(12, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_熔灰工坊_左侧铁架_01", new Vector2(55, 24), new Vector2(5, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_熔灰工坊_中央吊台_01", new Vector2(65, 30.5f), new Vector2(5.5f, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_熔灰工坊_右侧机关台_01", new Vector2(83, 30), new Vector2(5, 0.9f), Colors.Mechanism);
        CreateDanger(area, "Hazard_熔灰工坊_熔灰池_01", new Vector2(64, 20.5f), new Vector2(10, 1));
        CreateDetailBlock(area, "Detail_熔灰工坊_熔炉主体_01", new Vector2(57, 26), new Vector2(5, 10), Colors.DetailForge);
        CreateDetailBlock(area, "Detail_熔灰工坊_烟囱_01", new Vector2(57, 34), new Vector2(2, 8), Colors.WallTower);
        CreateDetailBlock(area, "Detail_熔灰工坊_熔光背景_01", new Vector2(67, 22), new Vector2(13, 2), Colors.DetailLavaGlow);
        CreateExitMarker(area, "Exit_熔灰工坊_返回钟塔大厅", new Vector2(53, 20.2f), "返回钟塔大厅", ExitDirection.Left);
        CreateExitMarker(area, "Exit_熔灰工坊_前往静霜钟室", new Vector2(84, 27.5f), "前往静霜钟室", ExitDirection.Right);
        CreateAbilityPickup(area, "Ability_火之魔法", "火之魔法", new Vector2(78, 29), Colors.AbilityFire);

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要火魔法_熔灰机关", "火之魔法", new Vector2(82, 22), new Vector2(3, 6), Colors.GateFire);

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_火焰类_熔灰工坊_01", new Vector2(58, 20), false);
            CreateEnemyPlaceholder(area, "Enemy_火焰类_熔灰工坊_02", new Vector2(68, 20), false);
            CreateEnemyPlaceholder(area, "Enemy_火焰类_熔灰工坊_03", new Vector2(76, 28), false);
            CreateEnemyWave(area, "熔灰工坊_火焰增援", new Vector2(55, 25.2f), new Vector2(64, 31.5f), new Vector2(82, 31.2f), new Vector2(72, 20));
        }

        if (includeTriggers)
            CreateNarrativeTrigger(area, "NarrativeTrigger_获得火魔法", new Vector2(78, 31), new Vector2(5, 4), false, false,
                "熔灰仍在燃烧。",
                "火焰会打开那些被冷灰封住的机关。");
    }

    /// <summary>
    /// Area 07：静霜钟室，冰魔法分支。
    /// </summary>
    private void GenerateFrostBellRoom()
    {
        Transform area = CreateArea("Area_07_静霜钟室_冰魔法", "静霜钟室：冰之魔法", new Vector2(95, 49));
        CreateAreaBlockout(area, "静霜钟室", new Vector2(97, 44), new Vector2(38, 30), Colors.BackFrost);
        CreateRoomFillDetails(area, "静霜钟室", new Vector2(97, 44), new Vector2(38, 30), Colors.DetailFrost, Colors.DetailIcePillar);

        CreatePlatform(area, "Platform_静霜钟室_入口_01", new Vector2(92, 36), new Vector2(18, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_静霜钟室_高台_01", new Vector2(104, 44), new Vector2(12, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_静霜钟室_钟架_01", new Vector2(94, 50), new Vector2(8, 1), Colors.Mechanism);
        CreatePlatform(area, "Platform_静霜钟室_左侧冰台_01", new Vector2(86, 42), new Vector2(5, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_静霜钟室_中央回响台_01", new Vector2(96, 47), new Vector2(5, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_静霜钟室_右上冰台_01", new Vector2(109, 53), new Vector2(5, 0.9f), Colors.GroundTower);
        CreateDetailBlock(area, "Detail_静霜钟室_冰钟轮廓_01", new Vector2(94, 47), new Vector2(6, 8), Colors.DetailFrost);
        CreateDetailBlock(area, "Detail_静霜钟室_冰柱_01", new Vector2(84, 44), new Vector2(1.2f, 12), Colors.DetailIcePillar);
        CreateDetailBlock(area, "Detail_静霜钟室_冰柱_02", new Vector2(109, 45), new Vector2(1.2f, 10), Colors.DetailIcePillar);
        CreateExitMarker(area, "Exit_静霜钟室_返回熔灰工坊", new Vector2(84, 38.5f), "返回熔灰工坊", ExitDirection.Left);
        CreateExitMarker(area, "Exit_静霜钟室_前往古老纪念堂", new Vector2(105, 51.5f), "前往古老纪念堂", ExitDirection.Up);
        CreateAbilityPickup(area, "Ability_冰之魔法", "冰之魔法", new Vector2(104, 47), Colors.AbilityIce);

        if (includeAbilityGates)
            CreateAbilityGate(area, "Gate_需要冰魔法_静霜机关", "冰之魔法", new Vector2(98, 39), new Vector2(3, 6), Colors.GateIce);

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_冰霜类_静霜钟室_01", new Vector2(87, 38), false);
            CreateEnemyPlaceholder(area, "Enemy_冰霜类_静霜钟室_02", new Vector2(98, 38), false);
            CreateEnemyPlaceholder(area, "Enemy_冰霜类_静霜钟室_03", new Vector2(105, 46), false);
            CreateEnemyWave(area, "静霜钟室_冰霜增援", new Vector2(86, 43.2f), new Vector2(96, 48.2f), new Vector2(109, 54.2f), new Vector2(101, 38.2f));
        }

        if (includeTriggers)
            CreateNarrativeTrigger(area, "NarrativeTrigger_获得冰魔法", new Vector2(104, 49), new Vector2(5, 4), false, false,
                "霜覆盖了钟舌。",
                "静止的东西，也许能成为新的道路。");
    }

    /// <summary>
    /// Area 08：旧日长廊和纪念堂，放置石碑和旧日赐福。
    /// </summary>
    private void GenerateMemorialHall()
    {
        Transform area = CreateArea("Area_08_古老纪念堂_旧日长廊", "古老纪念堂：旧日赐福", new Vector2(55, 67));
        CreateAreaBlockout(area, "古老纪念堂", new Vector2(55, 61), new Vector2(58, 26), Colors.BackMemorial);
        CreateRoomFillDetails(area, "古老纪念堂", new Vector2(55, 61), new Vector2(58, 26), Colors.DetailMemorial, Colors.MemorialStone);

        CreatePlatform(area, "Platform_旧日长廊_地面_01", new Vector2(55, 55), new Vector2(42, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_旧日长廊_中央祭台_01", new Vector2(55, 60), new Vector2(12, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_旧日长廊_左侧悼念台_01", new Vector2(38, 60), new Vector2(6, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_旧日长廊_右侧悼念台_01", new Vector2(72, 60), new Vector2(6, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_旧日长廊_上层回廊_01", new Vector2(55, 67), new Vector2(18, 0.9f), Colors.GroundTower);
        CreateDetailBlock(area, "Detail_旧日长廊_拱门_01", new Vector2(40, 62), new Vector2(3, 10), Colors.DetailMemorial);
        CreateDetailBlock(area, "Detail_旧日长廊_拱门_02", new Vector2(55, 64), new Vector2(4, 12), Colors.DetailMemorial);
        CreateDetailBlock(area, "Detail_旧日长廊_拱门_03", new Vector2(70, 62), new Vector2(3, 10), Colors.DetailMemorial);
        CreateExitMarker(area, "Exit_古老纪念堂_返回静霜钟室", new Vector2(31, 57.5f), "返回静霜钟室", ExitDirection.Left);
        CreatePortalPlaceholder(area, "Portal_古老纪念堂_回到钟塔大厅", new Vector2(77, 57.5f), "钟塔大厅");

        if (includeSavePoints)
            CreateSavePoint(area, "SavePoint_篝火_古老纪念堂入口", new Vector2(36, 57));

        CreateMemorialHall(area);

        CreateAbilityPickup(area, "Ability_旧日赐福_无限灰烬态", "旧日赐福：灰烬态不再消耗灰烬", new Vector2(55, 63), Colors.AbilityBlessing);

        if (includeTriggers)
        {
            CreateNarrativeTrigger(area, "NarrativeTrigger_进入古老纪念堂", new Vector2(36, 58.5f), new Vector2(5, 4), false, false,
                "这里没有钟声。",
                "只有名字，和还没有完全熄灭的祝福。");
            CreateNarrativeTrigger(area, "NarrativeTrigger_中央石碑_旧日赐福", new Vector2(55, 64), new Vector2(6, 5), true, false,
                "中央石碑回应了你的灰烬。",
                "旧日赐福：灰烬态不再消耗灰烬值。");
        }

        if (includeEnemies)
            CreateEnemyWave(area, "古老纪念堂_守碑残影", new Vector2(38, 61.2f), new Vector2(47, 56.2f), new Vector2(63, 56.2f), new Vector2(72, 61.2f));
    }

    /// <summary>
    /// Area 09：主塔登顶路线，大量灰烬态平台组成纵向路线。
    /// </summary>
    private void GenerateTowerClimbRoute()
    {
        Transform area = CreateArea("Area_09_主塔登顶路线", "主塔登顶路线", new Vector2(30, 88));
        CreateAreaBlockout(area, "主塔登顶路线", new Vector2(30, 52), new Vector2(30, 82), Colors.BackTower);
        CreateRoomFillDetails(area, "主塔登顶路线", new Vector2(30, 52), new Vector2(30, 82), Colors.DetailCrack, Colors.DetailChain);

        CreatePlatform(area, "Platform_主塔登顶_入口_01", new Vector2(30, 15), new Vector2(8, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_中段休息台_01", new Vector2(25, 52), new Vector2(8, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_顶层入口_01", new Vector2(30, 84), new Vector2(12, 1), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_左壁落脚_01", new Vector2(20, 29), new Vector2(4, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_右壁落脚_01", new Vector2(40, 36), new Vector2(4, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_左壁落脚_02", new Vector2(20, 63), new Vector2(4, 0.9f), Colors.GroundTower);
        CreatePlatform(area, "Platform_主塔登顶_右壁落脚_02", new Vector2(40, 72), new Vector2(4, 0.9f), Colors.GroundTower);
        CreateDetailBlock(area, "Detail_主塔登顶_左内壁裂缝_01", new Vector2(18, 42), new Vector2(1, 16), Colors.DetailCrack);
        CreateDetailBlock(area, "Detail_主塔登顶_右内壁裂缝_01", new Vector2(42, 68), new Vector2(1, 18), Colors.DetailCrack);
        CreateDetailBlock(area, "Detail_主塔登顶_垂直钟链_01", new Vector2(30, 56), new Vector2(1, 56), Colors.DetailChain);
        CreateExitMarker(area, "Exit_主塔登顶_返回钟塔大厅", new Vector2(30, 17.5f), "返回钟塔大厅", ExitDirection.Down);
        CreateExitMarker(area, "Exit_主塔登顶_前往塔顶钟室", new Vector2(30, 86.2f), "前往塔顶钟室", ExitDirection.Up);

        if (includeAshPlatforms)
            CreateTowerClimbRoute(area);

        if (includeEnemies)
        {
            CreateEnemyPlaceholder(area, "Enemy_混合_主塔登顶_01", new Vector2(34, 28), false);
            CreateEnemyPlaceholder(area, "Enemy_混合_主塔登顶_02", new Vector2(25, 44), false);
            CreateEnemyPlaceholder(area, "Enemy_混合_主塔登顶_03", new Vector2(35, 60), false);
            CreateEnemyPlaceholder(area, "Enemy_混合_主塔登顶_04", new Vector2(26, 78), false);
            CreateEnemyWave(area, "主塔登顶_空中巡逻", new Vector2(20, 30.2f), new Vector2(40, 37.2f), new Vector2(25, 53.2f), new Vector2(20, 64.2f), new Vector2(40, 73.2f));
        }
    }

    /// <summary>
    /// Area 10：塔顶钟室，Boss 战和结束 CG 触发点。
    /// </summary>
    private void GenerateBossRoom()
    {
        Transform area = CreateArea("Area_10_塔顶钟室_Boss", "塔顶钟室：Boss", new Vector2(30, 108));
        CreateAreaBlockout(area, "塔顶钟室", new Vector2(30, 101), new Vector2(54, 28), Colors.BackBoss);
        CreateRoomFillDetails(area, "塔顶钟室", new Vector2(30, 101), new Vector2(54, 28), Colors.DetailClock, Colors.DetailChain);

        CreatePlatform(area, "Platform_塔顶钟室_战斗场地_01", new Vector2(30, 95), new Vector2(36, 2), Colors.GroundTower);
        CreatePlatform(area, "Platform_塔顶钟室_左钟架_01", new Vector2(15, 103), new Vector2(7, 1), Colors.Mechanism);
        CreatePlatform(area, "Platform_塔顶钟室_右钟架_01", new Vector2(45, 103), new Vector2(7, 1), Colors.Mechanism);
        CreatePlatform(area, "Platform_塔顶钟室_中央空中台_01", new Vector2(30, 102), new Vector2(6, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_塔顶钟室_左上回避台_01", new Vector2(20, 108), new Vector2(5, 0.9f), Colors.Mechanism);
        CreatePlatform(area, "Platform_塔顶钟室_右上回避台_01", new Vector2(40, 108), new Vector2(5, 0.9f), Colors.Mechanism);
        CreateDetailBlock(area, "Detail_塔顶钟室_巨大钟轮廓_01", new Vector2(30, 106), new Vector2(14, 13), Colors.DetailClock);
        CreateDetailBlock(area, "Detail_塔顶钟室_破碎钟摆_01", new Vector2(30, 101), new Vector2(1.5f, 9), Colors.DetailChain);
        CreateExitMarker(area, "Exit_塔顶钟室_返回主塔登顶", new Vector2(9, 97), "返回主塔登顶路线", ExitDirection.Left);
        CreatePortalPlaceholder(area, "Portal_塔顶钟室_结局出口", new Vector2(51, 97), "结束 CG 触发点");

        if (includeSavePoints)
            CreateSavePoint(area, "SavePoint_篝火_Boss前", new Vector2(9, 97));

        if (includeAshPlatforms)
        {
            CreateAshPlatform(area, "AshPlatform_Boss区域_01", new Vector2(24, 101), new Vector2(5, 0.7f));
            CreateAshPlatform(area, "AshPlatform_Boss区域_02", new Vector2(36, 101), new Vector2(5, 0.7f));
        }

        if (includeEnemies)
            CreateEnemyPlaceholder(area, "Boss_塔顶守护者", new Vector2(30, 98), true);

        if (includeEnemies)
            CreateEnemyWave(area, "塔顶钟室_Boss前残响", new Vector2(18, 96.2f), new Vector2(24, 103.2f), new Vector2(36, 103.2f), new Vector2(42, 96.2f));

        if (includeTriggers)
        {
            CreateNarrativeTrigger(area, "NarrativeTrigger_Boss前", new Vector2(12, 98), new Vector2(5, 4), true, false,
                "塔顶的钟室就在前方。",
                "如果钟声再次响起，灰烬会走向哪里？");
            CreateNarrativeTrigger(area, "NarrativeTrigger_Boss后_结束CG触发点", new Vector2(50, 98), new Vector2(5, 5), true, false,
                "最后一声钟响之后，灰烬没有落下。",
                "它回到了所有被遗忘的人手中。");
        }
    }

    /// <summary>
    /// 创建区域背景和边界墙，让每个房间/走廊不再只是零散平台。
    /// </summary>
    private void CreateAreaBlockout(Transform parent, string areaName, Vector2 center, Vector2 size, Color backgroundColor)
    {
        CreateBackgroundBlock(parent, "Blockout_" + areaName + "_背景色块", center, size, backgroundColor);

        float wallThickness = 1.5f;
        CreateWallBlock(parent, "Wall_" + areaName + "_左边界", center + new Vector2(-size.x * 0.5f, 0f), new Vector2(wallThickness, size.y), Colors.WallTower);
        CreateWallBlock(parent, "Wall_" + areaName + "_右边界", center + new Vector2(size.x * 0.5f, 0f), new Vector2(wallThickness, size.y), Colors.WallTower);
        CreateWallBlock(parent, "Wall_" + areaName + "_顶部边界", center + new Vector2(0f, size.y * 0.5f), new Vector2(size.x, wallThickness), Colors.WallTower);
        CreateDetailBlock(parent, "Corner_" + areaName + "_左上角加厚块", center + new Vector2(-size.x * 0.5f, size.y * 0.5f), new Vector2(2.4f, 2.4f), Colors.WallCorner);
        CreateDetailBlock(parent, "Corner_" + areaName + "_右上角加厚块", center + new Vector2(size.x * 0.5f, size.y * 0.5f), new Vector2(2.4f, 2.4f), Colors.WallCorner);
        CreateDetailBlock(parent, "Guide_" + areaName + "_出口层级提示线", center + new Vector2(0f, -size.y * 0.5f + 2.2f), new Vector2(size.x * 0.82f, 0.18f), Colors.ExitGuide);
    }

    /// <summary>
    /// 创建无碰撞背景块，用来表示房间体量和远景墙面。
    /// </summary>
    private GameObject CreateBackgroundBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject block = CreateColoredBox(parent, name, position, size, color, false, false);
        SetSorting(block, -20);
        counters.detailCount++;
        return block;
    }

    /// <summary>
    /// 创建有碰撞的墙体色块。
    /// </summary>
    private GameObject CreateWallBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject wall = CreateColoredBox(parent, name, position, size, color, true, false);
        wall.layer = GetLayer("Ground");
        AddMarker(wall, "墙体占位", "完整色块墙体，可后续替换为 Tilemap 墙面。", color, size, GeneratedMapMarker.MarkerShape.Box);
        counters.wallCount++;
        counters.platformCount++;
        return wall;
    }

    /// <summary>
    /// 创建无碰撞装饰色块，表达窗户、立柱、钟轮、裂缝等关卡细节。
    /// </summary>
    private GameObject CreateDetailBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject detail = CreateColoredBox(parent, name, position, size, color, false, false);
        SetSorting(detail, -10);
        AddMarker(detail, "装饰细节", "仅用于关卡原型视觉层次，不参与碰撞。", color, size, GeneratedMapMarker.MarkerShape.Box);
        counters.detailCount++;
        return detail;
    }

    /// <summary>
    /// 用规律化色块填充房间，避免大房间内部过空。
    /// 这些对象不参与碰撞，只负责表达墙面层次、背景结构和未来美术替换区域。
    /// </summary>
    private void CreateRoomFillDetails(Transform parent, string areaName, Vector2 center, Vector2 size, Color primaryColor, Color secondaryColor)
    {
        float left = center.x - size.x * 0.5f + 4f;
        float right = center.x + size.x * 0.5f - 4f;
        float bottom = center.y - size.y * 0.5f + 4f;
        float top = center.y + size.y * 0.5f - 4f;

        CreateDetailBlock(parent, "Fill_" + areaName + "_背景横向结构_01", new Vector2(center.x, bottom + size.y * 0.25f), new Vector2(size.x * 0.55f, 0.35f), primaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_背景横向结构_02", new Vector2(center.x, bottom + size.y * 0.5f), new Vector2(size.x * 0.68f, 0.28f), secondaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_背景横向结构_03", new Vector2(center.x, bottom + size.y * 0.72f), new Vector2(size.x * 0.48f, 0.28f), primaryColor);

        CreateDetailBlock(parent, "Fill_" + areaName + "_左墙分层_01", new Vector2(left, center.y), new Vector2(0.8f, size.y * 0.52f), secondaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_右墙分层_01", new Vector2(right, center.y), new Vector2(0.8f, size.y * 0.52f), secondaryColor);

        CreateDetailBlock(parent, "Fill_" + areaName + "_左上结构块_01", new Vector2(left + 3f, top - 2f), new Vector2(4f, 2f), primaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_右上结构块_01", new Vector2(right - 3f, top - 2f), new Vector2(4f, 2f), primaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_下层破损块_01", new Vector2(left + 5f, bottom), new Vector2(3.5f, 1.2f), secondaryColor);
        CreateDetailBlock(parent, "Fill_" + areaName + "_下层破损块_02", new Vector2(right - 5f, bottom + 1.5f), new Vector2(3.5f, 1.2f), secondaryColor);
    }

    /// <summary>
    /// 创建出口门框，占位阶段用于强调房间连接方向，不直接写传送逻辑。
    /// </summary>
    private GameObject CreateExitMarker(Transform parent, string name, Vector2 position, string targetName, ExitDirection direction)
    {
        if (!includePortals)
            return null;

        GameObject rootObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(rootObject, "Create Exit Marker");
        rootObject.transform.SetParent(parent);
        rootObject.transform.position = ScaleVector(position);

        Vector2 frameSize = direction == ExitDirection.Up || direction == ExitDirection.Down
            ? new Vector2(5.5f, 2.2f)
            : new Vector2(2.2f, 5.5f);

        GameObject trigger = CreateColoredBox(rootObject.transform, "ExitTrigger_" + targetName, Vector2.zero, frameSize, Colors.ExitTrigger, true, true, false);
        trigger.layer = GetLayer("Interact");
        AddMarker(trigger, "出口占位", "出口方向：" + direction + "，目标区域：" + targetName + "。可替换为传送门或场景切换触发器。", Colors.ExitTrigger, frameSize, GeneratedMapMarker.MarkerShape.Box);

        CreateDoorFrame(rootObject.transform, "DoorFrame_" + targetName, Vector2.zero, frameSize, Colors.ExitFrame, direction);
        CreateFloatingText(rootObject.transform, "Label_" + targetName, targetName, GetExitLabelOffset(direction), Color.white, 26, false);

        counters.exitCount++;
        return rootObject;
    }

    /// <summary>
    /// 创建传送门占位。项目中已有 Portal prefab 时，可后续手动替换这里的对象。
    /// </summary>
    private GameObject CreatePortalPlaceholder(Transform parent, string name, Vector2 position, string targetName)
    {
        if (!includePortals)
            return null;

        GameObject portalRoot = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(portalRoot, "Create Portal Placeholder");
        portalRoot.transform.SetParent(parent);
        portalRoot.transform.position = ScaleVector(position);

        GameObject trigger = CreateColoredBox(portalRoot.transform, "PortalTrigger_" + targetName, Vector2.zero, new Vector2(2.4f, 4.2f), Colors.PortalCore, true, true, false);
        trigger.layer = GetLayer("Interact");
        AddMarker(trigger, "传送门占位", "项目中已有 Assets/Prefab/Interact/Portal.prefab。正式使用时可把该占位替换成 Portal prefab，并手动设置 teleportTarget。", Colors.PortalCore, new Vector2(2.4f, 4.2f), GeneratedMapMarker.MarkerShape.Box);

        CreateColoredBox(portalRoot.transform, "PortalFrame_左柱", new Vector2(-1.5f, 0), new Vector2(0.35f, 4.8f), Colors.PortalFrame, false, false, false);
        CreateColoredBox(portalRoot.transform, "PortalFrame_右柱", new Vector2(1.5f, 0), new Vector2(0.35f, 4.8f), Colors.PortalFrame, false, false, false);
        CreateColoredBox(portalRoot.transform, "PortalFrame_顶部", new Vector2(0, 2.2f), new Vector2(3.3f, 0.35f), Colors.PortalFrame, false, false, false);
        GameObject glow = CreateColoredBox(portalRoot.transform, "PortalGlow_" + targetName, Vector2.zero, new Vector2(1.2f, 3.2f), Colors.PortalGlow, false, false, false);
        SetSorting(glow, -8);
        CreateFloatingText(portalRoot.transform, "Label_传送门_" + targetName, "传送门：" + targetName, new Vector2(0, 3.2f), Color.white, 26, false);

        counters.portalCount++;
        return portalRoot;
    }

    /// <summary>
    /// 使用小色块拼出门框，让出口比普通触发器更醒目。
    /// </summary>
    private void CreateDoorFrame(Transform parent, string name, Vector2 center, Vector2 size, Color color, ExitDirection direction)
    {
        Transform frameRoot = new GameObject(name).transform;
        Undo.RegisterCreatedObjectUndo(frameRoot.gameObject, "Create Door Frame");
        frameRoot.SetParent(parent);
        frameRoot.localPosition = Vector3.zero;

        if (direction == ExitDirection.Up || direction == ExitDirection.Down)
        {
            CreateColoredBox(frameRoot, "Frame_左边", center + new Vector2(-size.x * 0.5f, 0), new Vector2(0.3f, size.y), color, false, false, false);
            CreateColoredBox(frameRoot, "Frame_右边", center + new Vector2(size.x * 0.5f, 0), new Vector2(0.3f, size.y), color, false, false, false);
            CreateColoredBox(frameRoot, "Frame_横梁", center + new Vector2(0, direction == ExitDirection.Up ? size.y * 0.5f : -size.y * 0.5f), new Vector2(size.x, 0.3f), color, false, false, false);
        }
        else
        {
            CreateColoredBox(frameRoot, "Frame_上边", center + new Vector2(0, size.y * 0.5f), new Vector2(size.x, 0.3f), color, false, false, false);
            CreateColoredBox(frameRoot, "Frame_下边", center + new Vector2(0, -size.y * 0.5f), new Vector2(size.x, 0.3f), color, false, false, false);
            CreateColoredBox(frameRoot, "Frame_侧边", center + new Vector2(direction == ExitDirection.Right ? size.x * 0.5f : -size.x * 0.5f, 0), new Vector2(0.3f, size.y), color, false, false, false);
        }
    }

    private Vector2 GetExitLabelOffset(ExitDirection direction)
    {
        switch (direction)
        {
            case ExitDirection.Left:
                return new Vector2(-2.3f, 3.4f);
            case ExitDirection.Right:
                return new Vector2(2.3f, 3.4f);
            case ExitDirection.Up:
                return new Vector2(0f, 2.2f);
            default:
                return new Vector2(0f, -2.2f);
        }
    }

    /// <summary>
    /// 创建普通地面或平台，占位阶段统一用矩形和 BoxCollider2D。
    /// </summary>
    private GameObject CreatePlatform(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject platform = CreateColoredBox(parent, name, position, size, color, true, false);
        platform.layer = GetLayer("Ground");
        CreatePlatformTrim(parent, name, position, size, false);
        counters.platformCount++;
        return platform;
    }

    /// <summary>
    /// 创建灰烬态平台；如果项目中存在 AshenObject，就用反射安全挂载。
    /// </summary>
    private GameObject CreateAshPlatform(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject platform = CreateColoredBox(parent, name, position, size, Colors.AshPlatform, true, false);
        platform.layer = GetLayer("Ground");

        Type ashenObjectType = FindType("AshenObject");
        if (ashenObjectType != null)
            Undo.AddComponent(platform, ashenObjectType);
        else
            platform.name = name + "_需要手动挂载AshenObject";

        CreatePlatformTrim(parent, name, position, size, true);
        AddMarker(platform, "灰烬态平台", "灰烬态下显示的平台，占位阶段可替换为旧钟塔结构美术。", Colors.AshPlatform, size, GeneratedMapMarker.MarkerShape.Box);
        counters.ashPlatformCount++;
        return platform;
    }

    /// <summary>
    /// 给跳台加顶边、底部支撑和端点色块，方便看出可落脚范围。
    /// </summary>
    private void CreatePlatformTrim(Transform parent, string baseName, Vector2 position, Vector2 size, bool isAshen)
    {
        Color topColor = isAshen ? Colors.AshPlatformTop : Colors.PlatformTop;
        Color sideColor = isAshen ? Colors.AshPlatformEdge : Colors.PlatformEdge;

        CreateColoredBox(parent, "Trim_" + baseName + "_可站立顶边", position + new Vector2(0, size.y * 0.5f + 0.08f), new Vector2(size.x, 0.16f), topColor, false, false);
        CreateColoredBox(parent, "Trim_" + baseName + "_左端点", position + new Vector2(-size.x * 0.5f, 0), new Vector2(0.18f, size.y + 0.18f), sideColor, false, false);
        CreateColoredBox(parent, "Trim_" + baseName + "_右端点", position + new Vector2(size.x * 0.5f, 0), new Vector2(0.18f, size.y + 0.18f), sideColor, false, false);

        if (size.x >= 5f)
        {
            CreateColoredBox(parent, "Trim_" + baseName + "_底部支撑_01", position + new Vector2(-size.x * 0.25f, -size.y * 0.5f - 0.35f), new Vector2(0.25f, 0.7f), sideColor, false, false);
            CreateColoredBox(parent, "Trim_" + baseName + "_底部支撑_02", position + new Vector2(size.x * 0.25f, -size.y * 0.5f - 0.35f), new Vector2(0.25f, 0.7f), sideColor, false, false);
        }
    }

    /// <summary>
    /// 创建红色危险区域占位。
    /// </summary>
    private GameObject CreateDanger(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject danger = CreateColoredBox(parent, name, position, size, Colors.Danger, true, true);
        AddMarker(danger, "危险区域", "可替换为尖刺、熔灰池或齿轮陷阱。", Colors.Danger, size, GeneratedMapMarker.MarkerShape.Box);
        return danger;
    }

    /// <summary>
    /// 创建篝火存档点占位，不强行挂载 CheckPoint。
    /// </summary>
    private GameObject CreateSavePoint(Transform parent, string name, Vector2 position)
    {
        GameObject savePoint = CreateColoredBox(parent, name, position, new Vector2(1.5f, 2f), Colors.SavePoint, true, true);
        savePoint.layer = GetLayer("Interact");
        AddMarker(savePoint, "篝火存档点", "如果需要正式存档功能，请手动挂载 CheckPoint 并配置 id。", Colors.SavePoint, new Vector2(1.5f, 2f), GeneratedMapMarker.MarkerShape.Circle);
        counters.savePointCount++;
        return savePoint;
    }

    /// <summary>
    /// 创建剧情触发点；如果 NarrativeTrigger 存在，则自动挂载并写入文本。
    /// </summary>
    private GameObject CreateNarrativeTrigger(Transform parent, string name, Vector2 position, Vector2 size, bool requireInteract, bool requireAshenPhase, params string[] lines)
    {
        Type narrativeType = FindType("NarrativeTrigger");
        string objectName = narrativeType == null ? name + "_需要手动挂载" : name;
        GameObject triggerObject = CreateColoredBox(parent, objectName, position, size, Colors.Narrative, true, true);
        triggerObject.layer = GetLayer("Interact");

        if (narrativeType != null)
        {
            Component trigger = Undo.AddComponent(triggerObject, narrativeType);
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SetStringArray(serializedTrigger.FindProperty("dialogueLines"), lines);
            SetStringProperty(serializedTrigger, "triggerId", name);
            SetBoolProperty(serializedTrigger, "triggerOnce", !requireInteract);
            SetBoolProperty(serializedTrigger, "requireInteractKey", requireInteract);
            SetIntProperty(serializedTrigger, "interactKey", (int)KeyCode.E);
            SetBoolProperty(serializedTrigger, "requireAshenPhase", requireAshenPhase);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        }

        AddMarker(triggerObject, "剧情触发点", requireInteract ? "进入范围后按 E 交互触发。" : "进入范围后自动触发。", Colors.Narrative, size, GeneratedMapMarker.MarkerShape.Box);
        counters.narrativeCount++;
        return triggerObject;
    }

    /// <summary>
    /// 创建能力获得点，仅做地图占位，不写入真实解锁逻辑。
    /// </summary>
    private GameObject CreateAbilityPickup(Transform parent, string name, string abilityName, Vector2 position, Color color)
    {
        GameObject ability = CreateColoredBox(parent, name, position, new Vector2(1.5f, 1.5f), color, false, false);
        ability.layer = GetLayer("Interact");

        CircleCollider2D collider = ability.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.85f;

        AddMarker(ability, "能力获得点", "占位能力：" + abilityName + "。正式功能请手动接入 AbilityActivator 或你的能力系统。", color, new Vector2(1.8f, 1.8f), GeneratedMapMarker.MarkerShape.Circle);
        counters.abilityCount++;
        return ability;
    }

    /// <summary>
    /// 创建能力门占位，带半透明方块和文字标注。
    /// </summary>
    private GameObject CreateAbilityGate(Transform parent, string name, string requiredAbility, Vector2 position, Vector2 size, Color color)
    {
        if (!includeAbilityGates)
            return null;

        GameObject gate = CreateColoredBox(parent, name, position, size, color, true, true);
        gate.layer = GetLayer("Interact");

        GeneratedAbilityGate abilityGate = gate.AddComponent<GeneratedAbilityGate>();
        abilityGate.Setup(requiredAbility, "该门未来需要能力：" + requiredAbility);

        CreateFloatingText(gate.transform, "Label_" + requiredAbility, requiredAbility, position + new Vector2(0, size.y * 0.5f + 1f), Color.white, 32);
        AddMarker(gate, "能力门", "未来接入能力检测：" + requiredAbility, color, size, GeneratedMapMarker.MarkerShape.Box);
        counters.gateCount++;
        return gate;
    }

    /// <summary>
    /// 创建敌人占位，不强行挂载 AI。
    /// </summary>
    private GameObject CreateEnemyPlaceholder(Transform parent, string name, Vector2 position, bool eliteOrBoss)
    {
        GameObject enemy = CreateColoredBox(parent, name, position, eliteOrBoss ? new Vector2(3f, 3f) : new Vector2(1.6f, 1.8f), eliteOrBoss ? Colors.Boss : Colors.Enemy, true, true);
        enemy.layer = GetLayer("Enemy");
        AddMarker(enemy, eliteOrBoss ? "精英/Boss 占位" : "敌人占位", "正式敌人请后续替换为 Enemy prefab。", eliteOrBoss ? Colors.Boss : Colors.Enemy, eliteOrBoss ? new Vector2(3f, 3f) : new Vector2(1.6f, 1.8f), GeneratedMapMarker.MarkerShape.Box);
        counters.enemyCount++;
        return enemy;
    }

    /// <summary>
    /// 快速创建一组普通敌人，提高房间战斗密度。
    /// </summary>
    private void CreateEnemyWave(Transform parent, string waveName, params Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            string index = (i + 1).ToString("00");
            CreateEnemyPlaceholder(parent, "Enemy_" + waveName + "_" + index, positions[i], false);
        }
    }

    /// <summary>
    /// 创建区域文字标注，使用 Unity 内置 TextMesh，避免强依赖 TMP。
    /// </summary>
    private GameObject CreateAreaLabel(Transform parent, string label, Vector2 position)
    {
        GameObject labelObject = new GameObject("Label_" + label);
        Undo.RegisterCreatedObjectUndo(labelObject, "Create Area Label");
        labelObject.transform.SetParent(parent);
        labelObject.transform.position = ScaleVector(position);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        MapAreaLabel areaLabel = labelObject.AddComponent<MapAreaLabel>();
        areaLabel.Setup(label, Colors.Label, 46);
        textMesh.characterSize = 0.22f * MapScale;
        return labelObject;
    }

    /// <summary>
    /// 创建旧日长廊中的石碑群和中央赐福石碑。
    /// </summary>
    private void CreateMemorialHall(Transform area)
    {
        int stoneCount = 12;
        float startX = 36f;
        for (int i = 0; i < stoneCount; i++)
        {
            string index = (i + 1).ToString("00");
            Vector2 position = new Vector2(startX + i * 3.4f, 57.5f);
            GameObject stone = CreateColoredBox(area, "MemorialStone_旧日石碑_" + index, position, new Vector2(1.2f, 2.6f), Colors.MemorialStone, true, true);
            AddMarker(stone, "旧日石碑", "以后可填好友昵称、头像和寄语。", Colors.MemorialStone, new Vector2(1.2f, 2.6f), GeneratedMapMarker.MarkerShape.Box);
            counters.memorialStoneCount++;

            if (includeTriggers)
            {
                CreateNarrativeTrigger(area, "NarrativeTrigger_旧日石碑_" + index, position + new Vector2(0, 1.8f), new Vector2(2.4f, 2.4f), true, false,
                    "这块石碑还没有刻上名字。",
                    "也许它在等待一个值得被记住的人。");
            }
        }

        GameObject centerStone = CreateColoredBox(area, "MemorialStone_中央赐福石碑", new Vector2(55, 60.8f), new Vector2(4f, 5f), Colors.CentralMemorial, true, true);
        AddMarker(centerStone, "中央赐福石碑", "与旧日赐福能力点配套的核心剧情物。", Colors.CentralMemorial, new Vector2(4f, 5f), GeneratedMapMarker.MarkerShape.Box);
        counters.memorialStoneCount++;
    }

    /// <summary>
    /// 创建主塔登顶路线的连续灰烬态跳台。
    /// </summary>
    private void CreateTowerClimbRoute(Transform area)
    {
        Vector2[] positions =
        {
            new Vector2(35, 24),
            new Vector2(25, 31),
            new Vector2(36, 38),
            new Vector2(24, 45),
            new Vector2(34, 56),
            new Vector2(25, 64),
            new Vector2(36, 72),
            new Vector2(29, 80)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            CreateAshPlatform(area, "AshPlatform_主塔登顶路线_" + (i + 1).ToString("00"), positions[i], new Vector2(5f, 0.7f));
        }
    }

    /// <summary>
    /// 创建可视化矩形物体，可选择是否带 BoxCollider2D。
    /// </summary>
    private GameObject CreateColoredBox(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool addCollider, bool isTrigger, bool useMapSpace = true)
    {
        GameObject box = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(box, "Create Map Placeholder");
        box.transform.SetParent(parent);
        Vector2 scaledPosition = ScaleVector(position);
        Vector2 scaledSize = ScaleVector(size);
        if (useMapSpace)
            box.transform.position = scaledPosition;
        else
            box.transform.localPosition = scaledPosition;
        box.transform.localScale = new Vector3(scaledSize.x, scaledSize.y, 1f);

        SpriteRenderer renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = placeholderSprite;
        renderer.color = color;
        renderer.sortingLayerName = "Ground";
        renderer.sortingOrder = 5;

        AddPositionLabelIfNeeded(box.transform, name, position, size, useMapSpace);

        if (addCollider)
        {
            BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
            collider.isTrigger = isTrigger;
        }

        return box;
    }

    /// <summary>
    /// 给主要占位添加坐标/尺寸标注。坐标使用未乘以 MapScale 前的设计坐标，便于和生成器代码对应。
    /// </summary>
    private void AddPositionLabelIfNeeded(Transform parent, string objectName, Vector2 position, Vector2 size, bool useMapSpace)
    {
        if (!includePositionLabels || !ShouldShowPositionLabel(objectName))
            return;

        GameObject labelObject = new GameObject("CoordLabel_" + objectName);
        Undo.RegisterCreatedObjectUndo(labelObject, "Create Coordinate Label");
        labelObject.transform.SetParent(parent);
        labelObject.transform.localPosition = new Vector3(0f, 0.68f, 0f);
        labelObject.transform.localScale = GetInverseScale(parent);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = objectName + "\nP(" + FormatFloat(position.x) + ", " + FormatFloat(position.y) + ")  S(" + FormatFloat(size.x) + ", " + FormatFloat(size.y) + ")";
        textMesh.color = Colors.CoordinateLabel;
        textMesh.fontSize = 18;
        textMesh.characterSize = Mathf.Max(0.18f, 0.12f * MapScale);
        textMesh.anchor = TextAnchor.LowerCenter;
        textMesh.alignment = TextAlignment.Center;
    }

    private Vector3 GetInverseScale(Transform target)
    {
        Vector3 scale = target.localScale;
        return new Vector3(
            1f / Mathf.Max(Mathf.Abs(scale.x), 0.001f),
            1f / Mathf.Max(Mathf.Abs(scale.y), 0.001f),
            1f);
    }

    private bool ShouldShowPositionLabel(string objectName)
    {
        if (objectName.StartsWith("Trim_", StringComparison.Ordinal) ||
            objectName.StartsWith("Frame_", StringComparison.Ordinal) ||
            objectName.StartsWith("PortalFrame_", StringComparison.Ordinal) ||
            objectName.StartsWith("PortalGlow_", StringComparison.Ordinal) ||
            objectName.StartsWith("Blockout_", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private string FormatFloat(float value)
    {
        return value.ToString("0.#");
    }

    /// <summary>
    /// 创建跟随某个对象的文字标签。
    /// </summary>
    private GameObject CreateFloatingText(Transform parent, string name, string text, Vector2 position, Color color, int fontSize, bool useMapSpace = true)
    {
        GameObject label = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(label, "Create Floating Text");
        label.transform.SetParent(parent);
        if (useMapSpace)
            label.transform.position = ScaleVector(position);
        else
            label.transform.localPosition = ScaleVector(position);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = 0.18f * MapScale;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        return label;
    }

    /// <summary>
    /// 添加生成标记脚本，方便 Scene 视图查看占位类型和说明。
    /// </summary>
    private void AddMarker(GameObject target, string type, string note, Color color, Vector2 size, GeneratedMapMarker.MarkerShape shape)
    {
        GeneratedMapMarker marker = target.AddComponent<GeneratedMapMarker>();
        marker.Setup(type, note, color, ScaleVector(size), shape);
    }

    private Vector2 ScaleVector(Vector2 value)
    {
        return value * MapScale;
    }

    private void SetSorting(GameObject target, int sortingOrder)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingOrder = sortingOrder;
    }

    private int GetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        return layer >= 0 ? layer : 0;
    }

    /// <summary>
    /// 查找可选玩法脚本类型，避免 Editor 工具强依赖某个脚本。
    /// </summary>
    private Type FindType(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(typeName);
            if (type != null && typeof(Component).IsAssignableFrom(type))
                return type;

            Type[] types;
            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            for (int j = 0; j < types.Length; j++)
            {
                type = types[j];
                if (type != null && type.Name == typeName && typeof(Component).IsAssignableFrom(type))
                    return type;
            }
        }

        return null;
    }

    private Sprite LoadPlaceholderSprite()
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite != null)
            return sprite;

        return AssetDatabase.GetBuiltinExtraResource<Sprite>("Sprites/Default.psd");
    }

    private void SetStringArray(SerializedProperty property, string[] values)
    {
        if (property == null)
            return;

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        }
    }

    private void SetStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.stringValue = value;
    }

    private void SetBoolProperty(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private void SetIntProperty(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.intValue = value;
    }

    private void PrintSummary()
    {
        Debug.Log(
            "已生成《灰烬钟塔》完整地图蓝图：\n" +
            "- 区域数量：" + counters.areaCount + "\n" +
            "- 普通平台数量：" + counters.platformCount + "\n" +
            "- 灰烬态平台数量：" + counters.ashPlatformCount + "\n" +
            "- 墙体色块数量：" + counters.wallCount + "\n" +
            "- 背景/装饰色块数量：" + counters.detailCount + "\n" +
            "- 出口占位数量：" + counters.exitCount + "\n" +
            "- 传送门占位数量：" + counters.portalCount + "\n" +
            "- 剧情触发点数量：" + counters.narrativeCount + "\n" +
            "- 能力点数量：" + counters.abilityCount + "\n" +
            "- 能力门数量：" + counters.gateCount + "\n" +
            "- 敌人占位数量：" + counters.enemyCount + "\n" +
            "- 篝火存档点数量：" + counters.savePointCount + "\n" +
            "- 石碑数量：" + counters.memorialStoneCount);
    }

    private class MapCounters
    {
        public int areaCount;
        public int platformCount;
        public int ashPlatformCount;
        public int wallCount;
        public int detailCount;
        public int exitCount;
        public int portalCount;
        public int narrativeCount;
        public int abilityCount;
        public int gateCount;
        public int enemyCount;
        public int savePointCount;
        public int memorialStoneCount;
    }

    private static class Colors
    {
        public static readonly Color GroundHaunt = new Color(0.34f, 0.31f, 0.42f, 1f);
        public static readonly Color GroundTower = new Color(0.38f, 0.38f, 0.42f, 1f);
        public static readonly Color BackHaunt = new Color(0.16f, 0.14f, 0.21f, 0.75f);
        public static readonly Color BackTower = new Color(0.18f, 0.18f, 0.22f, 0.72f);
        public static readonly Color BackMechanism = new Color(0.16f, 0.16f, 0.26f, 0.75f);
        public static readonly Color BackFoundry = new Color(0.24f, 0.12f, 0.1f, 0.76f);
        public static readonly Color BackFrost = new Color(0.13f, 0.2f, 0.26f, 0.76f);
        public static readonly Color BackMemorial = new Color(0.2f, 0.18f, 0.24f, 0.78f);
        public static readonly Color BackBoss = new Color(0.2f, 0.12f, 0.14f, 0.8f);
        public static readonly Color WallTower = new Color(0.25f, 0.25f, 0.3f, 1f);
        public static readonly Color DetailHaunt = new Color(0.25f, 0.22f, 0.32f, 0.85f);
        public static readonly Color DetailStone = new Color(0.48f, 0.47f, 0.5f, 0.9f);
        public static readonly Color DetailAshGhost = new Color(1f, 0.62f, 0.16f, 0.24f);
        public static readonly Color DetailWindow = new Color(0.95f, 0.62f, 0.18f, 0.5f);
        public static readonly Color DetailClock = new Color(0.7f, 0.58f, 0.38f, 0.45f);
        public static readonly Color DetailMechanism = new Color(0.36f, 0.4f, 0.72f, 0.55f);
        public static readonly Color DetailChain = new Color(0.42f, 0.42f, 0.48f, 0.85f);
        public static readonly Color DetailForge = new Color(0.45f, 0.18f, 0.12f, 0.9f);
        public static readonly Color DetailLavaGlow = new Color(1f, 0.28f, 0.04f, 0.45f);
        public static readonly Color DetailFrost = new Color(0.62f, 0.9f, 1f, 0.42f);
        public static readonly Color DetailIcePillar = new Color(0.7f, 0.95f, 1f, 0.6f);
        public static readonly Color DetailMemorial = new Color(0.48f, 0.45f, 0.55f, 0.65f);
        public static readonly Color DetailCrack = new Color(0.08f, 0.07f, 0.08f, 0.75f);
        public static readonly Color ExitTrigger = new Color(0.1f, 0.95f, 0.65f, 0.28f);
        public static readonly Color ExitFrame = new Color(0.35f, 0.9f, 0.75f, 0.85f);
        public static readonly Color PortalCore = new Color(0.2f, 0.75f, 1f, 0.38f);
        public static readonly Color PortalFrame = new Color(0.62f, 0.85f, 1f, 0.9f);
        public static readonly Color PortalGlow = new Color(0.1f, 0.95f, 1f, 0.45f);
        public static readonly Color WallCorner = new Color(0.18f, 0.18f, 0.22f, 1f);
        public static readonly Color ExitGuide = new Color(0.25f, 0.75f, 0.65f, 0.42f);
        public static readonly Color CoordinateLabel = new Color(1f, 0.96f, 0.68f, 1f);
        public static readonly Color PlatformTop = new Color(0.68f, 0.68f, 0.72f, 1f);
        public static readonly Color PlatformEdge = new Color(0.22f, 0.22f, 0.27f, 1f);
        public static readonly Color AshPlatformTop = new Color(1f, 0.82f, 0.28f, 0.85f);
        public static readonly Color AshPlatformEdge = new Color(0.95f, 0.38f, 0.04f, 0.75f);
        public static readonly Color AshPlatform = new Color(1f, 0.58f, 0.08f, 0.55f);
        public static readonly Color Mechanism = new Color(0.26f, 0.3f, 0.85f, 0.75f);
        public static readonly Color Danger = new Color(1f, 0.1f, 0.05f, 0.45f);
        public static readonly Color SavePoint = new Color(1f, 0.45f, 0.05f, 0.9f);
        public static readonly Color Narrative = new Color(0.35f, 0.9f, 1f, 0.28f);
        public static readonly Color AbilityAsh = new Color(1f, 0.5f, 0.05f, 0.95f);
        public static readonly Color AbilitySword = new Color(0.62f, 0.85f, 1f, 0.95f);
        public static readonly Color AbilityDash = new Color(0.1f, 0.95f, 1f, 0.95f);
        public static readonly Color AbilityFire = new Color(1f, 0.18f, 0.04f, 0.95f);
        public static readonly Color AbilityIce = new Color(0.45f, 0.9f, 1f, 0.95f);
        public static readonly Color AbilityBlessing = new Color(1f, 0.82f, 0.18f, 0.95f);
        public static readonly Color GateAsh = new Color(1f, 0.58f, 0.05f, 0.38f);
        public static readonly Color GateSword = new Color(0.5f, 0.8f, 1f, 0.38f);
        public static readonly Color GateDash = new Color(0.1f, 0.95f, 1f, 0.38f);
        public static readonly Color GateFire = new Color(1f, 0.2f, 0.02f, 0.38f);
        public static readonly Color GateIce = new Color(0.35f, 0.9f, 1f, 0.38f);
        public static readonly Color GateBlessing = new Color(1f, 0.85f, 0.2f, 0.38f);
        public static readonly Color Enemy = new Color(0.95f, 0.08f, 0.08f, 0.85f);
        public static readonly Color Boss = new Color(0.6f, 0.02f, 0.02f, 0.95f);
        public static readonly Color MemorialStone = new Color(0.55f, 0.58f, 0.65f, 0.9f);
        public static readonly Color CentralMemorial = new Color(0.95f, 0.82f, 0.38f, 0.9f);
        public static readonly Color Label = new Color(0.95f, 0.9f, 0.75f, 1f);
    }
}
