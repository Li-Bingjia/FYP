using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    [SerializeField] float waitTimeOnWayPoint = 1f;
    [SerializeField] Path path;
    [SerializeField] float normalSpeed = 3f;
    [SerializeField] float quietSpeed = 1.5f;
    NavMeshAgent agent;
    Animator animator;
    VisionAgent visionAgent;

    float time = 0f;
    [SerializeField] float findDistance = 1.5f; // 触发find动画的距离
    [SerializeField] Transform playerTarget;    // 玩家Transform

    bool isChasing = false; // 是否正在追逐玩家
    bool lostPlayer = false; // 是否刚刚丢失玩家
    Vector3 lastSeenPosition; // 玩家最后被看到的位置
    float lostWaitTime = 2f; // 丢失后等待时间
    float lostTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        visionAgent = GetComponent<VisionAgent>();
    }
    private void Start()
    {
        agent.destination = path.GetNextWayPoints();

        if (visionAgent != null)
        {
            visionAgent.onDetected += OnPlayerDetected;
            visionAgent.onLoseDetection += OnPlayerLost;

            // 箭头管理事件注册
            visionAgent.onDetected += AddIndicatorToPlayer;
            visionAgent.onLoseDetection += RemoveIndicatorFromPlayer;
        }
    }

    private void Update()
    {
        if (PauseController.IsGamePaused)
             return;
        // 计算归一化速度
        float normalizedSpeed = Mathf.InverseLerp(0f, agent.speed, agent.velocity.magnitude);
        animator.SetFloat("MovingSpeed", normalizedSpeed);

        // 检查玩家是否处于Quiet状态
        bool playerIsQuiet = false;
        if (playerTarget != null)
        {
            CharacterControl playerControl = playerTarget.GetComponent<CharacterControl>();
            if (playerControl != null)
                playerIsQuiet = playerControl.IsQuiet();
        }

        // 追逐时速度调整
        if (isChasing && playerTarget != null)
        {
            agent.speed = playerIsQuiet ? quietSpeed : normalSpeed;

            Vector3 toPlayer = playerTarget.position - transform.position;
            float dist = toPlayer.magnitude;
            float stopDistance = 1.0f;

            if (dist > stopDistance)
            {
                Vector3 targetPos = playerTarget.position - toPlayer.normalized * stopDistance;
                agent.destination = targetPos;
            }
            else
            {
                agent.destination = transform.position;
            }

            animator.SetBool("find", dist < findDistance);
        }
        else if (lostPlayer)
        {
            agent.speed = normalSpeed;
            agent.destination = lastSeenPosition;

            float distToLast = Vector3.Distance(transform.position, lastSeenPosition);
            lostTimer += Time.deltaTime;

            float reachThreshold = 0.2f;

            if (distToLast <= reachThreshold || lostTimer >= lostWaitTime)
            {
                lostPlayer = false;
                agent.destination = path.GetNextWayPoints();
                lostTimer = 0f;
            }

            animator.SetBool("find", false);
        }
        else
        {
            agent.speed = normalSpeed;
            if (agent.remainingDistance <= 0.1f)
            {
                time += Time.deltaTime;
                if (time >= waitTimeOnWayPoint)
                {
                    time = 0f;
                    agent.destination = path.GetNextWayPoints();
                }
            }
            animator.SetBool("find", false);
        }
    }

    void OnPlayerDetected()
    {
        if (visionAgent.target != null)
        {
            playerTarget = visionAgent.target;
            isChasing = true;
            lostPlayer = false;
        }
        // OnPlayerDetected
        if (playerTarget != null)
        {
            var cc = playerTarget.GetComponent<CharacterControl>();
            if (cc != null) cc.OnDetectedByEnemy();
        }

        // OnPlayerLost
        if (playerTarget != null)
        {
            var cc = playerTarget.GetComponent<CharacterControl>();
            if (cc != null) cc.OnLostByEnemy();
        }
    }

    void OnPlayerLost()
    {
        if (playerTarget != null)
        {
            lastSeenPosition = playerTarget.position;
        }
        isChasing = false;
        lostPlayer = true;
        lostTimer = 0f;
        animator.SetBool("find", false);
    }

    // 箭头管理
    void AddIndicatorToPlayer()
    {
        EnemyDirectionRing ring = FindFirstObjectByType<EnemyDirectionRing>();
        if (ring == null)
        {
            Debug.LogError("EnemyDirectionRing not found!");
            return;
        }
        if (!ring.enemies.Contains(this.transform))
        {
            ring.enemies.Add(this.transform);
            Debug.Log("Added " + gameObject.name + " to ring.enemies, now count: " + ring.enemies.Count);
        }
    }

    void RemoveIndicatorFromPlayer()
    {
        EnemyDirectionRing ring = FindFirstObjectByType<EnemyDirectionRing>();
        if (ring != null && ring.enemies.Contains(this.transform))
        {
            ring.enemies.Remove(this.transform);
            Debug.Log("Removed " + gameObject.name + " from ring.enemies");
        }
    }
}