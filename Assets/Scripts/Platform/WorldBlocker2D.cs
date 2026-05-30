using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class WorldBlocker2D : MonoBehaviour
{
    private const string PlayerLayerName = "Player";
    private const string WorldBoundaryLayerName = "WorldBoundary";
    private const string NoClimbWallLayerName = "NoClimbWall";

    public enum BlockerType
    {
        WorldBoundary,
        NoClimbWall
    }

    [Header("Blocker")]
    [SerializeField] private BlockerType blockerType = BlockerType.WorldBoundary;
    [SerializeField] private Vector2 colliderSize = new Vector2(1f, 8f);
    [SerializeField] private Vector2 colliderOffset;

    private BoxCollider2D boxCollider;

    public void Configure(BlockerType type, Vector2 size)
    {
        blockerType = type;
        colliderSize = size;
        ApplySettings();
    }

    private void Reset()
    {
        ApplySettings();
    }

    private void Awake()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        EnsureCollider();
        ApplyLayer();
        EnsurePlayerCollision();
    }

    private void EnsureCollider()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        boxCollider.isTrigger = false;
        boxCollider.usedByEffector = false;
        boxCollider.size = colliderSize;
        boxCollider.offset = colliderOffset;
    }

    private void ApplyLayer()
    {
        string layerName = blockerType == BlockerType.WorldBoundary
            ? WorldBoundaryLayerName
            : NoClimbWallLayerName;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
            gameObject.layer = layer;
    }

    private void EnsurePlayerCollision()
    {
        int playerLayer = LayerMask.NameToLayer(PlayerLayerName);
        int blockerLayer = gameObject.layer;

        if (playerLayer >= 0 && blockerLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, blockerLayer, false);
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = blockerType == BlockerType.WorldBoundary
            ? new Color(1f, 0.35f, 0.1f, 0.25f)
            : new Color(0.15f, 0.65f, 1f, 0.25f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(colliderOffset, colliderSize);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.9f);
        Gizmos.DrawWireCube(colliderOffset, colliderSize);
    }
}
