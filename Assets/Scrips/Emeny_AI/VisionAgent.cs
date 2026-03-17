using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisionAgent : MonoBehaviour
{
    [SerializeField] float visionRange = 1f;
    [SerializeField] float visionAngle = 45f;
    public Transform target;
    [SerializeField] Transform lineOfSightPivot;

    bool detected;
    public Action onDetected;
    public Action onLoseDetection;
    float quietDetectTimer = 0f;
    [SerializeField] float quietDetectDelay = 3f; // Quiet状态下延迟时间（秒）

    private void Update()
    {
        if (target == null) { Debug.LogWarning("Target not assigned"); return; }

        CharacterControl playerControl = target.GetComponent<CharacterControl>();
        bool isQuiet = playerControl != null && playerControl.IsQuiet();

        if (isQuiet)
        {
            // Quiet状态下计时
            quietDetectTimer += Time.deltaTime;
            if (quietDetectTimer < quietDetectDelay)
            {
                UnDetected();
                return;
            }
        }
        else
        {
            quietDetectTimer = 0f; // 非Quiet状态重置计时
        }

        bool b = CheckRange(visionRange);
        if (b == false) { UnDetected(); return; }
        b = CheckAngle();
        if (b == false) { UnDetected(); return; }
        b = LineOfSightCheck();
        if (b == false) { UnDetected(); return; }
        Detected();
    }

    private bool LineOfSightCheck()
    {
        VisionPoints visionPoints = target.GetComponent<VisionPoints>();
        foreach(Transform t in visionPoints.visionPoints)
        {
            Vector3 direction = (t.position - lineOfSightPivot.position).normalized;
            Ray ray = new Ray(lineOfSightPivot.position, direction);

            float distanceTargetToAgent = Vector3.Distance(lineOfSightPivot.position, t.position);
            RaycastHit[] hits = Physics.RaycastAll(ray, distanceTargetToAgent);
            List<VisionObstacle> visionObstacles = new List<VisionObstacle>();      
            foreach(RaycastHit rh in hits)
            {
                Debug.Log($"Raycast hit: {rh.transform.name}");
                VisionObstacle vo = rh.transform.GetComponent<VisionObstacle>();
                if(vo != null)
                {
                    Debug.Log($"Hit VisionObstacle: {rh.transform.name}, solid={vo.solid}, ObstaclePower={vo.ObstaclePower}");
                    visionObstacles.Add(vo);
                }
            } 
            float vision = visionRange;
            foreach(VisionObstacle vo in visionObstacles)
            {
                if(vo.solid == true)
                {
                    return false;
                }
                vision *= vo.ObstaclePower;
                if(vision < vo.cutOffPoint){return false;}
            }
            if(CheckRange(vision) == false){return false;}
            
            return true;     
        }
        return false;

    }
    private void Detected()
    {
        if(detected == false)
        {
            detected = true;
            onDetected?.Invoke();
            Debug.Log("Target Detected");
        }
    }
    private void UnDetected()
    {
        if(detected == true)
        {
            detected = false;
            onLoseDetection?.Invoke();
            Debug.Log("Target Lost");
        }
    }
    bool CheckRange(float maxrange)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < maxrange;
    }
    bool CheckAngle()
    {
        Vector3 targetDirection = target.position - transform.position;
        float angle = Vector3.Angle(transform.forward, targetDirection);
        return angle <= visionAngle;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Quaternion rotation = Quaternion.AngleAxis(visionAngle, Vector3.up);
        Vector3 endPoint = transform.position + (rotation * transform.forward * visionRange);
        Gizmos.DrawLine(transform.position, endPoint);

        rotation = Quaternion.AngleAxis(-visionAngle, Vector3.up);
        endPoint = transform.position + (rotation * transform.forward * visionRange);
        Gizmos.DrawLine(transform.position, endPoint);
    }
}
