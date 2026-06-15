using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 《灰烬钟塔》房间式地图蓝图生成器。
/// 该工具生成的是关卡设计蓝图，不是 Tilemap，也不是最终可玩地图。
/// </summary>
public class AshenRoomBlueprintWindow : EditorWindow
{
    private const string RootName = "Generated_RoomBlueprintRoot";
    private const string ExportFolder = "Assets/Generated/RoomBlueprint";
    private const float RoomSpacingX = 36f;
    private const float RoomSpacingY = 24f;
    private const int Columns = 3;

    private readonly Color boundColor = new Color(0.18f, 0.2f, 0.22f, 0.85f);
    private readonly Color terrainColor = new Color(0.28f, 0.28f, 0.3f, 0.9f);
    private readonly Color platformColor = new Color(0.55f, 0.55f, 0.58f, 0.9f);
    private readonly Color ashPlatformColor = new Color(1f, 0.55f, 0.08f, 0.55f);
    private readonly Color portalColor = new Color(0.15f, 0.85f, 1f, 0.65f);
    private readonly Color enemyColor = new Color(0.95f, 0.1f, 0.1f, 0.85f);
    private readonly Color triggerColor = new Color(0.1f, 0.95f, 0.35f, 0.75f);
    private readonly Color abilityColor = new Color(1f, 0.82f, 0.08f, 0.9f);
    private readonly Color mechanismColor = new Color(0.65f, 0.25f, 1f, 0.7f);
    private readonly Color saveColor = new Color(1f, 0.55f, 0.05f, 0.85f);
    private readonly Color routeColor = new Color(0.8f, 0.95f, 1f, 0.45f);

    private bool includeLabels = true;
    private bool includeFunctionNotes = true;
    private bool includeRouteGuides = true;
    private int selectedRoomIndex;
    private Vector2 scroll;

    private List<RoomBlueprintDefinition> rooms;

    [MenuItem("Tools/灰烬钟塔/房间蓝图生成器")]
    public static void OpenWindow()
    {
        GetWindow<AshenRoomBlueprintWindow>("房间蓝图生成器");
    }

    private void OnEnable()
    {
        rooms = BuildRoomDefinitions();
    }

    private void OnGUI()
    {
        if (rooms == null || rooms.Count == 0)
            rooms = BuildRoomDefinitions();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("《灰烬钟塔》房间式地图蓝图", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("该工具只生成独立房间蓝图：房间边界、地形、平台、机关、敌人、能力点、触发点和传送门。不会生成 Tilemap，也不会修改核心玩法脚本。", MessageType.Info);

        includeLabels = EditorGUILayout.Toggle("显示房间标签", includeLabels);
        includeFunctionNotes = EditorGUILayout.Toggle("显示功能标注", includeFunctionNotes);
        includeRouteGuides = EditorGUILayout.Toggle("显示推荐路线", includeRouteGuides);

        string[] roomNames = new string[rooms.Count];
        for (int i = 0; i < rooms.Count; i++)
            roomNames[i] = rooms[i].name;

        selectedRoomIndex = EditorGUILayout.Popup("选定房间", selectedRoomIndex, roomNames);

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("生成全部房间蓝图", GUILayout.Height(28f)))
            GenerateAllRoomBlueprints(true);

        if (GUILayout.Button("只生成选定房间蓝图", GUILayout.Height(24f)))
            GenerateSelectedRoomBlueprint();

        if (GUILayout.Button("重新生成房间蓝图", GUILayout.Height(24f)))
            GenerateAllRoomBlueprints(true);

        if (GUILayout.Button("删除全部房间蓝图", GUILayout.Height(24f)))
            ClearGeneratedBlueprints(true);

        if (GUILayout.Button("导出房间蓝图说明文本", GUILayout.Height(24f)))
            ExportRoomBlueprintText();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("生成结果根物体", RootName);
        EditorGUILayout.LabelField("说明文本路径", ExportFolder + "/灰烬钟塔_房间蓝图说明.txt");
        EditorGUILayout.EndScrollView();
    }

    private void GenerateAllRoomBlueprints(bool askBeforeReplace)
    {
        if (!ClearGeneratedBlueprints(askBeforeReplace))
            return;

        GameObject rootObject = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootObject, "Generate Room Blueprints");

        for (int i = 0; i < rooms.Count; i++)
            GenerateRoomBlueprint(rootObject.transform, rooms[i], GetRoomGridPosition(i));

        ExportRoomBlueprintText();
        Selection.activeGameObject = rootObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("已生成《灰烬钟塔》房间式地图蓝图，共 " + rooms.Count + " 个房间。");
    }

    private void GenerateSelectedRoomBlueprint()
    {
        GameObject rootObject = GameObject.Find(RootName);
        if (rootObject == null)
        {
            rootObject = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Room Blueprint Root");
        }

        RoomBlueprintDefinition room = rooms[selectedRoomIndex];
        Transform oldRoom = rootObject.transform.Find(room.name);
        if (oldRoom != null)
            Undo.DestroyObjectImmediate(oldRoom.gameObject);

        GenerateRoomBlueprint(rootObject.transform, room, Vector2.zero);
        ExportRoomBlueprintText();
        Selection.activeGameObject = rootObject.transform.Find(room.name).gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("已生成选定房间蓝图：" + room.name);
    }

    private bool ClearGeneratedBlueprints(bool askBeforeDelete)
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot == null)
            return true;

        if (askBeforeDelete)
        {
            bool shouldDelete = EditorUtility.DisplayDialog(
                "删除旧房间蓝图",
                "场景中已经存在 " + RootName + "。是否删除旧蓝图并继续？",
                "删除并继续",
                "取消");

            if (!shouldDelete)
                return false;
        }

        Undo.DestroyObjectImmediate(oldRoot);
        return true;
    }

    private Vector2 GetRoomGridPosition(int index)
    {
        int column = index % Columns;
        int row = index / Columns;
        return new Vector2(column * RoomSpacingX, -row * RoomSpacingY);
    }

    private void GenerateRoomBlueprint(Transform root, RoomBlueprintDefinition room, Vector2 position)
    {
        GameObject roomObject = new GameObject(room.name);
        Undo.RegisterCreatedObjectUndo(roomObject, "Create Room Blueprint");
        roomObject.transform.SetParent(root);
        roomObject.transform.position = position;

        RoomContext context = new RoomContext
        {
            room = room,
            root = roomObject.transform,
            bounds = CreateGroup(roomObject.transform, "Bounds"),
            terrain = CreateGroup(roomObject.transform, "Terrain"),
            platforms = CreateGroup(roomObject.transform, "Platforms"),
            ashPlatforms = CreateGroup(roomObject.transform, "AshPlatforms"),
            portals = CreateGroup(roomObject.transform, "Portals"),
            enemies = CreateGroup(roomObject.transform, "Enemies"),
            triggers = CreateGroup(roomObject.transform, "Triggers"),
            markers = CreateGroup(roomObject.transform, "Markers"),
            labels = CreateGroup(roomObject.transform, "Labels"),
            notes = CreateGroup(roomObject.transform, "Notes")
        };

        CreateRoomBounds(context);
        CreateRoomTerrain(context);
        CreateRoomPlatforms(context);
        CreateRoomPortals(context);
        CreateRoomMarkers(context);
        CreateRoomLabel(context);
    }

    private Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, "Create Room Group");
        group.transform.SetParent(parent);
        group.transform.localPosition = Vector3.zero;
        return group.transform;
    }

    private void CreateRoomBounds(RoomContext context)
    {
        RoomBlueprintDefinition room = context.room;
        CreateBox(context.bounds, "Bounds_房间范围底色", Vector2.zero, room.size, new Color(0.05f, 0.07f, 0.09f, 0.22f), 0);
        CreateBox(context.bounds, "Bounds_LeftWall", new Vector2(-room.size.x * 0.5f, 0f), new Vector2(0.35f, room.size.y), boundColor, 2);
        CreateBox(context.bounds, "Bounds_RightWall", new Vector2(room.size.x * 0.5f, 0f), new Vector2(0.35f, room.size.y), boundColor, 2);
        CreateBox(context.bounds, "Bounds_FloorLine", new Vector2(0f, -room.size.y * 0.5f), new Vector2(room.size.x, 0.35f), boundColor, 2);
        CreateBox(context.bounds, "Bounds_CeilingLine", new Vector2(0f, room.size.y * 0.5f), new Vector2(room.size.x, 0.25f), boundColor, 2);
    }

    private void CreateRoomTerrain(RoomContext context)
    {
        RoomBlueprintDefinition room = context.room;
        foreach (BlueprintElement element in room.terrain)
            CreateBox(context.terrain, element.name, element.position, element.size, terrainColor, 4);
    }

    private void CreateRoomPlatforms(RoomContext context)
    {
        RoomBlueprintDefinition room = context.room;
        foreach (BlueprintElement element in room.platforms)
            CreateBox(context.platforms, element.name, element.position, element.size, platformColor, 5);

        foreach (BlueprintElement element in room.ashPlatforms)
            CreateBox(context.ashPlatforms, element.name, element.position, element.size, ashPlatformColor, 6);
    }

    private void CreateRoomPortals(RoomContext context)
    {
        RoomBlueprintDefinition room = context.room;
        foreach (PortalDefinition portal in room.portals)
        {
            GameObject portalObject = CreateBox(context.portals, portal.name, portal.position, new Vector2(1.4f, 4f), portalColor, 10);
            CreateBox(portalObject.transform, "Portal_Frame_Left", new Vector2(-0.85f, 0f), new Vector2(0.18f, 4.3f), new Color(0.55f, 0.2f, 1f, 0.75f), 11);
            CreateBox(portalObject.transform, "Portal_Frame_Right", new Vector2(0.85f, 0f), new Vector2(0.18f, 4.3f), new Color(0.55f, 0.2f, 1f, 0.75f), 11);
            CreateBox(portalObject.transform, "Portal_Frame_Top", new Vector2(0f, 2.1f), new Vector2(1.9f, 0.18f), new Color(0.55f, 0.2f, 1f, 0.75f), 11);

            if (includeLabels)
                CreateText(context.labels, "Label_" + portal.name, portal.position + Vector2.up * 3f, portal.name + "\n目标：" + portal.targetRoom, 0.14f, TextAnchor.MiddleCenter);
        }
    }

    private void CreateRoomMarkers(RoomContext context)
    {
        RoomBlueprintDefinition room = context.room;
        foreach (BlueprintElement enemy in room.enemies)
        {
            CreateBox(context.enemies, enemy.name, enemy.position, enemy.size, enemyColor, 8);
            if (includeLabels)
                CreateText(context.labels, "Label_" + enemy.name, enemy.position + Vector2.up * 1.2f, enemy.name, 0.11f, TextAnchor.MiddleCenter);
        }

        foreach (BlueprintElement marker in room.markers)
        {
            Color color = GetMarkerColor(marker.kind);
            Transform parent = GetMarkerParent(context, marker.kind);
            CreateBox(parent, marker.name, marker.position, marker.size, color, 9);
            if (includeLabels)
                CreateText(context.labels, "Label_" + marker.name, marker.position + Vector2.up * 1.2f, marker.name, 0.11f, TextAnchor.MiddleCenter);
        }

        if (includeRouteGuides)
        {
            foreach (BlueprintElement route in room.routes)
                CreateBox(context.markers, route.name, route.position, route.size, routeColor, 7);
        }

        if (includeFunctionNotes)
            CreateText(context.notes, "Note_" + room.name, new Vector2(0f, -room.size.y * 0.5f - 2.2f), room.purpose + "\n教学重点：" + room.teachingFocus, 0.14f, TextAnchor.UpperCenter);
    }

    private void CreateRoomLabel(RoomContext context)
    {
        if (!includeLabels)
            return;

        RoomBlueprintDefinition room = context.room;
        CreateText(context.labels, "Title_" + room.name, new Vector2(0f, room.size.y * 0.5f + 2f), room.name, 0.22f, TextAnchor.MiddleCenter);
    }

    private Transform GetMarkerParent(RoomContext context, string kind)
    {
        switch (kind)
        {
            case "Ability":
            case "Save":
            case "Mechanism":
            case "Gate":
                return context.markers;
            case "Trigger":
                return context.triggers;
            default:
                return context.markers;
        }
    }

    private Color GetMarkerColor(string kind)
    {
        switch (kind)
        {
            case "Ability":
                return abilityColor;
            case "Save":
                return saveColor;
            case "Mechanism":
            case "Gate":
                return mechanismColor;
            case "Trigger":
                return triggerColor;
            default:
                return new Color(0.9f, 0.9f, 0.9f, 0.8f);
        }
    }

    private GameObject CreateBox(Transform parent, string name, Vector2 localPosition, Vector2 size, Color color, int sortingOrder)
    {
        GameObject box = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(box, "Create Blueprint Box");
        box.transform.SetParent(parent);
        box.transform.localPosition = localPosition;
        box.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return box;
    }

    private void CreateText(Transform parent, string name, Vector2 localPosition, string text, float size, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(textObject, "Create Blueprint Text");
        textObject.transform.SetParent(parent);
        textObject.transform.localPosition = localPosition;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = new Color(0.95f, 0.9f, 0.72f, 1f);
        textMesh.fontSize = 32;
        textMesh.characterSize = size;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
    }

    private void ExportRoomBlueprintText()
    {
        if (!Directory.Exists(ExportFolder))
            Directory.CreateDirectory(ExportFolder);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("《灰烬钟塔》房间式地图蓝图说明");
        builder.AppendLine("说明：该文档描述每个独立房间的功能、结构、点位和传送门连接。");
        builder.AppendLine("注意：这些房间通过 Portal_In / Portal_Out 连接，不通过连续地形直接连接。");
        builder.AppendLine();

        foreach (RoomBlueprintDefinition room in rooms)
        {
            builder.AppendLine("[" + room.name + "]");
            builder.AppendLine("功能：" + room.purpose);
            builder.AppendLine("结构：" + JoinElementNames(room.terrain, room.platforms, room.ashPlatforms));
            builder.AppendLine("点位：" + JoinElementNames(room.markers, room.enemies));
            builder.AppendLine("入口传送门：" + GetPortalList(room, true));
            builder.AppendLine("出口传送门：" + GetPortalList(room, false));
            builder.AppendLine("教学重点：" + room.teachingFocus);
            builder.AppendLine("备注：" + room.notes);
            builder.AppendLine();
        }

        string path = Path.Combine(ExportFolder, "灰烬钟塔_房间蓝图说明.txt");
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log("已导出房间蓝图说明文本：" + path);
    }

    private string JoinElementNames(params List<BlueprintElement>[] lists)
    {
        List<string> names = new List<string>();
        foreach (List<BlueprintElement> list in lists)
        {
            foreach (BlueprintElement element in list)
                names.Add(element.name);
        }

        return names.Count == 0 ? "无" : string.Join("、", names.ToArray());
    }

    private string GetPortalList(RoomBlueprintDefinition room, bool input)
    {
        List<string> values = new List<string>();
        foreach (PortalDefinition portal in room.portals)
        {
            if (portal.isInput == input)
                values.Add(portal.name + " -> " + portal.targetRoom);
        }

        return values.Count == 0 ? "无" : string.Join("；", values.ToArray());
    }

    private List<RoomBlueprintDefinition> BuildRoomDefinitions()
    {
        List<RoomBlueprintDefinition> result = new List<RoomBlueprintDefinition>();
        AddRoom(result, "Room_01_开场落点", "开场动画结束后玩家落入游戏的第一个房间。", "认识起点和前进方向。", "右侧传送门进入战斗教学。", RoomType.Start, "Room_02_荒凉小径_战斗教学");
        AddRoom(result, "Room_02_荒凉小径_战斗教学", "移动、跳跃、攻击和小怪教学。", "通过低平台和小怪建立基本操作。", "右侧通往篝火休整区。", RoomType.CombatIntro, "Room_03_篝火休整区");
        AddRoom(result, "Room_03_篝火休整区", "篝火/存档点教学。", "让玩家看到安全空间和存档点。", "左入口，右出口。", RoomType.SaveRoom, "Room_04_断桥教学区");
        AddRoom(result, "Room_04_断桥教学区", "第一次遇到无法通过的断桥。", "提示需要灰烬态。", "出口暂时被能力门标注。", RoomType.BrokenBridge, "Room_05_灰烬态通桥区");
        AddRoom(result, "Room_05_灰烬态通桥区", "第一次通过灰烬态桥梁。", "按 R 切相后通过灰烬平台。", "右侧进入钟塔前庭。", RoomType.AshenBridge, "Room_06_钟塔前庭_投剑战斗区");
        AddRoom(result, "Room_06_钟塔前庭_投剑战斗区", "进入钟塔前的战斗区。", "普通敌人和精英敌人战斗。", "右上门通往投掷宝剑教学。", RoomType.Courtyard, "Room_07_投掷宝剑教学区");
        AddRoom(result, "Room_07_投掷宝剑教学区", "获得投掷宝剑并立即教学。", "先战斗，再拿能力，再用投掷宝剑打远程机关开门。", "能力点不是孤立奖励，位于房间中后段。", RoomType.SwordAbility, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(result, "Room_08_钟塔大厅_Hub_主塔攀登一体房", "主枢纽房间，同时包含完整主塔竖向攀登路线。", "未获得旧日赐福前只能预览登顶路线；获得旧日赐福后从大厅内部直接完成主塔攀登。", "大厅底部连接三个分支，竖井上方通往 Boss 前休整室。", RoomType.Hub, "");
        AddRoom(result, "Room_10_齿轮廊道_冲刺前段", "进入冲刺区域前的普通挑战。", "齿轮平台、敌人和危险预告。", "右出口进入冲刺获取区。", RoomType.GearIntro, "Room_11_齿轮廊道_冲刺获取区");
        AddRoom(result, "Room_11_齿轮廊道_冲刺获取区", "获得冲刺。", "先平台机关，再拿冲刺，后半段用冲刺跨宽裂隙。", "能力点嵌在完整房间路线中。", RoomType.DashAbility, "Room_12_齿轮廊道_冲刺教学区");
        AddRoom(result, "Room_12_齿轮廊道_冲刺教学区", "连续使用冲刺的教学房间。", "宽裂隙、小平台、危险区域和敌人。", "离开后返回大厅或进入后续分支。", RoomType.DashPractice, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(result, "Room_13_熔灰工坊_火魔法前段", "火魔法区域前段。", "熔炉地形、平台、敌人和火炬机关预告。", "右出口进入火魔法获取区。", RoomType.FireIntro, "Room_14_熔灰工坊_火魔法获取区");
        AddRoom(result, "Room_14_熔灰工坊_火魔法获取区", "获得火之魔法。", "先通过工坊平台，再获得火魔法，再点燃火炬机关开门。", "能力点在中后段。", RoomType.FireAbility, "Room_15_熔灰工坊_火魔法教学区");
        AddRoom(result, "Room_15_熔灰工坊_火魔法教学区", "使用火之魔法解决机关。", "多个火炬机关和敌人。", "出口回到大厅。", RoomType.FirePractice, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(result, "Room_16_静霜钟室_冰魔法前段", "冰魔法区域前段。", "平台、水面预告和敌人。", "右出口进入冰魔法获取区。", RoomType.IceIntro, "Room_17_静霜钟室_冰魔法获取区");
        AddRoom(result, "Room_17_静霜钟室_冰魔法获取区", "获得冰之魔法。", "先普通平台，再获得冰魔法，再冻结水面/暂停机关通过。", "能力点在中后段。", RoomType.IceAbility, "Room_18_静霜钟室_冰魔法教学区");
        AddRoom(result, "Room_18_静霜钟室_冰魔法教学区", "使用冰之魔法解决房间挑战。", "冻结机关、高台和出口门。", "出口回到大厅或纪念堂入口。", RoomType.IcePractice, "Room_19_古老纪念堂入口");
        AddRoom(result, "Room_19_古老纪念堂入口", "进入纪念堂前的过渡房间。", "安静空间和文本提示。", "右出口进入旧日长廊。", RoomType.MemorialEntrance, "Room_20_旧日长廊");
        AddRoom(result, "Room_20_旧日长廊", "摆放好友石碑、头像和寄语的主长廊。", "多个可交互石碑。", "通往中央赐福石碑室。", RoomType.MemorialHall, "Room_21_中央赐福石碑室");
        AddRoom(result, "Room_21_中央赐福石碑室", "获得旧日赐福。", "先穿过长廊，再与中央石碑交互，获得灰烬态不再消耗灰烬值。", "出口传送回钟塔大厅，随后在同一个大厅竖井内完成主塔攀登。", RoomType.BlessingAbility, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(result, "Room_25_Boss前休整室", "Boss 前准备房间。", "篝火、安全地面和少量文本。", "右出口进入 Boss 房。", RoomType.BossRest, "Room_26_塔顶钟室_Boss");
        AddRoom(result, "Room_26_塔顶钟室_Boss", "最终 Boss 战。", "主战斗平台、两侧高台、Boss 点和灰烬态平台。", "Boss 后出口通往结局 CG。", RoomType.Boss, "Room_27_结局CG触发室");
        AddRoom(result, "Room_27_结局CG触发室", "Boss 战后进入结局 CG。", "简单地面、结局触发点和文字说明。", "流程结束。", RoomType.Ending, "");
        return result;
    }

    private void AddRoom(List<RoomBlueprintDefinition> list, string name, string purpose, string teachingFocus, string notes, RoomType type, string nextRoom)
    {
        RoomBlueprintDefinition room = new RoomBlueprintDefinition
        {
            name = name,
            purpose = purpose,
            teachingFocus = teachingFocus,
            notes = notes,
            type = type,
            size = GetRoomSize(type)
        };

        PopulateRoom(room, nextRoom);
        list.Add(room);
    }

    private Vector2 GetRoomSize(RoomType type)
    {
        switch (type)
        {
            case RoomType.Hub:
                return new Vector2(34f, 72f);
            case RoomType.MemorialHall:
                return new Vector2(32f, 16f);
            case RoomType.Boss:
                return new Vector2(30f, 18f);
            case RoomType.ClimbLower:
            case RoomType.ClimbMiddle:
            case RoomType.ClimbUpper:
                return new Vector2(24f, 20f);
            default:
                return new Vector2(26f, 15f);
        }
    }

    private void PopulateRoom(RoomBlueprintDefinition room, string nextRoom)
    {
        AddMainGround(room);
        AddDefaultPortals(room, nextRoom);

        switch (room.type)
        {
            case RoomType.Start:
                AddMarker(room, "Narrative_开场残响", "Trigger", new Vector2(-4f, -4f), new Vector2(1.2f, 1.2f));
                AddRoute(room, -7f, -3.7f, 10f);
                break;
            case RoomType.CombatIntro:
                AddPlatform(room, -4f, -1f, 5f);
                AddPlatform(room, 4f, 1.8f, 4f);
                AddEnemy(room, -1f, -4f);
                AddEnemy(room, 4f, -4f);
                AddEnemy(room, 8f, -4f);
                AddMarker(room, "Narrative_攻击教学", "Trigger", new Vector2(-8f, -4f), new Vector2(1.2f, 1.2f));
                AddRoute(room, -8f, -3.6f, 16f);
                break;
            case RoomType.SaveRoom:
                AddMarker(room, "SavePoint_篝火", "Save", new Vector2(-1f, -4f), new Vector2(1.4f, 1.8f));
                AddMarker(room, "Narrative_篝火提示", "Trigger", new Vector2(3f, -4f), new Vector2(1.2f, 1.2f));
                break;
            case RoomType.BrokenBridge:
                room.terrain.Clear();
                AddTerrain(room, "Terrain_左侧断桥地面", new Vector2(-7f, -6f), new Vector2(9f, 1.2f));
                AddTerrain(room, "Terrain_右侧断桥地面", new Vector2(7f, -6f), new Vector2(9f, 1.2f));
                AddMarker(room, "Gate_需要灰烬态", "Gate", new Vector2(0f, -4.8f), new Vector2(5f, 1.2f));
                AddMarker(room, "Interact_断桥石碑", "Trigger", new Vector2(-3f, -4.8f), new Vector2(1.2f, 1.2f));
                break;
            case RoomType.AshenBridge:
                AddAshPlatform(room, -3f, -2.5f, 5f);
                AddAshPlatform(room, 3f, -1.2f, 5f);
                AddMarker(room, "Narrative_按R切相", "Trigger", new Vector2(-7f, -4f), new Vector2(1.2f, 1.2f));
                AddRoute(room, -6f, -2.5f, 12f);
                break;
            case RoomType.Courtyard:
                AddPlatform(room, -5f, -1f, 5f);
                AddPlatform(room, 3f, 1f, 5f);
                AddPlatform(room, 8f, 3.3f, 4f);
                AddEnemy(room, -4f, -4f);
                AddEnemy(room, 1f, -4f);
                AddEnemy(room, 5f, -4f);
                AddEnemy(room, 8f, 2.6f, "Enemy_精英守门者");
                break;
            case RoomType.SwordAbility:
                AddPlatform(room, -5f, -1f, 4f);
                AddPlatform(room, 1f, 0.6f, 4f);
                AddEnemy(room, -3f, -4f);
                AddMarker(room, "Ability_投掷宝剑", "Ability", new Vector2(2f, -0.3f), new Vector2(1.4f, 1.4f));
                AddMarker(room, "Mechanism_远程机关", "Mechanism", new Vector2(7f, 2f), new Vector2(1.4f, 1.4f));
                AddMarker(room, "Gate_投剑开启出口", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                AddRoute(room, -8f, -3.6f, 17f);
                break;
            case RoomType.Hub:
                AddPlatform(room, -8f, -1f, 5f);
                AddPlatform(room, 0f, 2f, 6f);
                AddPlatform(room, 8f, -1f, 5f);
                AddAshPlatform(room, 0f, 5f, 4f);
                AddMarker(room, "SavePoint_大厅篝火", "Save", new Vector2(-7f, -5f), new Vector2(1.4f, 1.8f));
                AddMarker(room, "Narrative_大厅说明", "Trigger", new Vector2(0f, -5f), new Vector2(1.2f, 1.2f));
                AddPortal(room, "Portal_Out_To_Room_10_齿轮廊道", new Vector2(-10f, 3.5f), "Room_10_齿轮廊道_冲刺前段", false);
                AddPortal(room, "Portal_Out_To_Room_13_熔灰工坊", new Vector2(0f, 6f), "Room_13_熔灰工坊_火魔法前段", false);
                AddPortal(room, "Portal_Out_To_Room_16_静霜钟室", new Vector2(10f, 3.5f), "Room_16_静霜钟室_冰魔法前段", false);
                AddMarker(room, "Gate_需要旧日赐福_开启主塔攀登", "Gate", new Vector2(0f, 12f), new Vector2(5f, 1.2f));
                AddAshPlatform(room, -3f, 17f, 4f);
                AddAshPlatform(room, 5f, 22f, 4f);
                AddPlatform(room, -6f, 27f, 5f);
                AddAshPlatform(room, 2f, 32f, 4f);
                AddAshPlatform(room, -7f, 37f, 4f);
                AddPlatform(room, 6f, 42f, 5f);
                AddAshPlatform(room, -2f, 47f, 4f);
                AddAshPlatform(room, 7f, 52f, 4f);
                AddPlatform(room, -6f, 57f, 5f);
                AddAshPlatform(room, 1f, 62f, 4f);
                AddEnemy(room, -8f, 18f);
                AddEnemy(room, 8f, 33f);
                AddEnemy(room, -7f, 49f);
                AddMarker(room, "Mechanism_主塔火冰复合机关", "Mechanism", new Vector2(8f, 44f), new Vector2(2f, 2f));
                AddPortal(room, "Portal_Out_To_Room_25_Boss前休整室", new Vector2(0f, 67f), "Room_25_Boss前休整室", false);
                break;
            case RoomType.ClimbPreview:
                AddAshPlatform(room, -5f, -2f, 4f);
                AddAshPlatform(room, 1f, 1f, 4f);
                AddAshPlatform(room, 7f, 4f, 4f);
                AddMarker(room, "Narrative_灰烬时间不足", "Trigger", new Vector2(0f, -4f), new Vector2(1.2f, 1.2f));
                break;
            case RoomType.GearIntro:
                AddPlatform(room, -6f, -1.5f, 5f);
                AddPlatform(room, 1f, 1f, 4f);
                AddMarker(room, "Mechanism_齿轮机关预告", "Mechanism", new Vector2(5f, -4f), new Vector2(2f, 2f));
                AddEnemy(room, -2f, -4f);
                AddEnemy(room, 6f, -4f);
                break;
            case RoomType.DashAbility:
                AddPlatform(room, -5f, -1f, 4f);
                AddPlatform(room, 0f, 1f, 4f);
                AddMarker(room, "Ability_冲刺", "Ability", new Vector2(1f, 0.1f), new Vector2(1.4f, 1.4f));
                AddMarker(room, "Mechanism_宽裂隙", "Mechanism", new Vector2(5f, -5.2f), new Vector2(5f, 0.8f));
                AddMarker(room, "Gate_冲刺通过出口", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                AddRoute(room, -8f, -3.6f, 17f);
                break;
            case RoomType.DashPractice:
                AddPlatform(room, -6f, -1f, 3f);
                AddPlatform(room, 0f, 1.8f, 3f);
                AddPlatform(room, 6f, -0.5f, 3f);
                AddMarker(room, "Hazard_冲刺下方危险区", "Mechanism", new Vector2(0f, -5.3f), new Vector2(9f, 0.8f));
                AddEnemy(room, 2f, -4f);
                break;
            case RoomType.FireIntro:
                AddPlatform(room, -5f, -0.5f, 4f);
                AddPlatform(room, 3f, 1.5f, 5f);
                AddMarker(room, "Mechanism_火炬预告", "Mechanism", new Vector2(7f, -4f), new Vector2(1.4f, 2f));
                AddEnemy(room, -3f, -4f);
                AddEnemy(room, 4f, -4f);
                break;
            case RoomType.FireAbility:
                AddPlatform(room, -4f, -1f, 4f);
                AddMarker(room, "Ability_火之魔法", "Ability", new Vector2(1f, -0.2f), new Vector2(1.4f, 1.4f));
                AddMarker(room, "Mechanism_火炬机关", "Mechanism", new Vector2(6f, -3f), new Vector2(1.4f, 2.4f));
                AddMarker(room, "Gate_火魔法开启出口", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                AddRoute(room, -8f, -3.6f, 17f);
                break;
            case RoomType.FirePractice:
                AddPlatform(room, -6f, 0f, 4f);
                AddPlatform(room, 2f, 2f, 4f);
                AddMarker(room, "Mechanism_连续火炬_01", "Mechanism", new Vector2(-2f, -4f), new Vector2(1.2f, 2f));
                AddMarker(room, "Mechanism_连续火炬_02", "Mechanism", new Vector2(4f, -4f), new Vector2(1.2f, 2f));
                AddEnemy(room, 7f, -4f);
                break;
            case RoomType.IceIntro:
                AddPlatform(room, -6f, -0.8f, 4f);
                AddPlatform(room, 1f, 1.5f, 5f);
                AddMarker(room, "Mechanism_水面预告", "Mechanism", new Vector2(5f, -5.2f), new Vector2(5f, 0.8f));
                AddEnemy(room, -2f, -4f);
                AddEnemy(room, 6f, -4f);
                break;
            case RoomType.IceAbility:
                AddPlatform(room, -4f, -1f, 4f);
                AddMarker(room, "Ability_冰之魔法", "Ability", new Vector2(1f, -0.2f), new Vector2(1.4f, 1.4f));
                AddMarker(room, "Mechanism_冻结水面", "Mechanism", new Vector2(6f, -5.2f), new Vector2(5f, 0.8f));
                AddMarker(room, "Gate_冰魔法通过出口", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                AddRoute(room, -8f, -3.6f, 17f);
                break;
            case RoomType.IcePractice:
                AddPlatform(room, -6f, 0f, 4f);
                AddPlatform(room, 2f, 2.3f, 4f);
                AddMarker(room, "Mechanism_暂停机关", "Mechanism", new Vector2(5f, -3.5f), new Vector2(2f, 2f));
                AddMarker(room, "Gate_冰机关出口", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                break;
            case RoomType.MemorialEntrance:
                AddPlatform(room, -3f, -1f, 5f);
                AddMarker(room, "Narrative_纪念堂入口", "Trigger", new Vector2(0f, -4f), new Vector2(1.2f, 1.2f));
                break;
            case RoomType.MemorialHall:
                for (int i = 0; i < 10; i++)
                    AddMarker(room, "Interact_旧日石碑_" + (i + 1).ToString("00"), "Trigger", new Vector2(-12f + i * 2.7f, -4f), new Vector2(1f, 2.2f));
                AddPlatform(room, -6f, -0.5f, 5f);
                AddPlatform(room, 6f, -0.5f, 5f);
                break;
            case RoomType.BlessingAbility:
                AddMarker(room, "Interact_中央赐福石碑", "Trigger", new Vector2(0f, -3.3f), new Vector2(3f, 4f));
                AddMarker(room, "Ability_旧日赐福", "Ability", new Vector2(0f, 1.2f), new Vector2(1.6f, 1.6f));
                AddMarker(room, "Gate_回大厅开启主塔攀登", "Gate", new Vector2(10f, -3f), new Vector2(1.2f, 3.5f));
                break;
            case RoomType.ClimbLower:
                AddAshPlatform(room, -5f, -2f, 4f);
                AddAshPlatform(room, 3f, 1f, 4f);
                AddAshPlatform(room, -3f, 4f, 4f);
                AddEnemy(room, 4f, -5f);
                AddMarker(room, "Mechanism_登顶机关_下段", "Mechanism", new Vector2(0f, -5f), new Vector2(1.4f, 1.4f));
                break;
            case RoomType.ClimbMiddle:
                AddAshPlatform(room, -5f, -3f, 3.5f);
                AddAshPlatform(room, 4f, 0f, 3.5f);
                AddAshPlatform(room, -3f, 4f, 3.5f);
                AddPlatform(room, 5f, 6.5f, 4f);
                AddMarker(room, "Mechanism_机关踏板", "Mechanism", new Vector2(0f, -1f), new Vector2(1.4f, 1.4f));
                AddEnemy(room, -4f, -5f);
                AddEnemy(room, 4f, 3.5f);
                break;
            case RoomType.ClimbUpper:
                AddAshPlatform(room, -5f, -3f, 3.5f);
                AddPlatform(room, 1f, 0f, 3.5f);
                AddAshPlatform(room, 5f, 3.5f, 3.5f);
                AddPlatform(room, -2f, 7f, 4f);
                AddMarker(room, "Mechanism_火冰复合机关", "Mechanism", new Vector2(4f, -5f), new Vector2(2f, 2f));
                break;
            case RoomType.BossRest:
                AddMarker(room, "SavePoint_Boss前篝火", "Save", new Vector2(-2f, -4f), new Vector2(1.4f, 1.8f));
                AddMarker(room, "Narrative_Boss前提示", "Trigger", new Vector2(2f, -4f), new Vector2(1.2f, 1.2f));
                break;
            case RoomType.Boss:
                AddPlatform(room, -8f, -0.5f, 5f);
                AddPlatform(room, 8f, -0.5f, 5f);
                AddAshPlatform(room, -4f, 2.5f, 4f);
                AddAshPlatform(room, 4f, 2.5f, 4f);
                AddEnemy(room, 0f, -4f, "Boss_塔顶守护者");
                break;
            case RoomType.Ending:
                AddMarker(room, "Trigger_结局CG", "Trigger", new Vector2(0f, -4f), new Vector2(1.5f, 1.5f));
                break;
        }
    }

    private void AddDefaultPortals(RoomBlueprintDefinition room, string nextRoom)
    {
        AddPortal(room, "Portal_In_01", new Vector2(-room.size.x * 0.5f + 1.4f, -3f), "上一房间", true);
        if (!string.IsNullOrEmpty(nextRoom))
            AddPortal(room, "Portal_Out_01", new Vector2(room.size.x * 0.5f - 1.4f, -3f), nextRoom, false);
    }

    private void AddMainGround(RoomBlueprintDefinition room)
    {
        AddTerrain(room, "Terrain_主地面", new Vector2(0f, -room.size.y * 0.5f + 1.2f), new Vector2(room.size.x - 4f, 1.2f));
    }

    private void AddTerrain(RoomBlueprintDefinition room, string name, Vector2 position, Vector2 size)
    {
        room.terrain.Add(new BlueprintElement(name, position, size, "Terrain"));
    }

    private void AddPlatform(RoomBlueprintDefinition room, float x, float y, float width)
    {
        room.platforms.Add(new BlueprintElement("Platform_跳台_" + (room.platforms.Count + 1).ToString("00"), new Vector2(x, y), new Vector2(width, 0.65f), "Platform"));
    }

    private void AddAshPlatform(RoomBlueprintDefinition room, float x, float y, float width)
    {
        room.ashPlatforms.Add(new BlueprintElement("AshPlatform_灰烬态跳台_" + (room.ashPlatforms.Count + 1).ToString("00"), new Vector2(x, y), new Vector2(width, 0.55f), "AshPlatform"));
    }

    private void AddEnemy(RoomBlueprintDefinition room, float x, float y)
    {
        AddEnemy(room, x, y, "Enemy_敌人_" + (room.enemies.Count + 1).ToString("00"));
    }

    private void AddEnemy(RoomBlueprintDefinition room, float x, float y, string name)
    {
        room.enemies.Add(new BlueprintElement(name, new Vector2(x, y), new Vector2(1.3f, 1.5f), "Enemy"));
    }

    private void AddMarker(RoomBlueprintDefinition room, string name, string kind, Vector2 position, Vector2 size)
    {
        room.markers.Add(new BlueprintElement(name, position, size, kind));
    }

    private void AddRoute(RoomBlueprintDefinition room, float x, float y, float width)
    {
        room.routes.Add(new BlueprintElement("Route_推荐行进路线_" + (room.routes.Count + 1).ToString("00"), new Vector2(x, y), new Vector2(width, 0.18f), "Route"));
    }

    private void AddPortal(RoomBlueprintDefinition room, string name, Vector2 position, string targetRoom, bool isInput)
    {
        room.portals.Add(new PortalDefinition(name, position, targetRoom, isInput));
    }

    private enum RoomType
    {
        Start,
        CombatIntro,
        SaveRoom,
        BrokenBridge,
        AshenBridge,
        Courtyard,
        SwordAbility,
        Hub,
        ClimbPreview,
        GearIntro,
        DashAbility,
        DashPractice,
        FireIntro,
        FireAbility,
        FirePractice,
        IceIntro,
        IceAbility,
        IcePractice,
        MemorialEntrance,
        MemorialHall,
        BlessingAbility,
        ClimbLower,
        ClimbMiddle,
        ClimbUpper,
        BossRest,
        Boss,
        Ending
    }

    private class RoomContext
    {
        public RoomBlueprintDefinition room;
        public Transform root;
        public Transform bounds;
        public Transform terrain;
        public Transform platforms;
        public Transform ashPlatforms;
        public Transform portals;
        public Transform enemies;
        public Transform triggers;
        public Transform markers;
        public Transform labels;
        public Transform notes;
    }

    private class RoomBlueprintDefinition
    {
        public string name;
        public string purpose;
        public string teachingFocus;
        public string notes;
        public RoomType type;
        public Vector2 size;
        public List<BlueprintElement> terrain = new List<BlueprintElement>();
        public List<BlueprintElement> platforms = new List<BlueprintElement>();
        public List<BlueprintElement> ashPlatforms = new List<BlueprintElement>();
        public List<BlueprintElement> enemies = new List<BlueprintElement>();
        public List<BlueprintElement> markers = new List<BlueprintElement>();
        public List<BlueprintElement> routes = new List<BlueprintElement>();
        public List<PortalDefinition> portals = new List<PortalDefinition>();
    }

    private class BlueprintElement
    {
        public string name;
        public Vector2 position;
        public Vector2 size;
        public string kind;

        public BlueprintElement(string name, Vector2 position, Vector2 size, string kind)
        {
            this.name = name;
            this.position = position;
            this.size = size;
            this.kind = kind;
        }
    }

    private class PortalDefinition
    {
        public string name;
        public Vector2 position;
        public string targetRoom;
        public bool isInput;

        public PortalDefinition(string name, Vector2 position, string targetRoom, bool isInput)
        {
            this.name = name;
            this.position = position;
            this.targetRoom = targetRoom;
            this.isInput = isInput;
        }
    }
}
