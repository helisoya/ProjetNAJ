using ANF.Persistent;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAJ.GUI
{
    /// <summary>
    /// Represents a button that can close a linked GUI Component
    /// </summary>
    public class DeductionsUIEndButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private DeductionsUI linkedComponent;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (linkedComponent.isEnabled)
                transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (linkedComponent.isEnabled)
                transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !linkedComponent.isEnabled)
                return;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out AudioManager audioManager))
                audioManager.PlayUICursorCancelSFX();

            linkedComponent.SelectCurrentEndNode();

        }

    }
}

