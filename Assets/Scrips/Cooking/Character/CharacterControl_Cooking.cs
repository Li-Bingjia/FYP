using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControl_Cooking : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private Slider staminaBar;
    new Rigidbody rigidbody;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float sprintDuration = 5f;
    [SerializeField] float staminaRecoveryRate = 1f;
    [SerializeField] float quietSpeed = 1.5f;
    [SerializeField] GameObject speedBuffEffectPrefab;
    private GameObject speedBuffEffectInstance;
    float sprintTimer = 0f;
    bool isSprinting = false;
    [SerializeField] float rotationSpeed = 10f;
    Transform characterTransform;
    [SerializeField] LayerMask groundMask;
    private float speedBuffMultiplier = 1f;
    Vector3 motion;

    // 已移除 quietCubeTransform 和 quietCube
    bool isQuiet = false;

    // 物品拾取、投掷、交互、音效、UI
    [Header("Interaction")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickableLayer;
    [SerializeField] private Vector2 throwForce = new Vector2(5f, 3f);
    private GameObject heldObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private AudioClip throwSound;

    [Header("UI Reference")]
    [SerializeField] private InGameUIManager uiManager;

    private Transform mainCameraTransform;

    // 通用交互
    private IInteractable currentInteractable;
    private Building buildingSystem;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        characterTransform = transform;
        if (staminaBar != null)
        {
            staminaBar.maxValue = sprintDuration;
            staminaBar.value = sprintDuration - sprintTimer;
        }
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        buildingSystem = Object.FindFirstObjectByType<Building>();
    }

    void Update()
    {
        // 建筑模式下禁止操作
        if (buildingSystem != null && buildingSystem.isBuildMode)
            return;

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

        // 物品拾取与投掷
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                ThrowHeldObject();
            }
        }

        // 通用交互
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
            }
            else
            {
                TryGeneralInteraction();
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
                speedBuffEffectInstance.transform.localPosition = new Vector3(0, 3, 0);
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

    // 物品拾取
    private void TryPickUp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward, pickupRange, pickableLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<Rigidbody>() != null)
            {
                heldObject = hitCollider.gameObject;
                Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
                objRb.isKinematic = true;
                objRb.useGravity = false;
                Collider objCollider = heldObject.GetComponent<Collider>();
                if (objCollider != null)
                {
                    objCollider.enabled = false;
                }
                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
                PlaySound(pickUpSound);
                break;
            }
        }
    }

    // 物品投掷
    private void ThrowHeldObject()
    {
        if (heldObject == null) return;
        Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
        heldObject.transform.SetParent(null);
        objRb.isKinematic = false;
        objRb.useGravity = true;
        Collider objCollider = heldObject.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = true;
        }
        Vector3 force = transform.forward * throwForce.x + Vector3.up * throwForce.y;
        objRb.AddForce(force, ForceMode.Impulse);
        PlaySound(throwSound);
        heldObject = null;
    }

    // 播放音效
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 通用交互
    private bool TryGeneralInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hit in hits)
        {
            Coin coin = hit.GetComponent<Coin>();
            if (coin != null)
            {
                coin.Collect();
                return true;
            }
        }
        return false;
    }

    // 交互检测
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