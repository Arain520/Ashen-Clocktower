using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("偏移量")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("跟随速度")]
    [SerializeField] private float followSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}