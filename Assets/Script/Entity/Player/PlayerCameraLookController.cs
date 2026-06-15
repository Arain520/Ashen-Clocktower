using Cinemachine;
using UnityEngine;

public class PlayerCameraLookController : MonoBehaviour
{
    [SerializeField] private string virtualCameraName = "PlayerCamera";
    [SerializeField] private float lookUpOffset = 4f;
    [SerializeField] private float lookDownOffset = 4f;
    [SerializeField] private float offsetSmoothSpeed = 8f;
    [SerializeField] private float stoppedSpeedThreshold = 0.05f;

    private CinemachineFramingTransposer framingTransposer;
    private Vector3 defaultTrackedObjectOffset;
    private float targetYOffset;

    private void Awake()
    {
        CinemachineVirtualCamera virtualCamera = FindPlayerVirtualCamera();

        if (virtualCamera == null)
            return;

        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (framingTransposer != null)
            defaultTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }

    private void LateUpdate()
    {
        if (framingTransposer == null)
            return;

        Vector3 targetOffset = defaultTrackedObjectOffset + new Vector3(0, targetYOffset, 0);
        framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(
            framingTransposer.m_TrackedObjectOffset,
            targetOffset,
            offsetSmoothSpeed * Time.deltaTime);
    }

    public void SetVerticalLookInput(float yInput)
    {
        if (yInput > 0)
            targetYOffset = lookUpOffset;
        else if (yInput < 0)
            targetYOffset = -lookDownOffset;
        else
            targetYOffset = 0;
    }

    public void UpdateLookByPlayerVelocity(Vector2 playerVelocity, bool isInputBlocked)
    {
        if (isInputBlocked || playerVelocity.sqrMagnitude > stoppedSpeedThreshold * stoppedSpeedThreshold)
        {
            ResetLookOffset();
            return;
        }

        SetVerticalLookInput(Input.GetAxisRaw("Vertical"));
    }

    public void ResetLookOffset()
    {
        targetYOffset = 0;
    }

    private CinemachineVirtualCamera FindPlayerVirtualCamera()
    {
        GameObject cameraObject = GameObject.Find(virtualCameraName);

        if (cameraObject != null && cameraObject.TryGetComponent(out CinemachineVirtualCamera namedCamera))
            return namedCamera;

        return FindObjectOfType<CinemachineVirtualCamera>();
    }
}
