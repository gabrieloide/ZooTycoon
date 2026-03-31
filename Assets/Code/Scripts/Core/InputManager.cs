using UnityEngine;
using UnityEngine.InputSystem;

namespace ZooTycoon.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public InputSystem_Actions actions { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                actions = new InputSystem_Actions();

                actions.Player.ToggleMode.performed += ctx => GameManager.Instance.ToggleBuildMode();

                actions.Enable();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this && actions != null)
            {
                actions.Disable();
            }
        }
    }
}
