using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("偏移量")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("跟随速度")]
    [SerializeField] private float followSpeed = 5f;

    [Header("摄像头抖动")]
    [SerializeField] private float shakeAmount = 0.2f;
    [SerializeField] private float shakeDuration = 0.5f;
    private float shakeTimeRemaining = 0f;

    [Header("摄像头上下边界")]
    [SerializeField] private float moveSpeed = 2f;  // 摄像头上下移动的速度
    [SerializeField] private float yOffsetMin = -5f;  // 摄像头Y轴最小偏移量（相对当前偏移量）
    [SerializeField] private float yOffsetMax = 5f;   // 摄像头Y轴最大偏移量（相对当前偏移量）

    private float originalYOffset;  // 保存摄像头初始的Y偏移量

    private void Start()
    {
        // 记录初始偏移量
        originalYOffset = offset.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // 计算目标位置（基于偏移量）
        Vector3 targetPosition = target.position + offset;

        // 处理摄像头抖动效果
        if (shakeTimeRemaining > 0f)
        {
            targetPosition += new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);
            shakeTimeRemaining -= Time.deltaTime;
        }

        // 仅当玩家静止时，允许通过按键上下移动摄像头
        if (IsPlayerStationary())
        {
            // 按下 W 键，摄像头向上移动
            if (Input.GetKey(KeyCode.W))
            {
                offset.y = Mathf.Clamp(offset.y + moveSpeed * Time.deltaTime, yOffsetMin, yOffsetMax);
            }
            // 按下 S 键，摄像头向下移动
            else if (Input.GetKey(KeyCode.S))
            {
                offset.y = Mathf.Clamp(offset.y - moveSpeed * Time.deltaTime, yOffsetMin, yOffsetMax);
            }
            // 松开 W 或 S 键后，恢复摄像头的原始 Y 偏移量
            else
            {
                offset.y = Mathf.Lerp(offset.y, originalYOffset, Time.deltaTime * 5f); // 缓慢恢复
            }
        }

        // 平滑移动摄像头位置
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    // 检查玩家是否静止（即速度为零）
    private bool IsPlayerStationary()
    {
        if (target.GetComponent<Rigidbody2D>() != null)
        {
            return target.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f;
        }
        return true;  // 如果没有 Rigidbody2D，默认认为玩家静止
    }

    // 触发摄像头抖动效果
    public void TriggerShake(float duration)
    {
        shakeTimeRemaining = duration;
    }
}