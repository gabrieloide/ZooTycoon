using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private string panelParentName = "Panel";

    private Dictionary<string, GameObject> _panels = new Dictionary<string, GameObject>();
    private Stack<string> _history = new Stack<string>();
    private string _currentPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePanels();
    }

    public void InitializePanels()
    {
        GameObject panelParent = GameObject.Find(panelParentName);

        if (panelParent == null)
        {
            Debug.LogError($"PanelManager: Could not find the parent GameObject named '{panelParentName}' in the hierarchy.");
            return;
        }

        _panels.Clear();
        foreach (Transform child in panelParent.transform)
        {
            if (!_panels.ContainsKey(child.name))
            {
                _panels.Add(child.name, child.gameObject);
            }
        }
    }

    public void OpenPanel(string panelName)
    {
        if (!_panels.TryGetValue(panelName, out GameObject panel))
        {
            Debug.LogWarning($"PanelManager: Panel '{panelName}' not found.");
            return;
        }

        if (!string.IsNullOrEmpty(_currentPanel))
        {
            if (_currentPanel == panelName) return;

            _panels[_currentPanel].SetActive(false);
            _history.Push(_currentPanel);
        }

        panel.SetActive(true);
        _currentPanel = panelName;
    }

    public void Back()
    {
        if (_history.Count == 0)
        {
            Debug.LogWarning("PanelManager: No history to go back to.");
            return;
        }

        if (!string.IsNullOrEmpty(_currentPanel))
        {
            _panels[_currentPanel].SetActive(false);
        }

        string previousPanelName = _history.Pop();
        _panels[previousPanelName].SetActive(true);
        _currentPanel = previousPanelName;
    }

    public void ClosePanel(string panelName)
    {
        if (_panels.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(false);
            if (_currentPanel == panelName) _currentPanel = null;
        }
    }
}
