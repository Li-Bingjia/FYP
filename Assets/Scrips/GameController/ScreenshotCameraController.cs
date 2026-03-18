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
    [SerializeField] private Transform head;
    [SerializeField] private GameObject bodyModel; // 角色身体模型    

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float firstPersonPitchSmoothTime = 0.15f; // 第一人称pitch平滑时间

    [Header("Fixed Y Offset")]
    [SerializeField] private float fixedYOffset = 3f; // 固定Y轴偏移

    private CinemachineTransposer transposer;
    private float targetDistance = 10f;
    private float currentDistance = 10f;

    private float yaw, pitch;
    private float targetYaw, targetPitch;
    private float yawVelocity, pitchVelocity;
    private float thirdPersonDistance;
    private float thirdPersonYaw, thirdPersonPitch, thirdPersonTargetYaw, thirdPersonTargetPitch;

    public static bool IsFirstPersonView { get; private set; }
    private bool isFirstPerson = false;

    // 过渡相关
    private bool isTransitioning = false;
    private float transitionDuration = 0.3f; // 过渡时间
    private float transitionTimer = 0f;
    private Vector3 transitionStartOffset;
    private Vector3 transitionTargetOffset;
    private Quaternion transitionStartRot;
    private Quaternion transitionTargetRot;
    private bool transitionToFirstPerson = false;

    // 第一人称pitch平滑
    private float smoothPitch;
    private float smoothPitchVelocity;

    void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            currentDistance = targetDistance = -transposer.m_FollowOffset.z;
            yaw = targetYaw = target.eulerAngles.y;
            pitch = targetPitch = 20f;
            smoothPitch = pitch;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && !isTransitioning)
        {
            isTransitioning = true;
            transitionTimer = 0f;
            transitionToFirstPerson = !isFirstPerson;

            // 记录起点
            transitionStartOffset = transposer.m_FollowOffset;
            transitionStartRot = Quaternion.Euler(pitch, yaw, 0);

            // 计算终点
            if (transitionToFirstPerson)
            {
                // 保存第三人称参数
                thirdPersonDistance = targetDistance;
                thirdPersonYaw = yaw;
                thirdPersonPitch = pitch;
                thirdPersonTargetYaw = targetYaw;
                thirdPersonTargetPitch = targetPitch;

                transitionTargetOffset = new Vector3(0, fixedYOffset, -0.1f);
                transitionTargetRot = Quaternion.Euler(pitch, yaw, 0);
            }
            else
            {
                transitionTargetOffset = new Vector3(0, fixedYOffset, -thirdPersonDistance);
                transitionTargetRot = Quaternion.Euler(thirdPersonPitch, thirdPersonYaw, 0);
            }
        }
    }

    void LateUpdate()
    {
        if (transposer == null || (isFirstPerson ? head == null : target == null)) return;

        // 视角过渡
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);

            // 插值 offset 和 rotation
            transposer.m_FollowOffset = Vector3.Lerp(transitionStartOffset, transitionTargetOffset, t);
            Quaternion rot = Quaternion.Slerp(transitionStartRot, transitionTargetRot, t);

            // 插值 pitch/yaw
            if (transitionToFirstPerson)
            {
                pitch = Mathf.LerpAngle(thirdPersonPitch, pitch, t);
                yaw = Mathf.LerpAngle(thirdPersonYaw, yaw, t);
                smoothPitch = pitch;
            }
            else
            {
                pitch = Mathf.LerpAngle(pitch, thirdPersonPitch, t);
                yaw = Mathf.LerpAngle(yaw, thirdPersonYaw, t);
                smoothPitch = pitch;
            }

            if (t >= 1f)
            {
                isTransitioning = false;
                isFirstPerson = transitionToFirstPerson;
                IsFirstPersonView = isFirstPerson;
                if (isFirstPerson)
                {
                    vcam.Follow = head;
                    vcam.LookAt = head;
                    targetDistance = 0.1f;
                    if (bodyModel != null) bodyModel.SetActive(false);
                }
                else
                {
                    vcam.Follow = target;
                    vcam.LookAt = target;
                    targetDistance = thirdPersonDistance;
                    currentDistance = thirdPersonDistance;
                    yaw = thirdPersonYaw;
                    pitch = thirdPersonPitch;
                    targetYaw = thirdPersonTargetYaw;
                    targetPitch = thirdPersonTargetPitch;
                    if (bodyModel != null) bodyModel.SetActive(true);
                }
            }
            return;
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (!isFirstPerson && Mathf.Abs(scroll) > 0.001f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmooth);

        // Rotation input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        targetYaw += mouseX * mouseSensitivity * Time.deltaTime;
        targetPitch -= mouseY * mouseSensitivity * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Smooth rotation
        yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);

        if (isFirstPerson)
        {
            // 第一人称下，pitch平滑延迟
            smoothPitch = Mathf.SmoothDamp(smoothPitch, targetPitch, ref smoothPitchVelocity, firstPersonPitchSmoothTime);
        }
        else
        {
            // 第三人称直接跟随
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);
            smoothPitch = pitch;
        }

        // Camera position
        float yOffset = isFirstPerson ? 0f : fixedYOffset;
        Vector3 offset = new Vector3(0, yOffset, -currentDistance);
        Quaternion rotation = Quaternion.Euler(smoothPitch, yaw, 0);
        transposer.m_FollowOffset = rotation * offset;
    }
    public static ScreenshotCameraController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // 强制摄像机朝向目标
    public void ForceLookAt(Transform enemy)
    {
        if (enemy == null) return;

        Vector3 camPos = (IsFirstPersonView && head != null) ? head.position : target.position;
        Vector3 dir = enemy.position - camPos;

        // 计算目标yaw和pitch
        float targetYawAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float targetPitchAngle = -Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;

        targetYaw = targetYawAngle;
        targetPitch = Mathf.Clamp(targetPitchAngle, minPitch, maxPitch);

        // 只变视角，不动位置
        yaw = targetYaw;
        pitch = targetPitch;
        smoothPitch = pitch;
    }
}