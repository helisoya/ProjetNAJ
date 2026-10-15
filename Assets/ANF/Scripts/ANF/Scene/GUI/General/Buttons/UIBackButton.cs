using ANF.Persistent;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a button that can close a linked GUI Component
    /// </summary>
    public class UIBackButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private GUIComponent linkedComponent;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out AudioManager audioManager))
                audioManager.PlayUICursorCancelSFX();

            linkedComponent.SetEnabled(false);

        }

    }
}

