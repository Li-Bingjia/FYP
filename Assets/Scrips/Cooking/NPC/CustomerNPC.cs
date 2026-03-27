using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Video;

public class CustomerNPC : MonoBehaviour
{
    public enum State { MovingToSeat, Sitting, WaitingForFood, Eating, Leaving }
    public State currentState;

    [Header("Settings")]
    public string wantedDishName = "Steak";
    public int dishPrice = 20;
    public GameObject coinPrefab;
    [Tooltip("NPC Movement Speed")]
    public float moveSpeed = 2.0f;

    [Header("UI")]
    public GameObject orderBubble;
    public Image dishIcon;

    [Header("Reaction Feedback")]
    [Tooltip("The root object for the reaction bubble")]
    public GameObject reactionBubbleRoot;
    [Tooltip("Visual for normal food reaction (Recommendation: Use RawImage with VideoPlayer)")]
    public GameObject normalReactionVisual;
    [Tooltip("Visual for special food reaction (Recommendation: Use RawImage with VideoPlayer)")]
    public GameObject specialReactionVisual;

    private NavMeshAgent agent;
    private DiningChair targetChair;
    private Vector3 exitPosition;

    private GameObject currentFood;

    private Building buildingSystem;
    private bool wasInBuildMode = false;
    private Vector3 initialTargetChairPos;

    private Camera mainCamera;

    // 新增：Animator引用
    private Animator animator;

    public void Initialize(DiningChair chair, Vector3 exitPos, string dish, int price, Sprite icon)
    {
        targetChair = chair;
        exitPosition = exitPos;
        wantedDishName = dish;
        dishPrice = price;

        targetChair.isOccupied = true;

        initialTargetChairPos = targetChair.transform.position;

        if (dishIcon != null) dishIcon.sprite = icon;
        if (orderBubble != null) orderBubble.SetActive(false);
        if (reactionBubbleRoot != null) reactionBubbleRoot.SetActive(false);
    }

    void Start()
    {
        mainCamera = Camera.main;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        buildingSystem = FindObjectOfType<Building>();

        currentState = State.MovingToSeat;

        Vector3 seatPos = targetChair.transform.position;
        agent.SetDestination(seatPos);

        // 新增：获取Animator
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        if (orderBubble != null && orderBubble.activeInHierarchy)
        {
            orderBubble.transform.rotation = mainCamera.transform.rotation;
        }

        if (reactionBubbleRoot != null && reactionBubbleRoot.activeInHierarchy)
        {
            reactionBubbleRoot.transform.rotation = mainCamera.transform.rotation;
        }
    }

    void Update()
    {
        // 动画：走路速度
        if (animator != null && agent != null)
        {
            animator.SetFloat("MovingSpeed", agent.velocity.magnitude);
        }

        if (buildingSystem != null && buildingSystem.isBuildMode)
        {
            if (!wasInBuildMode)
            {
                wasInBuildMode = true;
                if (agent.enabled) agent.isStopped = true;
            }
            return;
        }
        else if (wasInBuildMode)
        {
            wasInBuildMode = false;
            if (agent.enabled) agent.isStopped = false;

            if (currentState == State.MovingToSeat)
            {
                if (Vector3.Distance(targetChair.transform.position, initialTargetChairPos) > 0.1f)
                {
                    FindNearestNewSeat();
                }
                else
                {
                    agent.SetDestination(targetChair.transform.position);
                }
            }
        }

        if (currentState == State.MovingToSeat)
        {
            float dist = Vector3.Distance(transform.position, targetChair.transform.position);

            if (!agent.pathPending && dist < 1f && (agent.remainingDistance < 0.2f || agent.velocity.sqrMagnitude < 0.01f))
            {
                OnSitDown();
            }
        }
        else if (currentState == State.Leaving)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FindNearestNewSeat()
    {
        targetChair.isOccupied = false;

        float closestDist = float.MaxValue;
        DiningChair bestChair = null;
        DiningChair[] allChairs = FindObjectsOfType<DiningChair>();

        foreach (var chair in allChairs)
        {
            if (!chair.isOccupied && chair.HasTableNearby())
            {
                float d = Vector3.Distance(transform.position, chair.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    bestChair = chair;
                }
            }
        }

        if (bestChair != null)
        {
            targetChair = bestChair;
            targetChair.isOccupied = true;
            initialTargetChairPos = targetChair.transform.position;
            agent.SetDestination(targetChair.transform.position);
        }
        else
        {
            LeaveRestaurant();
        }
    }

    private void OnSitDown()
    {
        currentState = State.Sitting;

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 动画：坐下
        if (animator != null)
            animator.SetBool("Sit", true);

        StartCoroutine(SmoothSitDownRoutine());
    }

    IEnumerator SmoothSitDownRoutine()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 targetPos = targetChair.transform.position + Vector3.up * 0.15f;
        Quaternion targetRot = targetChair.transform.rotation;

        if (targetChair.linkedTable != null)
        {
            Vector3 lookDir = targetChair.linkedTable.transform.position - targetPos;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                targetRot = Quaternion.LookRotation(lookDir);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (targetChair.linkedTable != null)
            targetChair.linkedTable.AddCustomer(this);

        StartCoroutine(WaitAndOrderRoutine());
    }

    IEnumerator WaitAndOrderRoutine()
    {
        yield return new WaitForSeconds(2f);
        if (currentState == State.Sitting)
        {
            currentState = State.WaitingForFood;
            if (orderBubble != null) orderBubble.SetActive(true);
        }
    }

    public void ReceiveFood(GameObject food)
    {
        if (currentState != State.WaitingForFood && currentState != State.Sitting) return;

        if (orderBubble != null) orderBubble.SetActive(false);

        currentFood = food;

        if (currentFood.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        if (currentFood.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        StartCoroutine(EatRoutine());
    }

    IEnumerator EatRoutine()
    {
        currentState = State.Eating;

        // 动画：吃饭
        if (animator != null)
            animator.SetTrigger("Eat");

        yield return new WaitForSeconds(1.0f);
        ShowFoodReaction();

        yield return new WaitForSeconds(9.0f);

        if (reactionBubbleRoot != null) reactionBubbleRoot.SetActive(false);

        LeaveMoney();
        LeaveRestaurant();
    }

    private void ShowFoodReaction()
    {
        if (reactionBubbleRoot == null) return;

        bool isSpecial = false;
        if (currentFood != null && currentFood.GetComponentInChildren<Light>() != null)
        {
            isSpecial = true;
        }

        reactionBubbleRoot.SetActive(true);

        GameObject activeVisual = null;

        if (normalReactionVisual != null)
        {
            bool isActive = !isSpecial;
            normalReactionVisual.SetActive(isActive);
            if (isActive) activeVisual = normalReactionVisual;
        }

        if (specialReactionVisual != null)
        {
            bool isActive = isSpecial;
            specialReactionVisual.SetActive(isActive);
            if (isActive) activeVisual = specialReactionVisual;
        }

        if (activeVisual != null)
        {
            VideoPlayer vp = activeVisual.GetComponent<VideoPlayer>();
            RawImage rawImage = activeVisual.GetComponent<RawImage>();

            if (vp != null)
            {
                vp.Play();

                if (rawImage != null && vp.renderMode == VideoRenderMode.APIOnly)
                {
                    StartCoroutine(BindVideoTexture(vp, rawImage));
                }
            }
        }
    }

    private IEnumerator BindVideoTexture(VideoPlayer vp, RawImage image)
    {
        while (!vp.isPrepared)
        {
            yield return null;
        }
        image.texture = vp.texture;
    }

    private void LeaveMoney()
    {
        if (coinPrefab != null && targetChair.linkedTable != null)
        {
            float spawnY = targetChair.linkedTable.transform.position.y + 0.8f;
            Collider tableCol = targetChair.linkedTable.GetComponentInChildren<Collider>();
            if (tableCol != null) spawnY = tableCol.bounds.max.y + 0.02f;

            Vector3 spawnPos = targetChair.linkedTable.transform.position;
            spawnPos.y = spawnY;

            Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
        }
    }

    private void LeaveRestaurant()
    {
        if (currentFood != null)
        {
            Destroy(currentFood);
        }

        targetChair.isOccupied = false;

        if (targetChair.linkedTable != null)
            targetChair.linkedTable.RemoveCustomer(this);

        // 动画：离开时取消坐下
        if (animator != null)
            animator.SetBool("Sit", false);

        StartCoroutine(SmoothLeaveRoutine());
    }

    IEnumerator SmoothLeaveRoutine()
    {
        Vector3 targetNavPos = transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(targetChair.transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            targetNavPos = hit.position;
        }

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 dirToExit = (exitPosition - transform.position).normalized;
        dirToExit.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(dirToExit);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, targetNavPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetNavPos;

        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(exitPosition);

        currentState = State.Leaving;
    }
}