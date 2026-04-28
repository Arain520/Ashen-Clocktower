using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    [Header("交互提示图片")]
    [SerializeField] private GameObject interactableHint;  // 物品的子物体，用于显示提示（例如图片或图标）

    [Header("联动控制")]
    [SerializeField] private TextDisplay textDisplay;  // 用于控制翻页和显示文案的 TextDisplay 脚本

    private bool isPlayerInRange = false;  // 玩家是否在物品范围内
    private bool canInteract = false;  // 是否允许玩家进行交互，只有在玩家靠近物品时才为 true

    // 公共属性以便 TextDisplay 访问
    public bool IsPlayerInRange => isPlayerInRange;
    public bool CanInteract => canInteract;

    private void Update()
    {
        // 只有当玩家在交互范围内并且可以交互时，按 E 键才会触发交互
        if (isPlayerInRange && canInteract && Input.GetKeyDown(KeyCode.E))
        {
            Interact();  // 执行交互操作
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // 玩家进入交互范围
        {
            Debug.Log("Player entered the interaction range.");
            isPlayerInRange = true;
            ShowInteractableHint(true);  // 显示交互提示图片
            canInteract = true;  // 允许交互
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // 玩家离开交互范围
        {
            Debug.Log("Player exited the interaction range.");
            isPlayerInRange = false;
            ShowInteractableHint(false);  // 隐藏交互提示图片
            canInteract = false;  // 禁止交互
        }
    }

    // 显示或隐藏交互提示图片（物体的子物体）
    private void ShowInteractableHint(bool show)
    {
        if (interactableHint != null)
        {
            interactableHint.SetActive(show);  // 设置子物体的显示或隐藏
        }
    }

    // 执行交互操作
    private void Interact()
    {
        if (textDisplay != null)
        {
            textDisplay.ShowPage();  // 展开文案内容
        }
    }

    // 设置是否可以交互
    public void SetCanInteract(bool value)
    {
        canInteract = value;  // 设置是否允许交互
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);  // 可视化物品的交互范围
    }
}