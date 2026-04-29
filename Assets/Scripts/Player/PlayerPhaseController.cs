using UnityEngine;

public class PlayerPhaseController : MonoBehaviour
{
    [Header("Phase Manager")]
    [SerializeField] private PhaseManager phaseManager;  // 引用 PhaseManager 脚本

    private void Update()
    {
        // 玩家按下 R 键时切换相位
        if (Input.GetKeyDown(KeyCode.R))  // 按 R 键触发切换相位
        {
            Debug.Log("Toggling phase...");  // 调试信息
            phaseManager.TogglePhase();  // 调用 PhaseManager 的切换相位方法
        }
    }
}