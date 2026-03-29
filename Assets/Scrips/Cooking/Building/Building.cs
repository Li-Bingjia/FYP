using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class Building : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private MonoBehaviour cameraControllerScript;
    [SerializeField] private BuildMenuUI buildMenuUI;
    [SerializeField] private GameObject buildButton;
    [SerializeField] private GameObject exitHintText;
    [SerializeField] private GameObject gridVisualizer;
    [SerializeField] private CinemachineVirtualCamera vcam; 
    [SerializeField] private GameObject playerModel;        
    private CinemachineTransposer transposer;
    private Vector3 savedOffset;
    private float savedYaw, savedPitch;

    [Header("Grid Settings")]
    public float cellSize = 0.5f;     
    public int gridWidth = 10;
    public int gridHeight = 10;
    public Vector3 gridOrigin = new Vector3(-2.5f, 0, -2.5f);
    [SerializeField] private LayerMask floorLayer; 

    [Header("Build Mode State")]
    public bool isBuildMode = false;
    public GameObject player;
    private Vector3 savedCamPosition;
    private Quaternion savedCamRotation;
    public float buildCamMoveSpeed = 5f; 
    public GameObject selectedObject;
    private CharacterControl_Cooking playerMovement;
    private float pivotToBottomOffset; 
    private Vector3 dragOffset; 
    private CameraBuildModeController buildCamController;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip selectObjectSound; 
    [SerializeField] private AudioClip placeObjectSound; 

    void Start()
    {
        if (player != null) playerMovement = player.GetComponent<CharacterControl_Cooking>();
        if (exitHintText != null) exitHintText.SetActive(false);
        if (gridVisualizer != null)
        {
            // 参数同步
            var gv = gridVisualizer.GetComponent<GridVisualizer>();
            if (gv != null)
            {
                gv.gridWidth = gridWidth;
                gv.gridHeight = gridHeight;
                gv.cellSize = cellSize;
                gv.gridOrigin = gridOrigin;
                gv.lineColor = Color.white;
                gv.RedrawGrid();
            }
            gridVisualizer.SetActive(false);
        }
        buildCamController = Camera.main.GetComponent<CameraBuildModeController>();
        if (vcam != null)
            transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
            return;
        }

        if (isBuildMode && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleBuildMode();
            return;
        }

        if (!isBuildMode) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0) && selectedObject == null)
        {
            TrySelectObject();
            if (selectedObject != null) return; 
        }

        if (selectedObject != null)
        {
            HandleObjectTransformation();
        }
    }

    public void ToggleBuildMode()
    {
        isBuildMode = !isBuildMode;
        if (buildButton != null) buildButton.SetActive(!isBuildMode);
        if (exitHintText != null) exitHintText.SetActive(isBuildMode);
        if (gridVisualizer != null) gridVisualizer.SetActive(isBuildMode); 
        if (playerMovement != null) playerMovement.enabled = !isBuildMode;
        if (!isBuildMode && selectedObject != null) ForceDropObject();

        // 摄像机切换
        if (transposer != null)
        {
            if (isBuildMode)
            {
                savedOffset = transposer.m_FollowOffset;
                savedYaw = vcam.transform.eulerAngles.y;
                savedPitch = vcam.transform.eulerAngles.x;
                transposer.m_FollowOffset = new Vector3(0, 15f, 0); 
                vcam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                savedCamPosition = vcam.transform.position;
                savedCamRotation = vcam.transform.rotation;

                Vector3 pos = vcam.transform.position;
                pos.y = 3f;
                vcam.transform.position = pos;
                vcam.transform.rotation = Quaternion.Euler(80f, 0f, 0f);
            }
            else
            {
                transposer.m_FollowOffset = savedOffset;
                vcam.transform.rotation = Quaternion.Euler(savedPitch, savedYaw, 0f);
                vcam.transform.position = savedCamPosition;
                vcam.transform.rotation = savedCamRotation;
            }
        }
        if (buildCamController != null)
        {
            if (isBuildMode)
            {
                buildCamController.EnterBuildMode();
                if (vcam != null) vcam.gameObject.SetActive(false); 
            }
            else
            {
                buildCamController.ExitBuildMode();
                if (vcam != null) vcam.gameObject.SetActive(true); 
            }
        }
        if (cameraControllerScript != null)
            cameraControllerScript.enabled = !isBuildMode;
        if (playerModel != null)
            playerModel.SetActive(!isBuildMode);
        if (buildMenuUI != null)
            buildMenuUI.SetContentGridActive(isBuildMode);

        // 每次进入建造模式都刷新一次网格参数和显示
        if (gridVisualizer != null)
        {
            var gv = gridVisualizer.GetComponent<GridVisualizer>();
            if (gv != null)
            {
                gv.gridWidth = gridWidth;
                gv.gridHeight = gridHeight;
                gv.cellSize = cellSize;
                gv.gridOrigin = gridOrigin;
                gv.lineColor = Color.white;
                gv.RedrawGrid();
            }
        }
    }

    private void TrySelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform target = GetFurnitureParent(hit.collider.transform);
            if (target != null)
            {
                GameObject hitObject = hit.collider.gameObject;

                // --- 检查占用状态 ---
                
                DiningChair chair = target.GetComponent<DiningChair>();
                if (chair == null) chair = hitObject.GetComponentInParent<DiningChair>();
                if (chair != null && !chair.CanBeMoved())
                {
                    Debug.Log("无法移动：椅子上有人！");
                    return;
                }

                DiningTable table = target.GetComponent<DiningTable>();
                if (table == null) table = hitObject.GetComponentInParent<DiningTable>();
                if (table != null && !table.CanBeMoved())
                {
                    Debug.Log("无法移动：桌子正在使用中！");
                    return; 
                }

                // [NEW] 检查容器是否为空
                StorageContainer container = target.GetComponent<StorageContainer>();
                if (container == null) container = hitObject.GetComponentInParent<StorageContainer>();
                if (container != null && !container.IsEmpty())
                {
                    Debug.Log("无法移动：容器内有物品！");
                    return;
                }
                // ---------------------

                selectedObject = target.gameObject;

                CalculatePivotOffset();

                if(Physics.Raycast(ray, out RaycastHit floorHit, 100f, floorLayer))
                {
                    dragOffset = selectedObject.transform.position - floorHit.point;
                    dragOffset.y = 0; 
                }
                else
                {
                    dragOffset = Vector3.zero;
                }

                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                
                // 播放选中音效
                if (audioSource != null && selectObjectSound != null)
                     audioSource.PlayOneShot(selectObjectSound);
            }
        }
    }

    public void SelectFurnitureFromUI(GameObject prefab)
    {
        if (prefab == null) return;
        if (selectedObject != null) ForceDropObject();

        GameObject newObj = Instantiate(prefab);
        selectedObject = newObj;
        
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        CalculatePivotOffset();
        dragOffset = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer))
        {
            Vector3 gridCenter = SnapToGrid(hit.point);
            float hoveringY = 0f + pivotToBottomOffset + 0.2f;
            selectedObject.transform.position = new Vector3(gridCenter.x, hoveringY, gridCenter.z);
        }

        if (audioSource != null && selectObjectSound != null)
            audioSource.PlayOneShot(selectObjectSound);
    }

    private void CalculatePivotOffset()
    {
        var colliders = selectedObject.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
            pivotToBottomOffset = selectedObject.transform.position.y - bounds.min.y;
        }
        else
        {
            pivotToBottomOffset = 0f;
        }
    }

    private Transform GetFurnitureParent(Transform t)
    {
        while (t != null)
        {
            if (t.CompareTag("Furniture")) return t;
            t = t.parent;
        }
        return null;
    }

    private void HandleObjectTransformation()
    {
        if (Input.GetKeyDown(KeyCode.R)) selectedObject.transform.Rotate(Vector3.up, -90f);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer))
        {
            Vector3 rawPosition = hit.point + dragOffset;
            Vector3 gridCenter = SnapToGrid(rawPosition);
            float hoveringY = 0f + pivotToBottomOffset + 0.2f;
            selectedObject.transform.position = new Vector3(gridCenter.x, hoveringY, gridCenter.z);
        }

        // Drop logic: Space to return to inventory, Left click to place
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            StoreSelectedObject();
        }
        else if (Input.GetMouseButtonDown(0) && selectedObject != null)
        {
            DropObject();
        }
    }

    // [NEW] 回收家具逻辑
    private void StoreSelectedObject()
    {
        if (selectedObject == null) return;

        Debug.Log($"已回收家具: {selectedObject.name}");

        BuildMenuUI buildMenuUI = FindFirstObjectByType<BuildMenuUI>();
        if (buildMenuUI != null)
        {
            buildMenuUI.RetrieveFurniture(selectedObject);
        }
        else
        {
            Destroy(selectedObject);
        }

        selectedObject = null;
        
        // 播放回收音效（可选，可复用 placeObjectSound 或添加新音效）
        if (audioSource != null && selectObjectSound != null)
            audioSource.PlayOneShot(selectObjectSound); 
    }

    private Vector3 SnapToGrid(Vector3 worldPosition)
    {
        float relativeX = worldPosition.x - gridOrigin.x;
        float relativeZ = worldPosition.z - gridOrigin.z;

        int xIndex = Mathf.FloorToInt(relativeX / cellSize);
        int zIndex = Mathf.FloorToInt(relativeZ / cellSize);

        xIndex = Mathf.Clamp(xIndex, 0, gridWidth - 1);
        zIndex = Mathf.Clamp(zIndex, 0, gridHeight - 1);

        float x = gridOrigin.x + (xIndex * cellSize) + (cellSize * 0.5f);
        float z = gridOrigin.z + (zIndex * cellSize) + (cellSize * 0.5f);

        return new Vector3(x, worldPosition.y, z);
    }

    private void DropObject()
    {
        if (selectedObject == null) return;
        
        Vector3 currentPos = selectedObject.transform.position;
        Vector3 finalPos = new Vector3(currentPos.x, pivotToBottomOffset, currentPos.z);

        if (IsPlayerColliding(finalPos))
        {
            Debug.Log("放置失败：位置被玩家占用");
            return;
        }
        FinishDropping(finalPos);
    }

    private void ForceDropObject()
    {
        if (selectedObject == null) return;
        Vector3 currentPos = selectedObject.transform.position;
        Vector3 finalPos = new Vector3(currentPos.x, pivotToBottomOffset, currentPos.z);
        FinishDropping(finalPos);
    }

    private void FinishDropping(Vector3 pos)
    {
        selectedObject.transform.position = pos;
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (audioSource != null && placeObjectSound != null)
        {
            audioSource.PlayOneShot(placeObjectSound);
        }

        selectedObject = null;
    }

    private bool IsPlayerColliding(Vector3 targetPos)
    {
        if (player == null) return false;
        float distance = Vector2.Distance(new Vector2(targetPos.x, targetPos.z), 
                                          new Vector2(player.transform.position.x, player.transform.position.z));
        return distance < (cellSize * 0.8f);
    }
}

