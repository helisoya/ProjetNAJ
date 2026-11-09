using ANF.GUI;
using ANF.Persistent;
using ANF.Scene;
using UnityEngine;

namespace NAJ.GUI
{

    /// <summary>
	/// Input Reminder Action for toggling on/off the inventory
	/// </summary>
    [System.Serializable]
    public class NAJInputReminderInventoryToggleAction : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent(out PauseMenuUI menu) &&
                menu.isEnabled)
                return;

            if (manager.GetGUIManager().GetComponent(out InventoryUI inventory))
            {
                inventory.TrySetEnabled(!inventory.isEnabled);

                if (PersistentDataManager.instance.GetGlobalData().GetComponent(out AudioManager audioManager))
                {
                    if (inventory.isEnabled)
                        audioManager.PlaySFX("CursorUnPause", 1.0f);
                    else
                        audioManager.PlaySFX("CursorPause", 1.0f);
                }
            }
        }

        public override void OnUp(ANFManager manager)
        {
        }
    }

    /// <summary>
    /// Input Reminder Action for toggling the check mode
    /// </summary>
    [System.Serializable]
    public class NAJInputReminderInventoryCheckAction : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent(out InventoryUI inventory) &&
                inventory.isEnabled && !inventory.isPaused)
                inventory.ToggleCheckMode();
        }

        public override void OnUp(ANFManager manager)
        {
        }
    }

    /// <summary>
    /// Input Reminder Action for presenting evidence
    /// </summary>
    [System.Serializable]
    public class NAJInputReminderInventoryPresentAction : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent(out InventoryUI inventory) &&
                inventory.isEnabled && !inventory.isPaused)
                inventory.TryPresentCurrentEvidence();
        }

        public override void OnUp(ANFManager manager)
        {
        }
    }

    /// <summary>
    /// Input Reminder Action for pressing on a statement
    /// </summary>
    [System.Serializable]
    public class NAJExaminationPressAction : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI) &&
                examinationUI.isEnabled && !examinationUI.isPaused)
                examinationUI.TryPress();
        }

        public override void OnUp(ANFManager manager)
        {
        }
    }
}

