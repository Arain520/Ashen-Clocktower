using UnityEngine;

/// <summary>
/// 能力门占位脚本。
/// 它不会真正阻挡或解锁流程，只记录这道门未来需要接入什么能力。
/// </summary>
public class GeneratedAbilityGate : MonoBehaviour
{
    [SerializeField] private string requiredAbility;
    [TextArea(2, 4)]
    [SerializeField] private string designNote;

    public void Setup(string ability, string note)
    {
        requiredAbility = ability;
        designNote = note;
    }
}
