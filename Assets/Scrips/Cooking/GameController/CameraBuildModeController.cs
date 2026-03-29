using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBuildModeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float fixedY = 3f;
    public float buildModeRotation = 80f; // 俯视角
    private bool isBuildMode = false;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    public Cinemachine.CinemachineVirtualCamera vcam;

    // 新增：保存原始Follow/LookAt
    private Transform originalFollow;
    private Transform originalLookAt;

    void Update()
    {
        if (!isBuildMode) return;

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;

        transform.position += move.normalized * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }

    public void EnterBuildMode()
    {
        if (isBuildMode) return;
        isBuildMode = true;

        savedPosition = transform.position;
        savedRotation = transform.rotation;

        if (vcam != null)
        {
            // 保存原始Follow/LookAt
            originalFollow = vcam.Follow;
            originalLookAt = vcam.LookAt;
            vcam.Follow = null;
            vcam.LookAt = null;
        }

        transform.rotation = Quaternion.Euler(buildModeRotation, 0f, 0f);
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }

    public void ExitBuildMode()
    {
        if (!isBuildMode) return;
        isBuildMode = false;

        transform.position = savedPosition;
        transform.rotation = savedRotation;

        // 恢复Follow/LookAt
        if (vcam != null)
        {
            vcam.Follow = originalFollow;
            vcam.LookAt = originalLookAt;
        }
    }
}