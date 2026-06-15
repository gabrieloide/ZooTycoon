using UnityEngine;
using UnityEngine.EventSystems;


namespace ZooTycoon.UI
{
    public class ShopDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isOnShop;

        public static bool IsOverShop { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOnShop = true;
            IsOverShop = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isOnShop = false;
            IsOverShop = false;
        }
    }
}
