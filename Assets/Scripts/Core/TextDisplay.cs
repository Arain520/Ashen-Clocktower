using UnityEngine;
using TMPro;
using System.Collections;

public class TextDisplay : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI pageText;  // 文本组件
    [SerializeField] private string[] pages;            // 存储每一页的文本
    [SerializeField] private int currentPage = 0;       // 当前页码
    [SerializeField] private GameObject nextButton;     // 下一页按钮
    [SerializeField] private GameObject rawImage;       // 对话框（RawImage）

    [Header("打字效果设置")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字速度，控制文字的逐字显示

    private bool isTyping = false;  // 是否正在打字
    private string currentText = ""; // 当前正在显示的文本

    private Interactable interactableScript;  // 引用 Interactable 脚本

    private void Start()
    {
        interactableScript = FindObjectOfType<Interactable>();  // 获取 Interactable 脚本
        rawImage.SetActive(false);  // 开场时隐藏对话框
        nextButton.SetActive(false);  // 初始化时隐藏按钮
        nextButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(NextPage);  // 为按钮添加点击事件监听
    }

    // 显示当前页的内容
    public void ShowPage()
    {
        rawImage.SetActive(true);  // 显示对话框
        if (pages.Length > 0 && currentPage < pages.Length)
        {
            currentText = pages[currentPage];
            pageText.text = "";  // 清空之前的文本
            StopAllCoroutines(); // 停止任何打字过程
            StartCoroutine(TypeText(currentText)); // 开始打字显示文本

            // 禁用交互（通过 TextDisplay 脚本）
            if (interactableScript != null)
            {
                interactableScript.SetCanInteract(false);  // 禁用交互
            }
        }
    }

    // 打字效果
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        for (int i = 0; i < text.Length; i++)
        {
            pageText.text += text[i];  // 逐个字符显示
            yield return new WaitForSeconds(typingSpeed); // 控制打字速度
        }
        isTyping = false;
        nextButton.SetActive(true);  // 打字完成后显示下一页按钮
    }

    // 下一页
    public void NextPage()
    {
        if (!isTyping)  // 如果当前不是正在打字
        {
            if (currentPage < pages.Length - 1)
            {
                currentPage++;  // 增加页码
                ShowPage();     // 显示下一页
            }
            else
            {
                nextButton.SetActive(false);  // 如果没有更多页，隐藏按钮
                pageText.text = "没有更多的内容了！";  // 显示结束提示
                // 关闭对话框
                Invoke("HideDialogueBox", 2f);  // 等待2秒后隐藏对话框
            }
        }
    }

    // 隐藏对话框
    private void HideDialogueBox()
    {
        rawImage.SetActive(false);  // 隐藏对话框

        // 恢复交互
        if (interactableScript != null)
        {
            interactableScript.SetCanInteract(true);  // 启用交互
        }

        // 重置 currentPage，确保文案不会重复播放
        currentPage = 0;  // 你可以根据需要改变是否重置为第一页
    }
}