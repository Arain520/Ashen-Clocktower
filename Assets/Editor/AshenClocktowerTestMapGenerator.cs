using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 《灰烬钟塔》Tilemap 版完整地图生成器。
/// 该工具只在 Editor 中使用，点击菜单后才会向当前场景生成 Tilemap 地图。
/// </summary>
public static class AshenClocktowerTestMapGenerator
{
    private const string RootName = "Generated_AshenClocktower_TilemapMap";
    private const int Scale = 1;

    private static TilePaletteSet tiles;
    private static Transform root;
    private static Tilemap ground;
    private static Tilemap walls;
    private static Tilemap background;
    private static Tilemap details;
    private static Tilemap hazards;
    private static Tilemap ashen;
    private static MapCounter counter;
    private static StringBuilder coordinateLog;

    [MenuItem("Tools/灰烬钟塔/生成 Tilemap 完整地图")]
    public static void GenerateFullTilemapMap()
    {
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "生成 Tilemap 完整地图",
                "场景中已经存在 Generated_AshenClocktower_TilemapMap。是否删除旧 Tilemap 地图并重新生成？",
                "删除并生成",
                "取消");

            if (!replace)
                return;

            Undo.DestroyObjectImmediate(oldRoot);
        }

        tiles = LoadTiles();
        if (!tiles.HasRequiredTiles)
        {
            EditorUtility.DisplayDialog(
                "生成失败",
                "没有找到可用 Tile。请检查 Assets/Palette/Haunt/Tileset 与 Assets/Palette/Castle/Tileset。",
                "确定");
            return;
        }

        counter = new MapCounter();
        coordinateLog = new StringBuilder();
        coordinateLog.AppendLine("《灰烬钟塔》Tilemap 精细蓝图坐标清单");
        coordinateLog.AppendLine("说明：坐标为生成器设计坐标，当前 Scale = " + Scale + "。");
        coordinateLog.AppendLine("流程原则：进入房间 -> 普通关卡挑战 -> 获得能力 -> 立即使用能力 -> 打开出口/返回主路线。");
        coordinateLog.AppendLine();
        GameObject rootObject = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootObject, "Generate Ashen Clocktower Tilemap Map");
        root = rootObject.transform;

        Grid grid = CreateGrid(root);
        background = CreateTilemap(grid.transform, "Tilemap_Background_背景", "Background", -20, false);
        details = CreateTilemap(grid.transform, "Tilemap_Details_装饰", "Background", -10, false);
        ground = CreateTilemap(grid.transform, "Tilemap_Ground_地面平台", "Ground", 0, true);
        walls = CreateTilemap(grid.transform, "Tilemap_Walls_墙体边界", "Ground", 1, true);
        hazards = CreateTilemap(grid.transform, "Tilemap_Hazards_危险区域", "Ground", 2, true);
        ashen = CreateTilemap(grid.transform, "Tilemap_Ashen_灰烬态平台", "Ground", 3, true);

        AddAshenObjectIfAvailable(ashen.gameObject);

        DrawFullBlueprint();
        CreateGameplayPlaceholders();
        ExportCoordinateList();

        Selection.activeGameObject = rootObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log(
            "已生成《灰烬钟塔》Tilemap 完整地图：\n" +
            "- 区域数量：" + counter.areaCount + "\n" +
            "- 地面/墙体 Tile 数：" + counter.solidTileCount + "\n" +
            "- 背景/装饰 Tile 数：" + counter.detailTileCount + "\n" +
            "- 灰烬态 Tile 数：" + counter.ashenTileCount + "\n" +
            "- 危险 Tile 数：" + counter.hazardTileCount + "\n" +
            "- 敌人占位数量：" + counter.enemyCount + "\n" +
            "- 传送门占位数量：" + counter.portalCount + "\n" +
            "- 能力点占位数量：" + counter.abilityCount + "\n" +
            "- 机关/能力门占位数量：" + counter.mechanismCount + "\n" +
            "- 剧情触发点数量：" + counter.narrativeCount);

        EditorUtility.DisplayDialog(
            "生成完成",
            "已按当前蓝图和 Palette 资源生成 Tilemap 完整地图。\n所有内容在 Generated_AshenClocktower_TilemapMap 下。",
            "确定");
    }

    [MenuItem("Tools/Ashen Clocktower/Generate Test Map")]
    public static void GenerateLegacyMenuEntry()
    {
        GenerateFullTilemapMap();
    }

    private static Grid CreateGrid(Transform parent)
    {
        GameObject gridObject = new GameObject("Grid_AshenClocktower_Tilemap");
        Undo.RegisterCreatedObjectUndo(gridObject, "Create Ashen Clocktower Grid");
        gridObject.transform.SetParent(parent);

        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = Vector3.one;
        return grid;
    }

    private static Tilemap CreateTilemap(Transform parent, string name, string sortingLayer, int sortingOrder, bool withCollider)
    {
        GameObject tilemapObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(tilemapObject, "Create Tilemap");
        tilemapObject.transform.SetParent(parent);

        if (withCollider)
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
                tilemapObject.layer = groundLayer;
        }

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

    private static void DrawFullBlueprint()
    {
        DrawArea_IntroPath();
        DrawArea_BrokenBridge();
        DrawArea_Courtyard();
        DrawArea_TowerHall();
        DrawArea_GearCorridor();
        DrawArea_AshFoundry();
        DrawArea_FrostBellRoom();
        DrawArea_MemorialHall();
        DrawArea_TowerClimb();
        DrawArea_BossRoom();
    }

    private static void DrawArea_IntroPath()
    {
        counter.areaCount++;
        DrawRoom("Area_01_荒凉小径", -80, 0, 48, 18, tiles.hauntBackground, tiles.hauntWall);
        Platform(ground, tiles.hauntGround, -92, -2, 30, 3);
        Platform(ground, tiles.hauntGround, -77, 4, 9, 2);
        Platform(ground, tiles.hauntGround, -61, 2, 14, 2);
        Platform(ground, tiles.hauntGround, -86, 8, 7, 2);
        Platform(ground, tiles.hauntGround, -69, 11, 7, 2);
        Platform(ground, tiles.hauntGround, -58, 7, 5, 2);
        DetailCluster(-87, 4, 4, 7, tiles.hauntDetail);
        DetailCluster(-71, 7, 3, 6, tiles.hauntDetail);
        DetailCluster(-55, 3, 6, 2, tiles.hauntDetail);
    }

    private static void DrawArea_BrokenBridge()
    {
        counter.areaCount++;
        DrawRoom("Area_02_断桥教学区", -42, 0, 38, 22, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, -50, -2, 13, 3);
        Platform(ground, tiles.castleGround, -28, -2, 15, 3);
        Platform(ground, tiles.castleGround, -45, 7, 6, 2);
        Platform(ground, tiles.castleGround, -48, 11, 5, 2);
        Platform(ground, tiles.castleGround, -21, 9, 6, 2);
        AshPlatform(-37, 3, 6, 1);
        AshPlatform(-31, 5, 5, 1);
        AshPlatform(-34, 9, 5, 1);
        DetailCluster(-38, 1, 2, 6, tiles.castleDamaged);
        DetailCluster(-30, 1, 2, 6, tiles.castleDamaged);
    }

    private static void DrawArea_Courtyard()
    {
        counter.areaCount++;
        DrawRoom("Area_03_钟塔前庭", -14, 0, 46, 28, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, -20, -2, 32, 3);
        Platform(ground, tiles.castleGround, -14, 7, 7, 2);
        Platform(ground, tiles.castleGround, 1, 5, 8, 2);
        Platform(ground, tiles.castleGround, 11, 8, 7, 2);
        Platform(ground, tiles.castlePlatform, 21, 11, 7, 2);
        Platform(ground, tiles.castleGround, 22, 17, 6, 2);
        Platform(ground, tiles.castlePlatform, 5, 12, 5, 2);
        Platform(ground, tiles.castlePlatform, 16, 15, 5, 2);
        Solid(walls, tiles.castleWall, 27, 0, 5, 17);
        WindowRow(0, 12, 3);
        WindowRow(12, 15, 3);
    }

    private static void DrawArea_TowerHall()
    {
        counter.areaCount++;
        DrawRoom("Area_04_钟塔大厅", 16, 0, 48, 42, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 12, -2, 32, 3);
        Platform(ground, tiles.castleGround, 16, 12, 11, 2);
        Platform(ground, tiles.castleGround, 45, 12, 11, 2);
        Platform(ground, tiles.castleGround, 31, 19, 10, 2);
        Platform(ground, tiles.castleGround, 23, 7, 7, 2);
        Platform(ground, tiles.castleGround, 40, 7, 7, 2);
        Platform(ground, tiles.castlePlatform, 32, 14, 7, 2);
        Platform(ground, tiles.castleGround, 22, 23, 6, 2);
        Platform(ground, tiles.castleGround, 42, 23, 6, 2);
        AshPlatform(32, 27, 6, 1);
        AshPlatform(38, 33, 6, 1);
        AshPlatform(26, 33, 5, 1);
        AshPlatform(32, 39, 5, 1);
        DetailCluster(32, 18, 10, 10, tiles.castleArch);
        ColumnPair(18, 8, 25);
        ColumnPair(52, 8, 25);
    }

    private static void DrawArea_GearCorridor()
    {
        counter.areaCount++;
        DrawRoom("Area_05_齿轮廊道_冲刺", -28, 28, 52, 28, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, -6, 25, 18, 3);
        Platform(ground, tiles.castlePlatform, -19, 32, 9, 2);
        Platform(ground, tiles.castleGround, -33, 37, 12, 3);
        Platform(ground, tiles.castlePlatform, 5, 39, 6, 2);
        Platform(ground, tiles.castleGround, -2, 29, 6, 2);
        Platform(ground, tiles.castlePlatform, -25, 42, 6, 2);
        Platform(ground, tiles.castlePlatform, -10, 45, 5, 2);
        Platform(ground, tiles.castleGround, -2, 42, 8, 2);
        Hazard(-8, 28, 7, 1);
        DetailCluster(-3, 37, 6, 6, tiles.castleGearLike);
        DetailCluster(-18, 31, 5, 5, tiles.castleGearLike);
        Solid(details, tiles.castleChain, 8, 34, 1, 14);
    }

    private static void DrawArea_AshFoundry()
    {
        counter.areaCount++;
        DrawRoom("Area_06_熔灰工坊_火魔法", 48, 26, 48, 32, tiles.caveBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 52, 24, 28, 3);
        Platform(ground, tiles.castleGround, 74, 34, 14, 2);
        Platform(ground, tiles.castlePlatform, 56, 32, 6, 2);
        Platform(ground, tiles.castlePlatform, 66, 40, 7, 2);
        Platform(ground, tiles.castlePlatform, 86, 39, 6, 2);
        Platform(ground, tiles.castlePlatform, 91, 45, 5, 2);
        Platform(ground, tiles.castleGround, 93, 31, 7, 2);
        Hazard(62, 27, 13, 2);
        DetailCluster(57, 35, 6, 12, tiles.castleDamaged);
        Solid(details, tiles.castleWall, 57, 45, 3, 10);
        DetailCluster(70, 30, 16, 3, tiles.caveDetail);
    }

    private static void DrawArea_FrostBellRoom()
    {
        counter.areaCount++;
        DrawRoom("Area_07_静霜钟室_冰魔法", 80, 50, 44, 34, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 83, 47, 21, 3);
        Platform(ground, tiles.castleGround, 103, 56, 14, 2);
        Platform(ground, tiles.castlePlatform, 94, 63, 9, 2);
        Platform(ground, tiles.castleGround, 86, 54, 6, 2);
        Platform(ground, tiles.castlePlatform, 97, 60, 6, 2);
        Platform(ground, tiles.castleGround, 110, 68, 6, 2);
        Platform(ground, tiles.castlePlatform, 116, 62, 5, 2);
        Platform(ground, tiles.castleGround, 118, 54, 6, 2);
        Hazard(100, 51, 12, 1);
        DetailCluster(95, 60, 7, 9, tiles.castleArch);
        Solid(details, tiles.castleWindow, 85, 56, 2, 13);
        Solid(details, tiles.castleWindow, 113, 58, 2, 12);
    }

    private static void DrawArea_MemorialHall()
    {
        counter.areaCount++;
        DrawRoom("Area_08_古老纪念堂_旧日长廊", 28, 72, 66, 30, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 34, 69, 50, 3);
        Platform(ground, tiles.castleGround, 55, 75, 15, 2);
        Platform(ground, tiles.castleGround, 38, 75, 7, 2);
        Platform(ground, tiles.castleGround, 76, 75, 7, 2);
        Platform(ground, tiles.castleGround, 55, 84, 20, 2);
        Platform(ground, tiles.castleGround, 84, 78, 8, 2);
        for (int i = 0; i < 12; i++)
            MemorialStone(36 + i * 4, 72);
        DetailCluster(55, 80, 7, 12, tiles.castleArch);
    }

    private static void DrawArea_TowerClimb()
    {
        counter.areaCount++;
        DrawRoom("Area_09_主塔登顶路线", 18, 28, 32, 88, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 30, 20, 10, 2);
        Platform(ground, tiles.castleGround, 25, 68, 10, 2);
        Platform(ground, tiles.castleGround, 30, 102, 14, 2);
        Platform(ground, tiles.castleGround, 20, 37, 5, 2);
        Platform(ground, tiles.castleGround, 42, 46, 5, 2);
        Platform(ground, tiles.castleGround, 20, 80, 5, 2);
        Platform(ground, tiles.castleGround, 42, 91, 5, 2);
        int[,] ashenPlatforms =
        {
            { 36, 31, 6 }, { 25, 40, 6 }, { 38, 49, 6 }, { 24, 58, 6 },
            { 36, 72, 6 }, { 25, 82, 6 }, { 38, 92, 6 }, { 31, 100, 6 }
        };
        for (int i = 0; i < ashenPlatforms.GetLength(0); i++)
            AshPlatform(ashenPlatforms[i, 0], ashenPlatforms[i, 1], ashenPlatforms[i, 2], 1);
        Solid(details, tiles.castleChain, 31, 35, 1, 62);
    }

    private static void DrawArea_BossRoom()
    {
        counter.areaCount++;
        DrawRoom("Area_10_塔顶钟室_Boss", 4, 112, 60, 32, tiles.castleBackground, tiles.castleWall);
        Platform(ground, tiles.castleGround, 12, 109, 42, 3);
        Platform(ground, tiles.castlePlatform, 15, 119, 8, 2);
        Platform(ground, tiles.castlePlatform, 45, 119, 8, 2);
        Platform(ground, tiles.castlePlatform, 30, 118, 7, 2);
        Platform(ground, tiles.castlePlatform, 21, 126, 6, 2);
        Platform(ground, tiles.castlePlatform, 41, 126, 6, 2);
        AshPlatform(25, 116, 6, 1);
        AshPlatform(38, 116, 6, 1);
        DetailCluster(31, 123, 16, 14, tiles.castleArch);
        Solid(details, tiles.castleChain, 31, 116, 2, 11);
    }

    private static void DrawRoom(string name, int x, int y, int width, int height, TileBase backgroundTile, TileBase wallTile)
    {
        Fill(background, backgroundTile, x, y, x + width - 1, y + height - 1, false);
        Fill(walls, wallTile, x, y, x, y + height - 1, true);
        Fill(walls, wallTile, x + width - 1, y, x + width - 1, y + height - 1, true);
        Fill(walls, wallTile, x, y + height - 1, x + width - 1, y + height - 1, true);
        Fill(details, tiles.castleDamaged, x + 3, y + 5, x + width - 4, y + 5, false);
        Fill(details, tiles.castleDamaged, x + 5, y + height - 6, x + width - 6, y + height - 6, false);
        CreateMarker(root, "Label_" + name, new Vector2(x + width * 0.5f, y + height + 2), name);
        RecordRect("Room", name, x, y, width, height, "完整房间边界，包含背景、左右墙、顶墙和内部路线");
        RecordRect("Walls", name + "_左墙", x, y, 1, height, "房间左边界");
        RecordRect("Walls", name + "_右墙", x + width - 1, y, 1, height, "房间右边界");
        RecordRect("Walls", name + "_顶墙", x, y + height - 1, width, 1, "房间上边界");
    }

    private static void Platform(Tilemap tilemap, TileBase tile, int x, int y, int width, int height)
    {
        Solid(tilemap, tile, x, y, width, height);
        Fill(details, tiles.castlePlatformTop, x, y + height, x + width - 1, y + height, false);
        RecordRect("Terrain/Platforms", "Platform", x, y, width, height, "可行走地面或跳台占位");
    }

    private static void Solid(Tilemap tilemap, TileBase tile, int x, int y, int width, int height)
    {
        Fill(tilemap, tile, x, y, x + width - 1, y + height - 1, true);
    }

    private static void AshPlatform(int x, int y, int width, int height)
    {
        Fill(ashen, tiles.ashenTile, x, y, x + width - 1, y + height - 1, false);
        counter.ashenTileCount += width * height;
        RecordRect("AshenPlatforms", "AshPlatform", x, y, width, height, "灰烬态才出现的路线平台");
    }

    private static void Hazard(int x, int y, int width, int height)
    {
        Fill(hazards, tiles.hazardTile, x, y, x + width - 1, y + height - 1, false);
        counter.hazardTileCount += width * height;
        RecordRect("Mechanisms/Hazards", "Hazard", x, y, width, height, "危险区域或机关占位");
    }

    private static void DetailCluster(int x, int y, int width, int height, TileBase tile)
    {
        Fill(details, tile, x, y, x + width - 1, y + height - 1, false);
    }

    private static void WindowRow(int x, int y, int count)
    {
        for (int i = 0; i < count; i++)
            Solid(details, tiles.castleWindow, x + i * 4, y, 2, 4);
    }

    private static void ColumnPair(int x, int y, int height)
    {
        Solid(details, tiles.castleColumn, x, y, 2, height);
    }

    private static void MemorialStone(int x, int y)
    {
        Solid(details, tiles.castleColumn, x, y, 2, 5);
    }

    private static void CreateGameplayPlaceholders()
    {
        CreateMarker(root, "Start_PlayerSpawn", new Vector2(-90, 1), "玩家起点");
        RecordCoordinate("TriggerPoints", "Start_PlayerSpawn", new Vector2(-90, 1), Vector2.one, "玩家起点");
        RecordAbilityRoomFlowOverview();
        CreateSavePoint("SavePoint_篝火_荒凉小径", new Vector2(-70, 1));
        CreateSavePoint("SavePoint_篝火_钟塔大厅", new Vector2(25, 1));
        CreateSavePoint("SavePoint_篝火_古老纪念堂入口", new Vector2(35, 72));
        CreateSavePoint("SavePoint_篝火_Boss前", new Vector2(10, 112));

        CreateAbility("Ability_灰烬态", new Vector2(-49, 2), "灰烬态");
        CreateAbility("Ability_投掷宝剑", new Vector2(6, 7), "投掷宝剑");
        CreateAbility("Ability_冲刺", new Vector2(-30, 41), "冲刺");
        CreateAbility("Ability_火之魔法", new Vector2(84, 42), "火之魔法");
        CreateAbility("Ability_冰之魔法", new Vector2(108, 59), "冰之魔法");
        CreateAbility("Ability_旧日赐福_无限灰烬态", new Vector2(56, 78), "旧日赐福");

        CreateMechanism("Mechanism_投掷宝剑_远程机关", new Vector2(24, 14), new Vector2(2.2f, 2.2f), "教学段：用投掷宝剑命中远程机关");
        CreateGate("Gate_需要投掷宝剑_塔门出口", new Vector2(29, 5), new Vector2(2.5f, 7f), "能力门：投掷宝剑打开塔门，进入钟塔大厅");

        CreateMechanism("Mechanism_冲刺_宽裂隙", new Vector2(-8, 36), new Vector2(8f, 2f), "教学段：获得冲刺后跨越宽裂隙");
        CreateGate("Gate_需要冲刺_齿轮廊道出口", new Vector2(3, 43), new Vector2(3f, 6f), "能力门：冲刺通过齿轮廊道出口");

        CreateMechanism("Mechanism_火魔法_火炬机关", new Vector2(93, 43), new Vector2(2.2f, 3f), "教学段：用火之魔法点燃机关");
        CreateGate("Gate_需要火魔法_熔灰工坊出口", new Vector2(96, 34), new Vector2(3f, 7f), "能力门：火魔法开启工坊出口");

        CreateMechanism("Mechanism_冰魔法_冻结水面", new Vector2(105, 52), new Vector2(12f, 2f), "教学段：用冰之魔法冻结水面或暂停机关");
        CreateGate("Gate_需要冰魔法_静霜钟室出口", new Vector2(120, 58), new Vector2(3f, 8f), "能力门：冰魔法通过静霜机关");

        CreateMechanism("Mechanism_旧日赐福_中央石碑", new Vector2(56, 80), new Vector2(5f, 6f), "教学段：与中央石碑交互获得旧日赐福");
        CreateGate("Gate_需要旧日赐福_主塔登顶入口", new Vector2(86, 79), new Vector2(4f, 8f), "能力门：旧日赐福后进入主塔登顶路线");

        CreatePortal("Portal_钟塔前庭_进入钟塔大厅", new Vector2(29, 5), "目标：钟塔大厅");
        CreatePortal("Portal_钟塔大厅_返回钟塔前庭", new Vector2(18, 2), "目标：钟塔前庭");
        CreatePortal("Portal_古老纪念堂_回到钟塔大厅", new Vector2(84, 73), "目标：钟塔大厅");
        CreatePortal("Portal_塔顶钟室_结局出口", new Vector2(56, 112), "目标：结束 CG");

        EnemyWave("荒凉小径", false, new Vector2(-84, 1), new Vector2(-73, 10), new Vector2(-63, 6), new Vector2(-55, 8));
        EnemyWave("钟塔前庭", false, new Vector2(-10, 1), new Vector2(2, 2), new Vector2(10, 6), new Vector2(18, 9));
        EnemyWave("齿轮廊道", false, new Vector2(3, 27), new Vector2(-12, 34), new Vector2(-29, 40), new Vector2(-2, 32));
        EnemyWave("熔灰工坊", false, new Vector2(57, 28), new Vector2(67, 42), new Vector2(83, 41), new Vector2(77, 28));
        EnemyWave("静霜钟室", false, new Vector2(87, 50), new Vector2(98, 62), new Vector2(111, 70), new Vector2(105, 50));
        EnemyWave("主塔登顶", false, new Vector2(21, 39), new Vector2(39, 48), new Vector2(25, 70), new Vector2(40, 94));
        EnemyWave("Boss前残响", false, new Vector2(20, 112), new Vector2(42, 112), new Vector2(30, 120));
        CreateEnemy("Boss_塔顶守护者", new Vector2(31, 113), true);

        CreateNarrativeTrigger("Narrative_开场落地点", new Vector2(-90, 2), new Vector2(6, 4), false, false,
            "钟声停下的那一刻，灰烬开始倒流。",
            "你在荒凉小径醒来，远处的钟塔没有任何回响。");
        CreateNarrativeTrigger("Narrative_断桥前", new Vector2(-46, 3), new Vector2(6, 5), true, false,
            "前方的桥已经断裂。",
            "灰烬态会让旧日的桥短暂显现。");
        CreateNarrativeTrigger("Narrative_钟塔大厅", new Vector2(24, 3), new Vector2(8, 5), false, false,
            "钟塔大厅向上延伸，像一口倒置的井。",
            "塔顶道路还需要更多能力与旧日赐福。");
        CreateNarrativeTrigger("Narrative_中央石碑", new Vector2(56, 80), new Vector2(8, 5), true, false,
            "中央石碑回应了你的灰烬。",
            "旧日赐福：灰烬态不再消耗灰烬值。");
        CreateNarrativeTrigger("Narrative_Boss前", new Vector2(12, 113), new Vector2(7, 5), true, false,
            "塔顶的钟室就在前方。",
            "如果钟声再次响起，灰烬会走向哪里？");
    }

    private static void EnemyWave(string area, bool elite, params Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            CreateEnemy("Enemy_" + area + "_" + (i + 1).ToString("00"), positions[i], elite);
    }

    private static void CreateEnemy(string name, Vector2 position, bool elite)
    {
        GameObject enemy = CreateBoxMarker(name, position, elite ? new Vector2(3, 3) : new Vector2(1.6f, 1.8f), elite ? new Color(0.65f, 0.05f, 0.05f, 0.8f) : new Color(0.95f, 0.08f, 0.08f, 0.8f));
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
            enemy.layer = enemyLayer;
        counter.enemyCount++;
        RecordCoordinate("Enemies", name, position, elite ? new Vector2(3, 3) : new Vector2(1.6f, 1.8f), elite ? "精英/Boss敌人占位" : "普通敌人占位");
    }

    private static void CreateAbility(string name, Vector2 position, string label)
    {
        CreateBoxMarker(name, position, new Vector2(1.8f, 1.8f), new Color(1f, 0.75f, 0.15f, 0.85f));
        CreateMarker(root, "Label_" + name, position + Vector2.up * 2.2f, label);
        counter.abilityCount++;
        RecordCoordinate("AbilityPoints", name, position, new Vector2(1.8f, 1.8f), "能力点位于房间中后段，前置普通挑战之后获取");
    }

    private static void CreateSavePoint(string name, Vector2 position)
    {
        CreateBoxMarker(name, position, new Vector2(1.5f, 2f), new Color(1f, 0.45f, 0.05f, 0.8f));
        RecordCoordinate("SavePoints", name, position, new Vector2(1.5f, 2f), "篝火/存档点占位");
    }

    private static void CreateMechanism(string name, Vector2 position, Vector2 size, string label)
    {
        // 机关占位用于标出获得能力后的立即练习段，不直接写入具体玩法逻辑。
        CreateBoxMarker(name, position, size, new Color(0.75f, 0.25f, 1f, 0.55f));
        CreateMarker(root, "Label_" + name, position + Vector2.up * (size.y * 0.5f + 1.2f), label);
        counter.mechanismCount++;
        RecordCoordinate("Mechanisms/Teaching", name, position, size, label);
    }

    private static void CreateGate(string name, Vector2 position, Vector2 size, string label)
    {
        // 能力门只作为地图蓝图提示，后续可手动替换为真实机关或传送门逻辑。
        CreateBoxMarker(name, position, size, new Color(0.15f, 0.35f, 1f, 0.45f));
        CreateMarker(root, "Label_" + name, position + Vector2.up * (size.y * 0.5f + 1.2f), label);
        counter.mechanismCount++;
        RecordCoordinate("Gates/Exits", name, position, size, label);
    }

    private static void CreatePortal(string name, Vector2 position, string label)
    {
        DrawPortalFrame(position);
        CreateBoxMarker(name, position, new Vector2(2.4f, 4.2f), new Color(0.2f, 0.75f, 1f, 0.45f));
        CreateMarker(root, "Label_" + name, position + Vector2.up * 3.1f, label);
        counter.portalCount++;
        RecordCoordinate("Portals/Exits", name, position, new Vector2(2.4f, 4.2f), label);
    }

    private static void DrawPortalFrame(Vector2 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        Solid(details, tiles.castleArch, x - 2, y - 2, 1, 5);
        Solid(details, tiles.castleArch, x + 2, y - 2, 1, 5);
        Solid(details, tiles.castleArch, x - 2, y + 2, 5, 1);
        Solid(details, tiles.castleWindow, x - 1, y - 1, 3, 3);
    }

    private static GameObject CreateBoxMarker(string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject marker = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(marker, "Create Gameplay Placeholder");
        marker.transform.SetParent(root);
        marker.transform.position = ToWorld(position);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = 20;

        Vector2 scaledSize = ToWorld(size);
        marker.transform.localScale = new Vector3(scaledSize.x, scaledSize.y, 1f);

        BoxCollider2D collider = marker.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        return marker;
    }

    private static void CreateNarrativeTrigger(string name, Vector2 position, Vector2 size, bool requireInteract, bool requireAshenPhase, params string[] lines)
    {
        GameObject triggerObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(triggerObject, "Create Narrative Trigger");
        triggerObject.transform.SetParent(root);
        triggerObject.transform.position = ToWorld(position);

        BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = ToWorld(size);

        NarrativeTrigger trigger = triggerObject.AddComponent<NarrativeTrigger>();
        SerializedObject serializedTrigger = new SerializedObject(trigger);
        SetStringArray(serializedTrigger.FindProperty("dialogueLines"), lines);
        SetStringProperty(serializedTrigger, "triggerId", name);
        SetBoolProperty(serializedTrigger, "triggerOnce", !requireInteract);
        SetBoolProperty(serializedTrigger, "requireInteractKey", requireInteract);
        SetIntProperty(serializedTrigger, "interactKey", (int)KeyCode.E);
        SetBoolProperty(serializedTrigger, "requireAshenPhase", requireAshenPhase);
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        counter.narrativeCount++;
        RecordCoordinate("TriggerPoints", name, position, size, requireInteract ? "需要按 E 交互触发" : "进入范围自动触发");
    }

    private static void CreateMarker(Transform parent, string name, Vector2 position, string text = "")
    {
        GameObject marker = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(marker, "Create Map Marker");
        marker.transform.SetParent(parent);
        if (parent == root)
            marker.transform.position = ToWorld(position);
        else
            marker.transform.localPosition = ToWorld(position);

        if (string.IsNullOrEmpty(text))
            return;

        TextMesh textMesh = marker.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = new Color(1f, 0.92f, 0.58f, 1f);
        textMesh.fontSize = 32;
        textMesh.characterSize = 0.18f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }

    private static Vector2 ToWorld(Vector2 value)
    {
        return value * Scale;
    }

    private static void Fill(Tilemap tilemap, TileBase tile, int xMin, int yMin, int xMax, int yMax, bool solid)
    {
        if (tile == null)
            return;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x * Scale, y * Scale, 0), tile);
                for (int sx = 0; sx < Scale; sx++)
                {
                    for (int sy = 0; sy < Scale; sy++)
                    {
                        tilemap.SetTile(new Vector3Int(x * Scale + sx, y * Scale + sy, 0), tile);
                    }
                }

                if (solid)
                    counter.solidTileCount += Scale * Scale;
                else
                    counter.detailTileCount += Scale * Scale;
            }
        }
    }

    private static void RecordAbilityRoomFlowOverview()
    {
        if (coordinateLog == null)
            return;

        coordinateLog.AppendLine("[AbilityRoomFlow]");
        coordinateLog.AppendLine("投掷宝剑：钟塔前庭入口 -> 普通平台和敌人 -> 中后段获得投掷宝剑 -> 远程机关教学 -> 塔门出口。");
        coordinateLog.AppendLine("冲刺：齿轮廊道入口 -> 齿轮平台和敌人 -> 中后段获得冲刺 -> 宽裂隙教学 -> 走廊出口。");
        coordinateLog.AppendLine("火之魔法：熔灰工坊入口 -> 熔灰平台和危险地形 -> 中后段获得火魔法 -> 火炬机关教学 -> 工坊出口。");
        coordinateLog.AppendLine("冰之魔法：静霜钟室入口 -> 冰霜平台和敌人 -> 中后段获得冰魔法 -> 冻结水面/暂停机关教学 -> 钟室出口。");
        coordinateLog.AppendLine("旧日赐福：旧日长廊入口 -> 多个石碑交互 -> 中央赐福石碑 -> 主塔登顶入口。");
        coordinateLog.AppendLine();
    }

    private static void RecordRect(string category, string name, int x, int y, int width, int height, string note)
    {
        RecordCoordinate(category, name, new Vector2(x, y), new Vector2(width, height), note);
    }

    private static void RecordCoordinate(string category, string name, Vector2 position, Vector2 size, string note)
    {
        if (coordinateLog == null)
            return;

        coordinateLog.AppendLine(
            "[" + category + "] " + name +
            " | position=(" + FormatFloat(position.x) + ", " + FormatFloat(position.y) + ")" +
            " | size=(" + FormatFloat(size.x) + ", " + FormatFloat(size.y) + ")" +
            " | " + note);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.##");
    }

    private static void ExportCoordinateList()
    {
        if (coordinateLog == null)
            return;

        string folder = "Assets/Generated";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, "AshenClocktower_TilemapBlueprint_Coordinates.txt");
        File.WriteAllText(path, coordinateLog.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log("已导出《灰烬钟塔》Tilemap 蓝图坐标清单：" + path);
    }

    private static TilePaletteSet LoadTiles()
    {
        TilePaletteSet set = new TilePaletteSet();
        set.castleGround = LoadTile("Assets/Palette/Castle/Tileset/floor_tile_1.asset", "Assets/Palette/Castle/Tileset", "floor");
        set.castlePlatform = LoadTile("Assets/Palette/Castle/Tileset/platform_1.asset", "Assets/Palette/Castle/Tileset", "platform");
        set.castlePlatformTop = LoadTile("Assets/Palette/Castle/Tileset/platform_2.asset", "Assets/Palette/Castle/Tileset", "platform");
        set.castleWall = LoadTile("Assets/Palette/Castle/Tileset/wall_1.asset", "Assets/Palette/Castle/Tileset", "wall");
        set.castleBackground = LoadTile("Assets/Palette/Castle/Tileset/brick_1.asset", "Assets/Palette/Castle/Tileset", "brick");
        set.castleDamaged = LoadTile("Assets/Palette/Castle/Tileset/damaged_brick_1.asset", "Assets/Palette/Castle/Tileset", "damaged");
        set.castleWindow = LoadTile("Assets/Palette/Castle/Tileset/window_small_1.asset", "Assets/Palette/Castle/Tileset", "window");
        set.castleColumn = LoadTile("Assets/Palette/Castle/Tileset/column_1.asset", "Assets/Palette/Castle/Tileset", "column");
        set.castleArch = LoadTile("Assets/Palette/Castle/Tileset/arch_1.asset", "Assets/Palette/Castle/Tileset", "arch");
        set.castleChain = LoadTile("Assets/Palette/Castle/Tileset/merlons_1.asset", "Assets/Palette/Castle/Tileset", "merlons");
        set.castleGearLike = LoadTile("Assets/Palette/Castle/Tileset/lion_column_1.asset", "Assets/Palette/Castle/Tileset", "lion");
        set.hazardTile = LoadTile("Assets/Palette/Castle/Tileset/spikes.asset", "Assets/Palette/Castle/Tileset", "spikes");

        set.hauntGround = LoadTile("Assets/Palette/Haunt/Tileset/haunt_0.asset", "Assets/Palette/Haunt/Tileset", "haunt");
        set.hauntWall = LoadTile("Assets/Palette/Haunt/Tileset/haunt_1.asset", "Assets/Palette/Haunt/Tileset", "haunt");
        set.hauntBackground = LoadTile("Assets/Palette/Haunt/Tileset/haunt_2.asset", "Assets/Palette/Haunt/Tileset", "haunt");
        set.hauntDetail = LoadTile("Assets/Palette/Haunt/Tileset/haunt_3.asset", "Assets/Palette/Haunt/Tileset", "haunt");

        set.caveBackground = LoadTile("Assets/Palette/Cave/Tileset/fantasy_4colors_tileset_frame_0_0.asset", "Assets/Palette/Cave/Tileset", "fantasy");
        set.caveDetail = LoadTile("Assets/Palette/Cave/Tileset/fantasy_4colors_tileset_frame_0_1.asset", "Assets/Palette/Cave/Tileset", "fantasy");
        set.ashenTile = set.hauntDetail != null ? set.hauntDetail : set.castlePlatform;
        return set;
    }

    private static TileBase LoadTile(string preferredPath, string fallbackFolder, string keyword)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(preferredPath);
        if (tile != null)
            return tile;

        string[] guids = AssetDatabase.FindAssets("t:TileBase " + keyword, new[] { fallbackFolder });
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("t:TileBase", new[] { fallbackFolder });

        if (guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<TileBase>(path);
    }

    private static void AddAshenObjectIfAvailable(GameObject target)
    {
        Type type = Type.GetType("AshenObject");
        if (type != null && typeof(Component).IsAssignableFrom(type))
            Undo.AddComponent(target, type);
        else if (target.GetComponent<AshenObject>() == null)
            target.AddComponent<AshenObject>();
    }

    private static void SetStringArray(SerializedProperty property, string[] values)
    {
        if (property == null)
            return;

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).stringValue = values[i];
    }

    private static void SetStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.stringValue = value;
    }

    private static void SetBoolProperty(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private static void SetIntProperty(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.intValue = value;
    }

    private class TilePaletteSet
    {
        public TileBase castleGround;
        public TileBase castlePlatform;
        public TileBase castlePlatformTop;
        public TileBase castleWall;
        public TileBase castleBackground;
        public TileBase castleDamaged;
        public TileBase castleWindow;
        public TileBase castleColumn;
        public TileBase castleArch;
        public TileBase castleChain;
        public TileBase castleGearLike;
        public TileBase hazardTile;
        public TileBase hauntGround;
        public TileBase hauntWall;
        public TileBase hauntBackground;
        public TileBase hauntDetail;
        public TileBase caveBackground;
        public TileBase caveDetail;
        public TileBase ashenTile;

        public bool HasRequiredTiles
        {
            get { return castleGround != null && castleWall != null && hauntGround != null; }
        }
    }

    private class MapCounter
    {
        public int areaCount;
        public int solidTileCount;
        public int detailTileCount;
        public int ashenTileCount;
        public int hazardTileCount;
        public int enemyCount;
        public int portalCount;
        public int abilityCount;
        public int mechanismCount;
        public int narrativeCount;
    }
}
