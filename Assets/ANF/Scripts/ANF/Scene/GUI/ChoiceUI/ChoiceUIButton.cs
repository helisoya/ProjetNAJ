using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a choice button in the Choice UI
    /// </summary>
    public class ChoiceUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private Image buttonImg;
        [SerializeField] private Locals.LocalizedText label;

        private ChoiceUI choiceUI;
        private int id;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="labelKey">The label's key</param>
        /// <param name="choiceUI">The Choice UI</param>
        public void Initialize(int id, string labelKey, ChoiceUI choiceUI)
        {
            this.id = id;
            this.choiceUI = choiceUI;

            label.SetNewKey(labelKey);

            buttonRoot.localScale = Vector2.zero;
            buttonRoot.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).SetId(transform);
        }

        /// <summary>
        /// Fades and destroy the button
        /// </summary>
        /// <param name="delay">The delay</param>
        /// <param name="actionOnDestroy">The action to perform afterwards (optional)</param>
        public void Fade(float delay, Action actionOnDestroy)
        {
            buttonRoot.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(delay).SetId(transform).OnComplete(() =>
            {
                if (actionOnDestroy != null)
                    actionOnDestroy.Invoke();
            });
        }

        public void OnEnter()
        {
            buttonImg.DOColor(Color.lightGray, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonRoot.DOScale(Vector3.one * 1.05f, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonRoot.DORotate(new Vector3(0, 0, -2.5f), 0.5f).SetEase(Ease.OutBounce).SetId(transform);
        }

        public void OnExit()
        {
            buttonImg.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonRoot.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.OutQuad).SetId(transform);
            buttonRoot.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBounce).SetId(transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                choiceUI.SelectChoice(id);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            choiceUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}