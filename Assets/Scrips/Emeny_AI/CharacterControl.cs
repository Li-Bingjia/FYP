using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControl : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private Slider staminaBar;
    new Rigidbody rigidbody;
    bool isQuiet = false;
    [SerializeField] Transform quietCubeTransform;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float sprintDuration = 5f;
    [SerializeField] float staminaRecoveryRate = 1f;
    [SerializeField] float quietSpeed = 1.5f;
    [SerializeField] QuietCube quietCube;
    [SerializeField] GameObject speedBuffEffectPrefab;
    private GameObject speedBuffEffectInstance;
    float sprintTimer = 0f;
    bool isSprinting = false;
    [SerializeField] float rotationSpeed = 10f;
    Transform characterTransform;
    [SerializeField] LayerMask groundMask;
    private IInteractable currentInteractable;

    // Buff & 侦测相关
    private float undetectedTimer = 0f;
    public float undetectedThreshold = 5f; // 5秒未被发现
    public bool stealthBuffQuestActive = false;
    private bool hasBuff = false;
    [HideInInspector]
    public bool isDetected = false;
    private float speedBuffMultiplier = 1f;

    Vector3 motion;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        characterTransform = transform;
        if (quietCube != null)
        {
            quietCube.SetActiveByQuiet(isQuiet);
        }
        if (staminaBar != null)
        {
            staminaBar.maxValue = sprintDuration;
            staminaBar.value = sprintDuration - sprintTimer;
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        // 获取摄像机的前和右方向（只取水平分量）
        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = camTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // 第一人称时，角色不旋转，移动方向始终和摄像机一致
        if (ScreenshotCameraController.IsFirstPersonView)
        {
            motion = camForward * input.z + camRight * input.x;
        }
        else
        {
            motion = camForward * input.z + camRight * input.x;

            // 只在第三人称时让角色朝向鼠标
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, groundMask))
            {
                Vector3 lookPoint = hit.point;
                Vector3 lookDir = lookPoint - characterTransform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    characterTransform.rotation = Quaternion.Slerp(characterTransform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }
        }

        animator.SetFloat("MovingSpeed", motion.magnitude);

        // Sprint逻辑
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        if (shiftPressed && motion.magnitude > 0.01f && sprintTimer < sprintDuration)
        {
            isSprinting = true;
            sprintTimer += Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            if (!shiftPressed && sprintTimer > 0f)
            {
                sprintTimer -= Time.deltaTime * staminaRecoveryRate;
                if (sprintTimer < 0f) sprintTimer = 0f;
            }
        }

        animator.SetBool("Sprint", isSprinting);

        if (Input.GetKeyDown(KeyCode.C))
        {
            isQuiet = !isQuiet;
            animator.SetBool("Quiet", isQuiet);

            if (quietCube != null)
            {
                quietCube.SetActiveByQuiet(isQuiet);
            }
        }

        if (quietCubeTransform != null)
        {
            Vector3 playerPos = transform.position;
            playerPos.y += 1f;
            quietCubeTransform.position = playerPos;
            quietCubeTransform.rotation = transform.rotation;
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && currentInteractable.CanInteract())
        {
            currentInteractable.Interact();
        }
        if (staminaBar != null)
        {
            staminaBar.value = sprintDuration - sprintTimer;
        }
        // 第一人称下，角色 transform 始终与摄像机（head）y轴方向一致
        if (ScreenshotCameraController.IsFirstPersonView)
        {
            Vector3 camEuler = camTransform.eulerAngles;
            characterTransform.rotation = Quaternion.Euler(0, camEuler.y, 0);
        }

        // 只在任务激活时才判定buff和计时
        if (stealthBuffQuestActive)
        {
            if (!isDetected)
            {
                undetectedTimer += Time.deltaTime;
                if (!hasBuff && undetectedTimer >= undetectedThreshold)
                {
                    hasBuff = true;
                    BuffManager.Instance.GiveSpeedBuff(this, 2f);
                }
            }
            else
            {
                undetectedTimer = 0f;
                hasBuff = false;
            }

            // 任务目标进度更新
            var quest = QuestController.Instance.activeQuests
                .Find(q => q.quest.questID.StartsWith("stealth_buff_quest"));
            if (quest != null)
            {
                var timeObj = quest.objectives.Find(o => o.type == Quest.ObjectiveType.Custom);
                if (timeObj != null)
                {
                    if (!isDetected)
                    {
                        timeObj.currentAmount = Mathf.Min(timeObj.currentAmount + Time.deltaTime, timeObj.requiredAmount);
                    }
                    else
                    {
                        timeObj.currentAmount = 0;
                    }
                }
            }
        }
    }
    public void SetSpeedBuff(float multiplier)
    {
        speedBuffMultiplier = multiplier;
        if (multiplier > 1f)
        {
            if (speedBuffEffectInstance == null && speedBuffEffectPrefab != null)
            {
                speedBuffEffectInstance = Instantiate(speedBuffEffectPrefab, transform);
                speedBuffEffectInstance.transform.localPosition = new Vector3(0, 3, 0); // 让特效在角色头顶或身体上方
            }
        }
        else
        {
            if (speedBuffEffectInstance != null)
            {
                Destroy(speedBuffEffectInstance);
                speedBuffEffectInstance = null;
            }
        }
    }
    public bool IsQuiet()
    {
        return isQuiet;
    }

    private void FixedUpdate()
    {
        float currentSpeed;
        if (isQuiet)
            currentSpeed = quietSpeed * speedBuffMultiplier;
        else if (isSprinting)
            currentSpeed = sprintSpeed * speedBuffMultiplier;
        else
            currentSpeed = moveSpeed * speedBuffMultiplier;

        rigidbody.velocity = new Vector3(motion.x * currentSpeed, rigidbody.velocity.y, motion.z * currentSpeed);
    }



    // 敌人发现/丢失时调用
    public void OnDetectedByEnemy()
    {
        isDetected = true;
        undetectedTimer = 0f;
        hasBuff = false;
    }

    public void OnLostByEnemy()
    {
        isDetected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }
}