using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a Tab Selection Icon in the Inventory UI
	/// </summary>
    public class InventoryUITabSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private InventoryUI.InventoryTabType linkedTab;
        [SerializeField] private InventoryUI menu;

        public void OnPointerDown(PointerEventData eventData)
        {
            menu.SwitchTab(linkedTab);
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

