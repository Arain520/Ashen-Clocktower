using UnityEngine;

/// <summary>
/// 单向平台配置脚本。
/// 挂到需要变成单向平台的物体上后，会自动准备 Collider2D 和 PlatformEffector2D。
/// </summary>
[DisallowMultipleComponent]
public class OneWayPlatformSetup : MonoBehaviour
{
    [Header("One Way Platform")]
    [Tooltip("可站立表面的角度范围。180 表示只阻挡来自平台上方的碰撞。")]
    [SerializeField, Range(0f, 360f)] private float surfaceArc = 180f;

    [Tooltip("单向表面的旋转偏移。平台正面不是朝上时，可以调整这个值。")]
    [SerializeField, Range(-180f, 180f)] private float rotationalOffset = 0f;

    private Collider2D platformCollider;
    private PlatformEffector2D platformEffector;

    private void Reset()
    {
        // 添加脚本时立即配置一次，方便在 Inspector 中直接看到组件。
        ConfigureOneWayPlatform();
    }

    private void Awake()
    {
        // 运行时兜底配置，避免 prefab 或场景对象漏配组件。
        ConfigureOneWayPlatform();
    }

    private void OnValidate()
    {
        // Inspector 数值变化时同步到 PlatformEffector2D。
        ConfigureOneWayPlatform();
    }

    private void ConfigureOneWayPlatform()
    {
        EnsureCollider();
        EnsureEffector();
        ApplyEffectorSettings();
    }

    private void EnsureCollider()
    {
        // 优先使用 CompositeCollider2D，适配 TilemapCollider2D + CompositeCollider2D 的平台。
        platformCollider = GetComponent<CompositeCollider2D>();

        if (platformCollider == null)
        {
            // 其次使用物体上已有的 Collider2D，不额外影响其他普通平台。
            platformCollider = GetComponent<Collider2D>();
        }

        if (platformCollider == null)
        {
            // 没有碰撞体时，自动添加 BoxCollider2D 作为默认平台碰撞体。
            platformCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // 让碰撞体交给 PlatformEffector2D 处理单向碰撞。
        platformCollider.usedByEffector = true;
    }

    private void EnsureEffector()
    {
        // 获取已有的 PlatformEffector2D；没有则自动添加。
        platformEffector = GetComponent<PlatformEffector2D>();

        if (platformEffector == null)
        {
            platformEffector = gameObject.AddComponent<PlatformEffector2D>();
        }
    }

    private void ApplyEffectorSettings()
    {
        if (platformEffector == null)
            return;

        // 开启 Unity 2D 自带的单向平台逻辑。
        platformEffector.useOneWay = true;
        platformEffector.surfaceArc = surfaceArc;
        platformEffector.rotationalOffset = rotationalOffset;
    }
}
