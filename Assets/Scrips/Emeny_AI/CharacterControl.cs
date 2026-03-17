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
    float sprintTimer = 0f;
    bool isSprinting = false;
    [SerializeField] float rotationSpeed = 10f;
    Transform characterTransform;
    [SerializeField] LayerMask groundMask;
    private IInteractable currentInteractable;

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

        // 让角色移动方向跟随摄像机
        motion = camForward * input.z + camRight * input.x;

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

        // 始终朝向鼠标
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
    }

    public bool IsQuiet()
    {
        return isQuiet;
    }

    private void FixedUpdate()
    {
        float currentSpeed;
        if (isQuiet)
            currentSpeed = quietSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = moveSpeed;

        rigidbody.velocity = new Vector3(motion.x * currentSpeed, rigidbody.velocity.y, motion.z * currentSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 可选：如果你还需要触发器方式，也可以保留
    }

    private void OnTriggerExit(Collider other)
    {
        // 可选
    }
}