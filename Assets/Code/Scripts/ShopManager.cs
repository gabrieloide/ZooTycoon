using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private ShopCatalog habitats;
    [SerializeField] private ShopCatalog animals;
    [SerializeField] private ShopCatalog decorations;
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
        PopulateContent(habitats);
    }
    public void OpenAnimals()
    {
        ClearContent();
        currentContent = "Animals";
        OnOpenAnimals?.Invoke();
        PopulateContent(animals);
    }
    public void OpenDecorations()
    {
        ClearContent();
        currentContent = "Decorations";
        OnOpenDecorations?.Invoke();
        PopulateContent(decorations);
    }

    public void ClearContent()
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
    public string GetCurrentContent() => currentContent;
    private void PopulateContent(ShopCatalog catalog)
    {
        foreach (GameObject item in catalog.items)
        {
            Instantiate(item, parent);
        }
    }
}