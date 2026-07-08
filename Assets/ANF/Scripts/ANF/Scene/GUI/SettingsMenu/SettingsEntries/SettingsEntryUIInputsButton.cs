using ANF.Persistent;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANF.GUI
{
    /// <summary>
	/// Handles audio / visual events for settings buttons
	/// </summary>
    public class SettingsEntryUIInputsButton : MonoBehaviour, ISelectHandler, ISubmitHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnDeselect(BaseEventData eventData)
        {
            GetComponent<RectTransform>().DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSelect(null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnDeselect(null);
        }

        public void OnSelect(BaseEventData eventData)
        {
            GetComponent<RectTransform>().DOScale(1.2f, 0.5f).SetEase(Ease.OutQuad);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
                audioManager.PlayUICursorMoveSFX();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            GetComponent<RectTransform>().DOPunchScale(-Vector3.one * 0.5f, 0.5f).SetEase(Ease.OutQuad);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
                audioManager.PlayUICursorConfirmSFX();
        }
    }
}
