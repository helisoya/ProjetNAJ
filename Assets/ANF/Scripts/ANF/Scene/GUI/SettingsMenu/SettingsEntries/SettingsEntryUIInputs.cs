using ANF.Persistent;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANF.GUI
{
    /// <summary>
	/// Handles audio events for settings entries
	/// </summary>
    public class SettingsEntryUIInputs : MonoBehaviour, ISelectHandler, ISubmitHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
                audioManager.PlayUICursorMoveSFX();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
                audioManager.PlayUICursorConfirmSFX();
        }
    }
}
