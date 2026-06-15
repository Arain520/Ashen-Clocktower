using UnityEngine;

/// <summary>
/// 生成地图使用的通用标记脚本。
/// 它只负责在 Scene 视图中显示占位范围，不参与正式玩法逻辑。
/// </summary>
public class GeneratedMapMarker : MonoBehaviour
{
    public enum MarkerShape
    {
        Box,
        Circle
    }

    [Header("Marker Info")]
    [SerializeField] private string markerType = "占位";
    [TextArea(2, 4)]
    [SerializeField] private string note;

    [Header("Gizmo")]
    [SerializeField] private MarkerShape shape = MarkerShape.Box;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0.1f, 0.35f);
    [SerializeField] private Vector2 gizmoSize = Vector2.one;
    [SerializeField] private bool drawGizmo = true;

    public void Setup(string type, string markerNote, Color color, Vector2 size, MarkerShape markerShape)
    {
        markerType = type;
        note = markerNote;
        gizmoColor = color;
        gizmoSize = size;
        shape = markerShape;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmo)
            return;

        Gizmos.color = gizmoColor;

        if (shape == MarkerShape.Circle)
        {
            float radius = Mathf.Max(gizmoSize.x, gizmoSize.y) * 0.5f;
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
            Vector3 size = new Vector3(gizmoSize.x, gizmoSize.y, 0.1f);
            Gizmos.DrawCube(transform.position, size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
