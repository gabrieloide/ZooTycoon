using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera[] cameras;
    public static CameraManager Instance { get; private set; }
    public float moveSpeed = 10f;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void MoveCamera(Vector2 moveInput)
    {
        transform.position += new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * moveSpeed;
    }
    public void ChangeCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == index) ? 10 : 0;
        }
    }

    private void Start()
    {
        if (ZooTycoon.Core.GameManager.Instance != null)
        {
            ZooTycoon.Core.GameManager.Instance.OnModeChanged += HandleModeChanged;
            HandleModeChanged();
        }
    }

    private void OnDestroy()
    {
        if (ZooTycoon.Core.GameManager.Instance != null)
        {
            ZooTycoon.Core.GameManager.Instance.OnModeChanged -= HandleModeChanged;
        }
    }

    private void HandleModeChanged()
    {
        // Asumiendo que índice 0 es Normal y 1 es Modo Construcción
        int targetIndex = ZooTycoon.Core.GameManager.Instance.isBuildMode ? 1 : 0;
        ChangeCamera(targetIndex);
    }
}