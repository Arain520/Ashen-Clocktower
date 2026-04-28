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

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;
        if(shakeTimeRemaining > 0f)
        {
            targetPosition += new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);
            shakeTimeRemaining -= Time.deltaTime;
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
    public void TriggerShake(float duration)
    {
        shakeTimeRemaining = duration;
    }
}