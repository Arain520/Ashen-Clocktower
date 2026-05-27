using UnityEngine;

public class AshenObject : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] private bool visibleOnlyInAshenPhase = true;

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void Start()
    {
        // 普通状态下默认隐藏灰烬世界物体，但不 SetActive(false)，避免管理器之后找不到它。
        SetAshenVisible(!visibleOnlyInAshenPhase);
    }

    public void SetAshenVisible(bool visible)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].enabled = visible;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = visible;
        }
    }
}
