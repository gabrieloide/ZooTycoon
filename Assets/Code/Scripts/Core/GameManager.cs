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

        public void ToggleBuildMode()
        {
            isBuildMode = !isBuildMode;
            OnModeChanged?.Invoke();
            Debug.Log($"Build Mode is now: {(isBuildMode ? "ON" : "OFF")}");
        }
    }
}
