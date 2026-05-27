using UnityEngine;

public class AshenWorldManager : MonoBehaviour
{
    public static AshenWorldManager Instance { get; private set; }

    private AshenObject[] ashenObjects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 AshenWorldManager，请确认只保留一个。");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshAshenObjects();
        SetAshenWorldActive(false);
    }

    public void RefreshAshenObjects()
    {
        // 自动查找场景中挂了 AshenObject 的物体。AshenObject 不会被 SetActive(false)，所以这里能稳定找到。
        ashenObjects = FindObjectsOfType<AshenObject>(true);
    }

    public void SetAshenWorldActive(bool active)
    {
        if (ashenObjects == null || ashenObjects.Length == 0)
        {
            RefreshAshenObjects();
        }

        for (int i = 0; i < ashenObjects.Length; i++)
        {
            if (ashenObjects[i] != null)
            {
                ashenObjects[i].SetAshenVisible(active);
            }
        }
    }
}
