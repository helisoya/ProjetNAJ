using System;
using ANF.GUI;
using ANF.Locals;
using ANF.Scene;
using ANF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.Persistent
{
    /// <summary>
	/// Represents an input reminder in the Input reminder UI
	/// </summary>
    public class InputReminderUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private LocalizedText label;
        [SerializeField] private AutomaticIcon icon;
        private InputReminderAction action;
        private ANFManager manager;

        /// <summary>
		/// Sets the label's font style
		/// </summary>
		/// <param name="fontStyle">The new font style</param>
        public void SetLabelStyle(TMPro.FontStyles fontStyle)
        {
            label.GetText().fontStyle = fontStyle;
        }

        /// <summary>
		/// Initialize the component
		/// </summary>
		/// <param name="manager">The ANF Manager</param>
		/// <param name="labelKey">The label's key</param>
		/// <param name="reminderAction">The action linked to clicking on the button</param>
		/// <param name="action">The icon's linked action</param>
		/// <param name="bindingIndexGamepad">The binding index for the action on the gamepad</param>
		/// <param name="bindingIndexKeyboard">The binding index for the action on the keyboard</param>
        public void Initialize(ANFManager manager, string labelKey, InputReminderAction reminderAction,
            InputActionReference action, int bindingIndexGamepad, int bindingIndexKeyboard)
        {
            this.action = reminderAction;
            this.manager = manager;
            label.SetNewKey(labelKey);
            label.GetText().ForceMeshUpdate(true);
            icon.SetData(action, bindingIndexKeyboard, bindingIndexGamepad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out AudioManager audioManager))
                audioManager.PlayUICursorConfirmSFX();

            action.OnDown(manager);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            action.OnUp(manager);
        }
    }

    /// <summary>
    /// Class responsible for the action of clicking on the input reminder
    /// </summary>
    [System.Serializable]
    public abstract class InputReminderAction
    {
        public abstract void OnDown(ANFManager manager);
        public abstract void OnUp(ANFManager manager);
    }

    /// <summary>
    /// Handles the autoplay reminder action
    /// </summary>
    [System.Serializable]
    public class InputReminderActionNone : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {

        }

        public override void OnUp(ANFManager manager)
        {

        }
    }

    /// <summary>
    /// Handles the pause menu reminder action
    /// </summary>
    [System.Serializable]
    public class InputReminderActionPauseMenu : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<PauseMenuUI>(out PauseMenuUI pauseMenu))
                pauseMenu.SetEnabled(true);
        }

        public override void OnUp(ANFManager manager)
        {

        }
    }

    /// <summary>
	/// Handles the autoplay reminder action
	/// </summary>
    [System.Serializable]
    public class InputReminderActionAutoPlay : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetWorld().GetComponent<FlowStateHandler>(out FlowStateHandler flowState))
                flowState.ToggleAutoPlay();
        }

        public override void OnUp(ANFManager manager)
        {
            if (manager.GetWorld().GetComponent<FlowStateHandler>(out FlowStateHandler flowState))
                flowState.ToggleAutoPlay();
        }
    }

    /// <summary>
	/// Handles the skip mode reminder action
	/// </summary>
    [System.Serializable]
    public class InputReminderActionSkipMode : InputReminderAction
    {
        public override void OnDown(ANFManager manager)
        {
            if (manager.GetWorld().GetComponent<FlowStateHandler>(out FlowStateHandler flowState))
                flowState.ToggleSkipMode();
        }

        public override void OnUp(ANFManager manager)
        {
            if (manager.GetWorld().GetComponent<FlowStateHandler>(out FlowStateHandler flowState))
                flowState.ToggleSkipMode();
        }
    }
}

