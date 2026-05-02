using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    private string currentContent = "Habitats";
    public event Action OnOpenShop;
    public event Action OnCloseShop;
    public event Action OnOpenHabitats;
    public event Action OnOpenAnimals;
    public event Action OnOpenDecorations;
    public void OpenShop()
    {
        OnOpenShop?.Invoke();
    }
    public void CloseShop()
    {
        OnCloseShop?.Invoke();
        ClearContent();
        currentContent = null;
    }
    public void OpenHabitats()
    {
        ClearContent();
        currentContent = "Habitats";
        OnOpenHabitats?.Invoke();
    }
    public void OpenAnimals()
    {
        ClearContent();
        currentContent = "Animals";
        OnOpenAnimals?.Invoke();
    }
    public void OpenDecorations()
    {
        ClearContent();
        currentContent = "Decorations";
        OnOpenDecorations?.Invoke();
    }

    public void ClearContent()
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
    public string GetCurrentContent() => currentContent;
}