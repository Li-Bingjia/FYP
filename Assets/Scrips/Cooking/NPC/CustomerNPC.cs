using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Video;

public class CustomerNPC : MonoBehaviour
{
    public enum State { MovingToSeat, Sitting, WaitingForFood, Eating, Leaving }
    public State currentState;

    [Header("NPC Settings")]
    public string wantedDishName = "Steak";
    public int dishPrice = 20;
    public GameObject coinPrefab;
    public float moveSpeed = 2.0f;

    [Header("Slot Bubble UI")]
    public GameObject slotBubblePrefab; 
    public Sprite wantedDishIcon;
    public GameObject happyVideoObj;   
    public GameObject specialVideoObj;
    private GameObject slotBubbleInstance;
    private Image slotIconImage;
    private Transform bubbleFollowTarget;
    private VideoPlayer happyVideo;
    private VideoPlayer specialVideo;
    private NavMeshAgent agent;
    private DiningChair targetChair;
    private Vector3 exitPosition;
    private GameObject currentFood;
    private Animator animator;
    private Camera mainCamera;

    public void Initialize(DiningChair chair, Vector3 exitPos, string dish, int price, Sprite icon)
    {
        targetChair = chair;
        exitPosition = exitPos;
        wantedDishName = dish;
        dishPrice = price;
        wantedDishIcon = icon;
        targetChair.isOccupied = true;
    }

    void Start()
    {
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = moveSpeed;
        animator = GetComponent<Animator>();
        currentState = State.MovingToSeat;
        agent.SetDestination(targetChair.transform.position);

        // 创建slot气泡
        slotBubbleInstance = Instantiate(slotBubblePrefab, transform);
        slotIconImage = slotBubbleInstance.GetComponentInChildren<Image>();
        slotBubbleInstance.SetActive(false);
        bubbleFollowTarget = transform; // 可自定义为头部骨骼等

        // 初始化视频组件
        if (happyVideoObj != null)
            happyVideo = happyVideoObj.GetComponent<VideoPlayer>();
        if (specialVideoObj != null)
            specialVideo = specialVideoObj.GetComponent<VideoPlayer>();
    }

    void LateUpdate()
    {
        // 气泡UI始终朝向摄像机
        if (slotBubbleInstance != null && mainCamera != null)
        {
            slotBubbleInstance.transform.position = bubbleFollowTarget.position + Vector3.up * 2.0f;
            slotBubbleInstance.transform.rotation = Quaternion.LookRotation(slotBubbleInstance.transform.position - mainCamera.transform.position);
        }
    }

    void Update()
    {
        if (animator != null && agent != null)
            animator.SetFloat("MovingSpeed", agent.velocity.magnitude);

        if (currentState == State.MovingToSeat)
        {
            if (!agent.pathPending && Vector3.Distance(transform.position, targetChair.transform.position) < 1f)
                OnSitDown();
        }
        else if (currentState == State.Leaving)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
                Destroy(gameObject);
        }
    }

    private void OnSitDown()
    {
        currentState = State.Sitting;
        agent.isStopped = true;
        agent.enabled = false;
        if (animator != null) animator.SetBool("Sit", true);
        StartCoroutine(WaitAndOrderRoutine());
    }

    System.Collections.IEnumerator WaitAndOrderRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (currentState == State.Sitting)
        {
            currentState = State.WaitingForFood;
            ShowOrderSlot();
        }
    }

    void ShowOrderSlot()
    {
        if (slotBubbleInstance != null && slotIconImage != null)
        {
            slotBubbleInstance.SetActive(true);
            slotIconImage.sprite = wantedDishIcon;
            slotIconImage.color = Color.white;
        }
    }

    // 玩家给菜
    public void ReceiveFood(GameObject food, Sprite foodIcon, bool isSpecial = false)
    {
        if (currentState != State.WaitingForFood && currentState != State.Sitting) return;
        currentFood = food;
        if (currentFood.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        if (currentFood.TryGetComponent<Collider>(out var col)) col.enabled = false;

        // slot显示收到的菜
        if (slotIconImage != null)
        {
            slotIconImage.sprite = foodIcon;
            slotIconImage.color = Color.white;
        }

        StartCoroutine(EatRoutine(isSpecial));
    }

    System.Collections.IEnumerator EatRoutine(bool isSpecial)
    {
        currentState = State.Eating;
        if (animator != null) animator.SetTrigger("Eat");
        yield return new WaitForSeconds(1.0f);

        if (slotIconImage != null) slotIconImage.enabled = false;

        if (isSpecial && specialVideoObj != null)
        {
            specialVideoObj.SetActive(true);
            specialVideo.Play();
        }
        else if (!isSpecial && happyVideoObj != null)
        {
            happyVideoObj.SetActive(true);
            happyVideo.Play();
        }

        yield return new WaitForSeconds(2.0f);

        if (happyVideoObj != null) happyVideoObj.SetActive(false);
        if (specialVideoObj != null) specialVideoObj.SetActive(false);
        if (slotIconImage != null) slotIconImage.enabled = true;
        slotBubbleInstance.SetActive(false);

        LeaveMoney();
        LeaveRestaurant();
    }
    void LeaveMoney()
    {
        if (coinPrefab != null && targetChair.linkedTable != null)
        {
            Vector3 spawnPos = targetChair.linkedTable.transform.position + Vector3.up * 0.8f;
            Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation);
        }
    }

    void LeaveRestaurant()
    {
        if (currentFood != null) Destroy(currentFood);
        targetChair.isOccupied = false;
        if (animator != null) animator.SetBool("Sit", false);

        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(exitPosition);
        currentState = State.Leaving;
    }
    public void OnPlayerDropFood(Sprite foodIcon, bool isSpecial)
    {
        // 判断是否是NPC想要的菜品
        bool isWanted = (foodIcon == wantedDishIcon);

        // 你可以根据业务逻辑自定义特殊菜品的判定
        if (isWanted && !isSpecial)
        {
            // 正常投喂，happy反应
            ReceiveFood(null, foodIcon, false);
        }
        else if (isSpecial)
        {
            // 特殊投喂，special反应
            ReceiveFood(null, foodIcon, true);
        }
        else
        {
            // 不是想要的菜品且不是特殊菜品，可自定义无反应或不理会
            // 例如弹出提示、无动画等
        }
    }
}