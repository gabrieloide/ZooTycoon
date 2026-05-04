using UnityEngine;

[CreateAssetMenu(fileName = "NewCatalog", menuName = "ZooTycoon/ShopCatalog")]
public class ShopCatalog : ScriptableObject
{
    public enum Category { Habitats, Animals, Decorations }
    [SerializeField] private Category category;
    public GameObject[] items;
}
