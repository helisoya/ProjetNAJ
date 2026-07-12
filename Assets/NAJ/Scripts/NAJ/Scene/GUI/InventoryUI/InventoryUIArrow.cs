using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a movement arrow in the Inventory UI
	/// </summary>
    public class InventoryUIArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private bool isLeftButton;
        [SerializeField] private InventoryUI menu;

        public void OnPointerDown(PointerEventData eventData)
        {
            menu.IncrementTab(isLeftButton);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(1.15f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
