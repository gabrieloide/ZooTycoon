using UnityEngine;

namespace ZooTycoon.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool isBuildMode { get; private set; }

        public event System.Action OnModeChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        void Update()
        {
            if (isBuildMode)
            {
                var moveInput = InputManager.Instance.actions.Player.Move.ReadValue<Vector2>().normalized;
                CameraManager.Instance.MoveCamera(moveInput);
            }
        }

        public void ToggleBuildMode()
        {
            isBuildMode = !isBuildMode;
            OnModeChanged?.Invoke();
            Debug.Log($"Build Mode is now: {(isBuildMode ? "ON" : "OFF")}");
        }
    }
}
