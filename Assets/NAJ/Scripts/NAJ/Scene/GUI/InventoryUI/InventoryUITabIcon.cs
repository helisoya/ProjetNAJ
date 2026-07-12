using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a tab icon in the Inventory UI
	/// </summary>
    public class InventoryUITabIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Image icon;
        private bool isActiveTab;
        private int tabId;
        private InventoryUI menu;

        /// <summary>
		/// Initialize the icon
		/// </summary>
		/// <param name="id">The icon's tab id</param>
        /// <param name="menu">The menu</param>
        public void Initialize(int id, InventoryUI menu)
        {
            tabId = id;
            this.menu = menu;
        }

        public void SetIsActiveTab(bool isActiveTab)
        {
            this.isActiveTab = isActiveTab;
            if (isActiveTab)
                OnPointerEnter(null);
            else
                OnPointerExit(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            menu.SetTab(tabId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            icon.DOColor(Color.lightGreen, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveTab)
                icon.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
