using UnityEngine;
using TMPro;
using ZooTycoon.Core;

namespace ZooTycoon.UI
{
    public class UIManager : MonoBehaviour
    {
        public TMP_Text modeText;
        private HabitatBuilder habitatBuilder;
        [SerializeField] private TMP_Text gridSizeText;

        private void Start()
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnModeChanged += UpdateUI;
                UpdateUI();
            }
            habitatBuilder = FindAnyObjectByType<HabitatBuilder>();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnModeChanged -= UpdateUI;
            }
        }
        private void Update()
        {
            if (habitatBuilder == null || gridSizeText == null) return;
            var sizeGrid = habitatBuilder.GetSizeGrid(out bool isCorrect);

            gridSizeText.text = $"{sizeGrid.x} x | {sizeGrid.y} y";



            if (sizeGrid.x >= 2 && sizeGrid.y >= 2 && sizeGrid.x < 8 && sizeGrid.y < 8 && isCorrect)
            {
                gridSizeText.color = Color.green;
            }
            else
            {
                gridSizeText.color = Color.red;
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
                gridSizeText.text = "0 x | 0 y";
            }
        }
    }
}
