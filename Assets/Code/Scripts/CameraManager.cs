using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera[] cameras;
    private void OnEnable()
    {
        if (ZooTycoon.Core.GameManager.Instance != null)
        {
            ZooTycoon.Core.GameManager.Instance.OnModeChanged += OnModeChanged;
        }
    }
    private void OnDisable()
    {
        if (ZooTycoon.Core.GameManager.Instance != null)
        {
            ZooTycoon.Core.GameManager.Instance.OnModeChanged -= OnModeChanged;
        }
    }
    private void OnModeChanged()
    {
        if (ZooTycoon.Core.GameManager.Instance.isBuildMode)
        {
            ChangeCamera(0);
        }
        else
        {
            ChangeCamera(1);
        }
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