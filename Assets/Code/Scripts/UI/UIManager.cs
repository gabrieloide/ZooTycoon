using UnityEngine;
using TMPro;
using ZooTycoon.Core;

namespace ZooTycoon.UI
{
    public class UIManager : MonoBehaviour
    {
        public TMP_Text modeText;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnModeChanged += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnModeChanged -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            if (modeText == null) return;

            if (GameManager.Instance.isBuildMode)
            {
                modeText.text = "Build Mode";
                modeText.color = Color.yellow;
            }
            else
            {
                modeText.text = "Normal Mode";
                modeText.color = Color.white;
            }
        }
    }
}
