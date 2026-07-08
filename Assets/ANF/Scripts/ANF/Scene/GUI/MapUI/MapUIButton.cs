using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a map's button
	/// </summary>
    public class MapUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private Image buttonImg;
        [SerializeField] private RectTransform labelRoot;
        [SerializeField] private Locals.LocalizedText label;
        private string linkedScript;
        private MapUI mapUI;
        private int id;
        private bool isCurrentButton;
        private float labelRestXPos;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="labelKey">The label's key</param>
        /// <param name="linkedScript">The button's linked script</param>
        /// <param name="sprite">The button's sprite</param>
        /// <param name="isCurrentButton">True if the button represents the current player's location</param>
        /// <param name="canvasTransform">The canvas's transforms</param>
        /// <param name="mapUI">The Map UI</param>
        public void Initialize(int id, string labelKey, string linkedScript, Sprite sprite, bool isCurrentButton, RectTransform canvasTransform, MapUI mapUI)
        {
            this.id = id;
            this.mapUI = mapUI;
            this.linkedScript = linkedScript;
            this.isCurrentButton = isCurrentButton;

            buttonImg.sprite = sprite;

            label.SetNewKey(labelKey);
            label.RegisterText();
            label.GetText().ForceMeshUpdate(true, true);
            labelRoot.localScale = new Vector2(0, 1);

            buttonRoot.localScale = Vector2.zero;
            buttonImg.color = isCurrentButton ? Color.lightGreen : Color.white;
            buttonRoot.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetId(transform);

            LayoutRebuilder.ForceRebuildLayoutImmediate(labelRoot);

            float side = buttonRoot.anchoredPosition.x + labelRoot.sizeDelta.x >= canvasTransform.sizeDelta.x ? -1 : 1;

            labelRoot.anchorMin = new Vector2(0.5f + 0.5f * side, 0.5f);
            labelRoot.anchorMax = new Vector2(0.5f + 0.5f * side, 0.5f);
            labelRestXPos = labelRoot.sizeDelta.x / 2.0f * side;
            labelRoot.anchoredPosition = new Vector2(0f, 0f);
        }

        /// <summary>
		/// Gets the linked script for this button
		/// </summary>
		/// <returns>Its linked script</returns>
        public string GetLinkedScript()
        {
            return linkedScript;
        }

        public void OnEnter()
        {
            labelRoot.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            labelRoot.DOAnchorPosX(labelRestXPos, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonImg.DOColor(Color.softRed, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonRoot.SetAsLastSibling();
            buttonRoot.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetId(transform);
        }

        public void OnExit()
        {
            labelRoot.DOScaleX(0, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            labelRoot.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonImg.DOColor(isCurrentButton ? Color.lightGreen : Color.white, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                mapUI.SelectButton(id);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mapUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}

