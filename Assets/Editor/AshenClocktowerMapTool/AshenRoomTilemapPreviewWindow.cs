using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 《灰烬钟塔》房间流程 Tilemap 预览生成器。
/// 只生成 Ground 层和灰烬态格子，用来快速预览大致流程，不处理背景和最终美术细节。
/// </summary>
public class AshenRoomTilemapPreviewWindow : EditorWindow
{
    private const string RootName = "Generated_RoomTilemapPreviewRoot";
    private const int RoomScale = 2;
    private const int Columns = 3;
    private const int RoomSpacingX = 84;
    private const int RoomSpacingY = 160;

    private bool includeLabels = true;
    private bool includePortalPrefabs = true;
    private bool includeSavePointPrefabs = true;
    private Vector2 scroll;

    private TileBase groundTile;
    private TileBase ashenTile;
    private GameObject portalPrefab;
    private GameObject savePointPrefab;

    private Tilemap groundTilemap;
    private Tilemap ashenTilemap;
    private Transform markerRoot;

    [MenuItem("Tools/灰烬钟塔/房间流程 Tilemap 预览生成器")]
    public static void OpenWindow()
    {
        GetWindow<AshenRoomTilemapPreviewWindow>("房间流程 Tilemap 预览");
    }

    private void OnEnable()
    {
        LoadDefaultAssets();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("《灰烬钟塔》房间流程 Tilemap 预览", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("该工具只铺 Ground 和灰烬态 Tilemap，并放置篝火/传送门 Prefab，用于预览房间流程。不会生成背景，不会修改核心玩法脚本。", MessageType.Info);

        groundTile = (TileBase)EditorGUILayout.ObjectField("Ground Tile", groundTile, typeof(TileBase), false);
        ashenTile = (TileBase)EditorGUILayout.ObjectField("灰烬态 Tile", ashenTile, typeof(TileBase), false);
        portalPrefab = (GameObject)EditorGUILayout.ObjectField("传送门 Prefab", portalPrefab, typeof(GameObject), false);
        savePointPrefab = (GameObject)EditorGUILayout.ObjectField("篝火/存档点 Prefab", savePointPrefab, typeof(GameObject), false);

        includeLabels = EditorGUILayout.Toggle("生成房间文字标签", includeLabels);
        includePortalPrefabs = EditorGUILayout.Toggle("生成传送门 Prefab", includePortalPrefabs);
        includeSavePointPrefabs = EditorGUILayout.Toggle("生成篝火 Prefab", includeSavePointPrefabs);

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("生成房间流程 Tilemap 预览", GUILayout.Height(30f)))
            GeneratePreviewMap(true);

        if (GUILayout.Button("删除房间流程 Tilemap 预览", GUILayout.Height(24f)))
            ClearPreviewRoot(true);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("生成根物体", RootName);
        EditorGUILayout.EndScrollView();
    }

    private void LoadDefaultAssets()
    {
        groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Palette/Castle/Tileset/floor_tile_1.asset");
        if (groundTile == null)
            groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Palette/Haunt/Tileset/haunt_0.asset");

        ashenTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Palette/Haunt/Tileset/haunt_3.asset");
        if (ashenTile == null)
            ashenTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Palette/Castle/Tileset/platform_1.asset");

        portalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Interact/Portal.prefab");
        savePointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Interact/CheckPoint.prefab");
    }

    private void GeneratePreviewMap(bool askBeforeReplace)
    {
        if (groundTile == null || ashenTile == null)
        {
            EditorUtility.DisplayDialog("生成失败", "请先指定 Ground Tile 和灰烬态 Tile。", "确定");
            return;
        }

        if (!ClearPreviewRoot(askBeforeReplace))
            return;

        GameObject rootObject = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootObject, "Generate Room Tilemap Preview");

        Grid grid = CreateGrid(rootObject.transform);
        groundTilemap = CreateTilemap(grid.transform, "Tilemap_Ground_流程预览", "Ground", 0, true);
        ashenTilemap = CreateTilemap(grid.transform, "Tilemap_Ashen_灰烬态预览", "Ground", 1, true);
        AddAshenObjectIfAvailable(ashenTilemap.gameObject);

        markerRoot = CreateGroup(rootObject.transform, "Markers_传送门_篝火_标签");

        List<RoomPreviewDefinition> rooms = BuildRooms();
        for (int i = 0; i < rooms.Count; i++)
            GenerateRoom(rooms[i], GetRoomOrigin(i));

        Selection.activeGameObject = rootObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("已生成《灰烬钟塔》房间流程 Tilemap 预览，共 " + rooms.Count + " 个房间。");
    }

    private bool ClearPreviewRoot(bool askBeforeDelete)
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot == null)
            return true;

        if (askBeforeDelete)
        {
            bool shouldDelete = EditorUtility.DisplayDialog(
                "删除旧 Tilemap 预览",
                "场景中已经存在 " + RootName + "。是否删除旧预览并继续？",
                "删除并继续",
                "取消");

            if (!shouldDelete)
                return false;
        }

        Undo.DestroyObjectImmediate(oldRoot);
        return true;
    }

    private Grid CreateGrid(Transform parent)
    {
        GameObject gridObject = new GameObject("Grid_RoomTilemapPreview");
        Undo.RegisterCreatedObjectUndo(gridObject, "Create Preview Grid");
        gridObject.transform.SetParent(parent);
        return gridObject.AddComponent<Grid>();
    }

    private Tilemap CreateTilemap(Transform parent, string name, string sortingLayer, int sortingOrder, bool withCollider)
    {
        GameObject tilemapObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(tilemapObject, "Create Preview Tilemap");
        tilemapObject.transform.SetParent(parent);

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (withCollider && groundLayer >= 0)
            tilemapObject.layer = groundLayer;

        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = sortingOrder;

        if (withCollider)
        {
            TilemapCollider2D tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
            Rigidbody2D rigidbody = tilemapObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Static;
            tilemapObject.AddComponent<CompositeCollider2D>();
            tilemapCollider.usedByComposite = true;
        }

        return tilemap;
    }

    private Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, "Create Preview Group");
        group.transform.SetParent(parent);
        group.transform.localPosition = Vector3.zero;
        return group.transform;
    }

    private Vector2Int GetRoomOrigin(int index)
    {
        int column = index % Columns;
        int row = index / Columns;
        return new Vector2Int(column * RoomSpacingX, -row * RoomSpacingY);
    }

    private void GenerateRoom(RoomPreviewDefinition room, Vector2Int origin)
    {
        // 用 Ground Tile 画房间边界，方便直接看出每个房间的独立范围。
        DrawRoomFrame(origin, room.size);

        foreach (RectInt rect in room.groundRects)
            Fill(groundTilemap, groundTile, origin, rect);

        foreach (RectInt rect in room.ashenRects)
            Fill(ashenTilemap, ashenTile, origin, rect);

        foreach (MarkerDefinition marker in room.markers)
            CreateMarkerPrefabOrFallback(room, origin, marker);

        if (includeLabels)
            CreateLabel(room.name, origin + new Vector2Int(room.size.x / 2, room.size.y + 2), room.name);
    }

    private void DrawRoomFrame(Vector2Int origin, Vector2Int size)
    {
        Fill(groundTilemap, groundTile, origin, new RectInt(0, 0, size.x, 1));
        Fill(groundTilemap, groundTile, origin, new RectInt(0, size.y - 1, size.x, 1));
        Fill(groundTilemap, groundTile, origin, new RectInt(0, 0, 1, size.y));
        Fill(groundTilemap, groundTile, origin, new RectInt(size.x - 1, 0, 1, size.y));
    }

    private void Fill(Tilemap tilemap, TileBase tile, Vector2Int origin, RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
                tilemap.SetTile(new Vector3Int(origin.x + x, origin.y + y, 0), tile);
        }
    }

    private void CreateMarkerPrefabOrFallback(RoomPreviewDefinition room, Vector2Int origin, MarkerDefinition marker)
    {
        Vector3 position = new Vector3(origin.x + marker.position.x, origin.y + marker.position.y, 0f);
        GameObject prefab = null;

        if (marker.kind == MarkerKind.Portal && includePortalPrefabs)
            prefab = portalPrefab;
        else if (marker.kind == MarkerKind.SavePoint && includeSavePointPrefabs)
            prefab = savePointPrefab;

        GameObject created = null;
        if (prefab != null)
        {
            created = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(created, "Create Preview Prefab Marker");
            created.name = marker.name;
            created.transform.SetParent(markerRoot);
            created.transform.position = position;
        }
        else
        {
            created = CreateFallbackMarker(marker.name, position, GetMarkerColor(marker.kind), marker.size);
        }

        if (includeLabels)
            CreateLabel("Label_" + marker.name, new Vector2(position.x, position.y + 2f), marker.label);

        if (created != null && marker.kind == MarkerKind.Portal)
            created.name = marker.name + "_To_" + marker.targetRoom;
    }

    private GameObject CreateFallbackMarker(string name, Vector3 position, Color color, Vector2 size)
    {
        GameObject marker = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(marker, "Create Preview Marker");
        marker.transform.SetParent(markerRoot);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = 20;
        return marker;
    }

    private Color GetMarkerColor(MarkerKind kind)
    {
        switch (kind)
        {
            case MarkerKind.Portal:
                return new Color(0.1f, 0.85f, 1f, 0.75f);
            case MarkerKind.SavePoint:
                return new Color(1f, 0.5f, 0.05f, 0.85f);
            case MarkerKind.Ability:
                return new Color(1f, 0.85f, 0.05f, 0.9f);
            case MarkerKind.Enemy:
                return new Color(0.95f, 0.1f, 0.1f, 0.85f);
            case MarkerKind.Mechanism:
                return new Color(0.65f, 0.25f, 1f, 0.75f);
            default:
                return Color.white;
        }
    }

    private void CreateLabel(string name, Vector2 position, string text)
    {
        GameObject labelObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(labelObject, "Create Preview Label");
        labelObject.transform.SetParent(markerRoot);
        labelObject.transform.position = position;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = new Color(1f, 0.92f, 0.65f, 1f);
        textMesh.fontSize = 32;
        textMesh.characterSize = 0.18f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }

    private void AddAshenObjectIfAvailable(GameObject target)
    {
        if (target.GetComponent<AshenObject>() == null)
            target.AddComponent<AshenObject>();
    }

    private List<RoomPreviewDefinition> BuildRooms()
    {
        List<RoomPreviewDefinition> rooms = new List<RoomPreviewDefinition>();
        AddRoom(rooms, "Room_01_开场落点", RoomKind.Start, "Room_02_荒凉小径_战斗教学");
        AddRoom(rooms, "Room_02_荒凉小径_战斗教学", RoomKind.CombatIntro, "Room_03_篝火休整区");
        AddRoom(rooms, "Room_03_篝火休整区", RoomKind.SaveRoom, "Room_04_断桥教学区");
        AddRoom(rooms, "Room_04_断桥教学区", RoomKind.BrokenBridge, "Room_05_灰烬态通桥区");
        AddRoom(rooms, "Room_05_灰烬态通桥区", RoomKind.AshenBridge, "Room_06_钟塔前庭_投剑战斗区");
        AddRoom(rooms, "Room_06_钟塔前庭_投剑战斗区", RoomKind.Courtyard, "Room_07_投掷宝剑教学区");
        AddRoom(rooms, "Room_07_投掷宝剑教学区", RoomKind.SwordAbility, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(rooms, "Room_08_钟塔大厅_Hub_主塔攀登一体房", RoomKind.Hub, "");
        AddRoom(rooms, "Room_10_齿轮廊道_冲刺前段", RoomKind.GearIntro, "Room_11_齿轮廊道_冲刺获取区");
        AddRoom(rooms, "Room_11_齿轮廊道_冲刺获取区", RoomKind.DashAbility, "Room_12_齿轮廊道_冲刺教学区");
        AddRoom(rooms, "Room_12_齿轮廊道_冲刺教学区", RoomKind.DashPractice, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(rooms, "Room_13_熔灰工坊_火魔法前段", RoomKind.FireIntro, "Room_14_熔灰工坊_火魔法获取区");
        AddRoom(rooms, "Room_14_熔灰工坊_火魔法获取区", RoomKind.FireAbility, "Room_15_熔灰工坊_火魔法教学区");
        AddRoom(rooms, "Room_15_熔灰工坊_火魔法教学区", RoomKind.FirePractice, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(rooms, "Room_16_静霜钟室_冰魔法前段", RoomKind.IceIntro, "Room_17_静霜钟室_冰魔法获取区");
        AddRoom(rooms, "Room_17_静霜钟室_冰魔法获取区", RoomKind.IceAbility, "Room_18_静霜钟室_冰魔法教学区");
        AddRoom(rooms, "Room_18_静霜钟室_冰魔法教学区", RoomKind.IcePractice, "Room_19_古老纪念堂入口");
        AddRoom(rooms, "Room_19_古老纪念堂入口", RoomKind.MemorialEntrance, "Room_20_旧日长廊");
        AddRoom(rooms, "Room_20_旧日长廊", RoomKind.MemorialHall, "Room_21_中央赐福石碑室");
        AddRoom(rooms, "Room_21_中央赐福石碑室", RoomKind.BlessingAbility, "Room_08_钟塔大厅_Hub_主塔攀登一体房");
        AddRoom(rooms, "Room_25_Boss前休整室", RoomKind.BossRest, "Room_26_塔顶钟室_Boss");
        AddRoom(rooms, "Room_26_塔顶钟室_Boss", RoomKind.Boss, "Room_27_结局CG触发室");
        AddRoom(rooms, "Room_27_结局CG触发室", RoomKind.Ending, "");
        return rooms;
    }

    private void AddRoom(List<RoomPreviewDefinition> rooms, string name, RoomKind kind, string nextRoom)
    {
        RoomPreviewDefinition room = new RoomPreviewDefinition
        {
            name = name,
            kind = kind,
            nextRoom = nextRoom,
            size = GetRoomSize(kind)
        };

        PopulateRoom(room);
        rooms.Add(room);
    }

    private Vector2Int GetRoomSize(RoomKind kind)
    {
        Vector2Int baseSize;
        switch (kind)
        {
            case RoomKind.Hub:
                baseSize = new Vector2Int(34, 72);
                break;
            case RoomKind.Boss:
                baseSize = new Vector2Int(30, 18);
                break;
            case RoomKind.MemorialHall:
                baseSize = new Vector2Int(32, 16);
                break;
            case RoomKind.ClimbLower:
            case RoomKind.ClimbMiddle:
            case RoomKind.ClimbUpper:
                baseSize = new Vector2Int(24, 20);
                break;
            default:
                baseSize = new Vector2Int(26, 15);
                break;
        }

        return new Vector2Int(baseSize.x * RoomScale, baseSize.y * RoomScale);
    }

    private void PopulateRoom(RoomPreviewDefinition room)
    {
        int baseWidth = room.size.x / RoomScale;
        int baseHeight = room.size.y / RoomScale;

        AddGround(room, 2, 1, baseWidth - 4, 2);
        AddPortal(room, "Portal_In_01", 2, 3, "上一房间");
        if (!string.IsNullOrEmpty(room.nextRoom))
            AddPortal(room, "Portal_Out_01", baseWidth - 3, 3, room.nextRoom);

        switch (room.kind)
        {
            case RoomKind.CombatIntro:
                AddPlatform(room, 6, 6, 5);
                AddPlatform(room, 14, 8, 5);
                AddEnemy(room, 10, 3);
                AddEnemy(room, 16, 3);
                break;
            case RoomKind.SaveRoom:
                AddSave(room, 12, 3);
                break;
            case RoomKind.BrokenBridge:
                room.groundRects.Clear();
                AddGround(room, 2, 1, 9, 2);
                AddGround(room, 15, 1, 9, 2);
                AddMechanism(room, "Gate_需要灰烬态", 13, 3);
                break;
            case RoomKind.AshenBridge:
                AddAshen(room, 8, 5, 5);
                AddAshen(room, 14, 7, 5);
                break;
            case RoomKind.Courtyard:
                AddPlatform(room, 6, 6, 5);
                AddPlatform(room, 14, 8, 5);
                AddEnemy(room, 8, 3);
                AddEnemy(room, 14, 3);
                AddEnemy(room, 19, 7);
                break;
            case RoomKind.SwordAbility:
                AddPlatform(room, 7, 6, 4);
                AddAbility(room, "Ability_投掷宝剑", 13, 7);
                AddMechanism(room, "Mechanism_远程机关", 19, 9);
                AddMechanism(room, "Gate_投剑开启出口", 23, 3);
                break;
            case RoomKind.Hub:
                AddSave(room, 8, 3);
                AddPlatform(room, 8, 7, 5);
                AddPlatform(room, 21, 7, 5);
                AddAshen(room, 15, 11, 5);
                AddPortal(room, "Portal_Out_To_Room_10_齿轮廊道", 4, 10, "Room_10_齿轮廊道_冲刺前段");
                AddPortal(room, "Portal_Out_To_Room_13_熔灰工坊", 15, 14, "Room_13_熔灰工坊_火魔法前段");
                AddPortal(room, "Portal_Out_To_Room_16_静霜钟室", 30, 10, "Room_16_静霜钟室_冰魔法前段");
                AddMechanism(room, "Gate_需要旧日赐福_开启主塔攀登", 17, 18);
                AddAshen(room, 14, 22, 5);
                AddAshen(room, 21, 27, 5);
                AddPlatform(room, 8, 31, 5);
                AddAshen(room, 15, 36, 5);
                AddAshen(room, 7, 41, 5);
                AddPlatform(room, 21, 46, 5);
                AddAshen(room, 14, 51, 5);
                AddAshen(room, 22, 56, 5);
                AddPlatform(room, 9, 61, 6);
                AddAshen(room, 16, 66, 5);
                AddPortal(room, "Portal_Out_To_Room_25_Boss前休整室", 17, 69, "Room_25_Boss前休整室");
                AddEnemy(room, 8, 23);
                AddEnemy(room, 24, 37);
                AddEnemy(room, 10, 52);
                AddMechanism(room, "Mechanism_主塔火冰复合机关", 25, 48);
                break;
            case RoomKind.ClimbPreview:
                AddAshen(room, 6, 5, 4);
                AddAshen(room, 13, 8, 4);
                AddAshen(room, 19, 11, 4);
                break;
            case RoomKind.GearIntro:
                AddPlatform(room, 6, 6, 5);
                AddPlatform(room, 14, 8, 5);
                AddMechanism(room, "Mechanism_齿轮预告", 19, 3);
                AddEnemy(room, 10, 3);
                break;
            case RoomKind.DashAbility:
                AddPlatform(room, 7, 6, 4);
                AddAbility(room, "Ability_冲刺", 12, 7);
                AddMechanism(room, "Mechanism_宽裂隙", 17, 2);
                AddGround(room, 21, 1, 3, 2);
                break;
            case RoomKind.DashPractice:
                AddPlatform(room, 6, 6, 3);
                AddPlatform(room, 13, 9, 3);
                AddPlatform(room, 20, 6, 3);
                AddMechanism(room, "Hazard_冲刺危险区", 14, 2);
                break;
            case RoomKind.FireIntro:
                AddPlatform(room, 7, 6, 4);
                AddPlatform(room, 15, 8, 5);
                AddMechanism(room, "Mechanism_火炬预告", 20, 3);
                AddEnemy(room, 11, 3);
                break;
            case RoomKind.FireAbility:
                AddPlatform(room, 7, 6, 4);
                AddAbility(room, "Ability_火之魔法", 12, 7);
                AddMechanism(room, "Mechanism_火炬机关", 18, 3);
                AddMechanism(room, "Gate_火魔法开启出口", 23, 3);
                break;
            case RoomKind.FirePractice:
                AddPlatform(room, 7, 7, 4);
                AddPlatform(room, 16, 9, 4);
                AddMechanism(room, "Mechanism_连续火炬_01", 11, 3);
                AddMechanism(room, "Mechanism_连续火炬_02", 18, 3);
                AddEnemy(room, 21, 3);
                break;
            case RoomKind.IceIntro:
                AddPlatform(room, 7, 6, 4);
                AddPlatform(room, 15, 8, 5);
                AddMechanism(room, "Mechanism_水面预告", 18, 2);
                AddEnemy(room, 11, 3);
                break;
            case RoomKind.IceAbility:
                AddPlatform(room, 7, 6, 4);
                AddAbility(room, "Ability_冰之魔法", 12, 7);
                AddMechanism(room, "Mechanism_冻结水面", 18, 2);
                AddMechanism(room, "Gate_冰魔法通过出口", 23, 3);
                break;
            case RoomKind.IcePractice:
                AddPlatform(room, 7, 7, 4);
                AddPlatform(room, 16, 10, 4);
                AddMechanism(room, "Mechanism_暂停机关", 18, 3);
                break;
            case RoomKind.MemorialEntrance:
                AddPlatform(room, 10, 6, 6);
                break;
            case RoomKind.MemorialHall:
                for (int i = 0; i < 10; i++)
                    AddMechanism(room, "Interact_旧日石碑_" + (i + 1).ToString("00"), 5 + i * 2, 3);
                AddPlatform(room, 8, 7, 5);
                AddPlatform(room, 20, 7, 5);
                break;
            case RoomKind.BlessingAbility:
                AddMechanism(room, "Interact_中央赐福石碑", 13, 4);
                AddAbility(room, "Ability_旧日赐福", 13, 8);
                break;
            case RoomKind.ClimbLower:
                AddAshen(room, 6, 6, 4);
                AddAshen(room, 14, 9, 4);
                AddAshen(room, 8, 13, 4);
                AddEnemy(room, 17, 3);
                break;
            case RoomKind.ClimbMiddle:
                AddAshen(room, 5, 5, 4);
                AddAshen(room, 14, 8, 4);
                AddAshen(room, 7, 12, 4);
                AddPlatform(room, 15, 16, 4);
                AddEnemy(room, 6, 3);
                AddEnemy(room, 16, 11);
                break;
            case RoomKind.ClimbUpper:
                AddAshen(room, 5, 5, 4);
                AddPlatform(room, 12, 8, 4);
                AddAshen(room, 17, 12, 4);
                AddPlatform(room, 9, 16, 5);
                AddMechanism(room, "Mechanism_火冰复合机关", 17, 3);
                break;
            case RoomKind.BossRest:
                AddSave(room, 12, 3);
                break;
            case RoomKind.Boss:
                AddPlatform(room, 5, 7, 6);
                AddPlatform(room, 20, 7, 6);
                AddAshen(room, 11, 10, 4);
                AddAshen(room, 16, 10, 4);
                AddEnemy(room, 15, 3, "Boss_塔顶守护者");
                break;
            case RoomKind.Ending:
                AddMechanism(room, "Trigger_结局CG", 13, 3);
                break;
        }

        AddSharedRoomDetails(room, baseWidth, baseHeight);
    }

    private void AddSharedRoomDetails(RoomPreviewDefinition room, int baseWidth, int baseHeight)
    {
        // 在放大后的房间里补一些边缘台阶、上层短平台和墙脚块，让流程预览不显得空。
        if (room.kind == RoomKind.BrokenBridge)
            return;

        AddPlatform(room, 3, baseHeight - 5, 4);
        AddPlatform(room, baseWidth - 7, baseHeight - 5, 4);

        if (room.kind != RoomKind.Start && room.kind != RoomKind.SaveRoom && room.kind != RoomKind.Ending)
        {
            AddPlatform(room, baseWidth / 2 - 3, baseHeight - 8, 6);
            AddGround(room, 2, 4, 2, 3);
            AddGround(room, baseWidth - 4, 4, 2, 3);
        }

        if (room.kind == RoomKind.Hub || room.kind == RoomKind.Boss || room.kind == RoomKind.MemorialHall)
        {
            AddPlatform(room, baseWidth / 2 - 8, baseHeight / 2 + 1, 5);
            AddPlatform(room, baseWidth / 2 + 3, baseHeight / 2 + 1, 5);
        }
    }

    private void AddGround(RoomPreviewDefinition room, int x, int y, int width, int height)
    {
        room.groundRects.Add(ScaleRect(x, y, width, height));
    }

    private void AddPlatform(RoomPreviewDefinition room, int x, int y, int width)
    {
        room.groundRects.Add(ScaleRect(x, y, width, 1));
    }

    private void AddAshen(RoomPreviewDefinition room, int x, int y, int width)
    {
        room.ashenRects.Add(ScaleRect(x, y, width, 1));
    }

    private void AddPortal(RoomPreviewDefinition room, string name, int x, int y, string targetRoom)
    {
        room.markers.Add(new MarkerDefinition(name, MarkerKind.Portal, ScalePoint(x, y), ScaleSize(1.4f, 2.2f), targetRoom));
    }

    private void AddSave(RoomPreviewDefinition room, int x, int y)
    {
        room.markers.Add(new MarkerDefinition("SavePoint_篝火", MarkerKind.SavePoint, ScalePoint(x, y), ScaleSize(1.3f, 1.5f), ""));
    }

    private void AddAbility(RoomPreviewDefinition room, string name, int x, int y)
    {
        room.markers.Add(new MarkerDefinition(name, MarkerKind.Ability, ScalePoint(x, y), ScaleSize(1.2f, 1.2f), ""));
    }

    private void AddEnemy(RoomPreviewDefinition room, int x, int y)
    {
        AddEnemy(room, x, y, "Enemy_敌人");
    }

    private void AddEnemy(RoomPreviewDefinition room, int x, int y, string name)
    {
        room.markers.Add(new MarkerDefinition(name, MarkerKind.Enemy, ScalePoint(x, y), ScaleSize(1.3f, 1.4f), ""));
    }

    private void AddMechanism(RoomPreviewDefinition room, string name, int x, int y)
    {
        room.markers.Add(new MarkerDefinition(name, MarkerKind.Mechanism, ScalePoint(x, y), ScaleSize(1.4f, 1.4f), ""));
    }

    private RectInt ScaleRect(int x, int y, int width, int height)
    {
        return new RectInt(x * RoomScale, y * RoomScale, width * RoomScale, height * RoomScale);
    }

    private Vector2 ScalePoint(int x, int y)
    {
        return new Vector2(x * RoomScale, y * RoomScale);
    }

    private Vector2 ScaleSize(float width, float height)
    {
        return new Vector2(width * RoomScale, height * RoomScale);
    }

    private enum RoomKind
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

    private enum MarkerKind
    {
        Portal,
        SavePoint,
        Ability,
        Enemy,
        Mechanism
    }

    private class RoomPreviewDefinition
    {
        public string name;
        public string nextRoom;
        public RoomKind kind;
        public Vector2Int size;
        public List<RectInt> groundRects = new List<RectInt>();
        public List<RectInt> ashenRects = new List<RectInt>();
        public List<MarkerDefinition> markers = new List<MarkerDefinition>();
    }

    private class MarkerDefinition
    {
        public string name;
        public MarkerKind kind;
        public Vector2 position;
        public Vector2 size;
        public string targetRoom;

        public string label
        {
            get
            {
                if (kind == MarkerKind.Portal)
                    return name + "\n目标：" + targetRoom;
                return name;
            }
        }

        public MarkerDefinition(string name, MarkerKind kind, Vector2 position, Vector2 size, string targetRoom)
        {
            this.name = name;
            this.kind = kind;
            this.position = position;
            this.size = size;
            this.targetRoom = targetRoom;
        }
    }
}
