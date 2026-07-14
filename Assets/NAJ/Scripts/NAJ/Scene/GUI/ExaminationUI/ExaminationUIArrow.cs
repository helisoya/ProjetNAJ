using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a movement arrow in the Examination UI
	/// </summary>
    public class ExaminationUIArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private bool isLeftButton;
        [SerializeField] private ExaminationUI menu;
        private bool canBeInteractedWith = false;

        /// <summary>
		/// Changes if the button is interactable or not
		/// </summary>
		/// <param name="canBeInteractedWith">True if interactable</param>
        public void SetCanBeInteractedWith(bool canBeInteractedWith)
        {
            this.canBeInteractedWith = canBeInteractedWith;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (canBeInteractedWith)
                menu.TryChangePart(isLeftButton);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (canBeInteractedWith)
                transform.DOScale(1.15f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (canBeInteractedWith)
                transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
