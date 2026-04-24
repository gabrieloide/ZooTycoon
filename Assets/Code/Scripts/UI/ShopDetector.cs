using UnityEngine;
using UnityEngine.EventSystems;


namespace ZooTycoon.UI
{
    public class ShopDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isOnShop;
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("ShopDetector: Pointer Down");
            isOnShop = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("ShopDetector: Pointer Up");
            isOnShop = false;
        }
    }
}