using UnityEngine;

/// <summary>
/// 生成地图区域文字标注。
/// 只用于关卡原型阶段，方便在 Scene 视图中识别区域。
/// </summary>
[RequireComponent(typeof(TextMesh))]
public class MapAreaLabel : MonoBehaviour
{
    [SerializeField] private string areaName;

    public void Setup(string label, Color color, int fontSize = 48)
    {
        areaName = label;

        TextMesh textMesh = GetComponent<TextMesh>();
        textMesh.text = label;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }
}
