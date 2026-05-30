using UnityEditor;
using UnityEngine;

public static class WorldBlocker2DCreator
{
    [MenuItem("GameObject/Metroidvania/World Boundary", false, 10)]
    private static void CreateWorldBoundary(MenuCommand menuCommand)
    {
        CreateBlocker(
            "WorldBoundary",
            WorldBlocker2D.BlockerType.WorldBoundary,
            new Vector2(1f, 12f),
            menuCommand);
    }

    [MenuItem("GameObject/Metroidvania/No Climb Wall", false, 11)]
    private static void CreateNoClimbWall(MenuCommand menuCommand)
    {
        CreateBlocker(
            "NoClimbWall",
            WorldBlocker2D.BlockerType.NoClimbWall,
            new Vector2(1f, 8f),
            menuCommand);
    }

    private static void CreateBlocker(
        string objectName,
        WorldBlocker2D.BlockerType blockerType,
        Vector2 size,
        MenuCommand menuCommand)
    {
        GameObject blocker = new GameObject(objectName);
        GameObjectUtility.SetParentAndAlign(blocker, menuCommand.context as GameObject);

        WorldBlocker2D worldBlocker = blocker.AddComponent<WorldBlocker2D>();
        worldBlocker.Configure(blockerType, size);

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
            blocker.transform.position = sceneView.pivot;

        Undo.RegisterCreatedObjectUndo(blocker, "Create " + objectName);
        Selection.activeObject = blocker;
    }
}
