using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a part icon in the Examination UI
	/// </summary>
    public class ExaminationUIPartIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Image icon;
        private bool isActiveTab;
        private int tabId;
        private ExaminationUI menu;
        private bool canBeInteractedWith = false;

        /// <summary>
		/// Initialize the icon
		/// </summary>
		/// <param name="id">The icon's tab id</param>
        /// <param name="menu">The menu</param>
        public void Initialize(int id, ExaminationUI menu)
        {
            tabId = id;
            this.menu = menu;
        }

        /// <summary>
        /// Changes if the button is interactable or not
        /// </summary>
        /// <param name="canBeInteractedWith">True if interactable</param>
        public void SetCanBeInteractedWith(bool canBeInteractedWith)
        {
            this.canBeInteractedWith = canBeInteractedWith;
        }

        public void SetIsActiveTab(bool isActiveTab)
        {
            this.isActiveTab = isActiveTab;
            if (isActiveTab)
                icon.DOColor(Color.lightGreen, 0.5f).SetEase(Ease.OutQuad);
            else
                icon.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (canBeInteractedWith)
                menu.TryChangePart(tabId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (canBeInteractedWith)
                icon.DOColor(Color.lightGreen, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveTab && canBeInteractedWith)
                icon.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
