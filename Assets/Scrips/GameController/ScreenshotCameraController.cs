using UnityEngine;
using Cinemachine;

public class ScreenshotCameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private Transform target;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float zoomSmooth = 8f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 80f;

    private CinemachineTransposer transposer;
    private float targetDistance = 10f;
    private float currentDistance = 10f;

    private float yaw, pitch;
    private float targetYaw, targetPitch;
    private float yawVelocity, pitchVelocity;

    void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            currentDistance = targetDistance = -transposer.m_FollowOffset.z;
            yaw = targetYaw = target.eulerAngles.y;
            pitch = targetPitch = 20f;
        }
    }

    void LateUpdate()
    {
        if (transposer == null || target == null) return;

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmooth);

        // Rotation input
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            targetYaw += mouseX * mouseSensitivity * Time.deltaTime;
            targetPitch -= mouseY * mouseSensitivity * Time.deltaTime;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        // Smooth rotation
        yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        // Camera position
        Vector3 offset = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transposer.m_FollowOffset = rotation * offset;
    }

}