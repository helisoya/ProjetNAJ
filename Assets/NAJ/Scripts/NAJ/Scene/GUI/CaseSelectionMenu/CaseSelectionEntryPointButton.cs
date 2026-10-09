using ANF.Locals;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents an entry point button in the case selection menu
	/// </summary>
    public class CaseSelectionEntryPointButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        [SerializeField] private LocalizedText label;
        [SerializeField] private Image buttonImage;
        private int linkedIdx;
        private CaseSelectionMenu menu;

        /// <summary>
		/// Initialize the component
		/// </summary>
		/// <param name="linkedId">Its linked index</param>
		/// <param name="labelKey">Its label key</param>
		/// <param name="caseSelectionMenu">The parent menu</param>
        public void Initialize(int linkedId, string labelKey, CaseSelectionMenu caseSelectionMenu)
        {
            label.SetNewKey(labelKey);
            this.linkedIdx = linkedId;
            menu = caseSelectionMenu;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            menu.SelectEntryPoint(linkedIdx);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            menu.SetCurrentEntryPoint(linkedIdx, false);
        }

        /// <summary>
		/// Highlights the component
		/// </summary>
        public void Highlight()
        {
            transform.DOScale(Vector3.one * 1.0f, 0.5f).SetEase(Ease.OutBounce);
            buttonImage.DOColor(Color.white * new Vector4(
                0.9f,
                1.0f,
                0.9f,
                1.0f), 0.5f).SetEase(Ease.OutQuad);
        }

        /// <summary>
		/// UnHighlights the component
		/// </summary>
        public void UnHighlight()
        {
            transform.DOScale(Vector3.one * 1.0f, 0.5f).SetEase(Ease.OutBounce);
            buttonImage.DOColor(Color.white * new Vector4(
                1.0f,
                1.0f,
                1.0f,
                1.0f), 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
