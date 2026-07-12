using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents the little examine icon in the Inventory UI
	/// </summary>
    public class InventoryUIExamineIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private InventoryUI menu;

        public void OnPointerDown(PointerEventData eventData)
        {
            menu.ToggleCheckMode();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DORotate(new Vector3(0.0f, 0.0f, -10.0f), 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
