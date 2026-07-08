using ANF.Persistent;
using ANF.Scene;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Leguar.TotalJSON;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents the component responsible for showing dialogs.
    /// Dialogs can comprise commands.
    /// Ex : I want [wait 5,speed 0.5] A CARIBOU [defaultSpeed] tomorrow
	/// </summary>
    [System.Serializable]
    public class InputReminderUI : GUIComponent
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform reminderRoot;
        [SerializeField] private InputReminderUIButton reminderPrefab;
        [SerializeField] private SerializedDictionary<string, InputReminderData> registeredReminders;


        public override void OnInitialize()
        {
            canvasGroup.alpha = 0;

            foreach (InputReminderData reminder in registeredReminders.Values)
            {
                reminder.instanceButton = Instantiate(reminderPrefab, reminderRoot);
                reminder.instanceButton.Initialize(manager, reminder.labelKey, reminder.clickAction,
                    reminder.inputAction, reminder.gamepadBindingIndex, reminder.keyboardBindingIndex);

                reminder.instanceButton.gameObject.SetActive(reminder.enabled);
            }
        }

        public override void OnStart()
        {
        }

        public void OnAutoPlayToggle(bool enabled)
        {
            if (registeredReminders.TryGetValue("autoplay", out InputReminderData reminder))
                reminder.instanceButton.SetLabelStyle(enabled ? TMPro.FontStyles.Underline : TMPro.FontStyles.Normal);
        }

        public void OnSkipModeToggle(bool enabled)
        {
            if (registeredReminders.TryGetValue("skipMode", out InputReminderData reminder))
                reminder.instanceButton.SetLabelStyle(enabled ? TMPro.FontStyles.Underline : TMPro.FontStyles.Normal);
        }

        /// <summary>
		/// Sets if a reminder is enabled or not
		/// </summary>
		/// <param name="id">The reminder's id</param>
		/// <param name="enabled">True if enabled</param>
        public void SetReminderEnabled(string id, bool enabled)
        {
            if (registeredReminders.TryGetValue(id, out InputReminderData reminder))
            {
                reminder.enabled = enabled;
                reminder.instanceButton.gameObject.SetActive(enabled);
            }
        }

        public override void OnUpdate()
        {

        }

        public override void OnEnabled()
        {
            OnUnPaused();
        }

        public override void OnDisabled()
        {
            OnPaused();
        }

        public override void OnPaused()
        {
            canvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnSave(JSON json)
        {
            foreach (string key in registeredReminders.Keys)
            {
                json.Add(key, registeredReminders[key].enabled);
            }
        }

        public override void OnLoad(JSON json)
        {
            foreach (string key in json.Keys)
            {
                if (registeredReminders.ContainsKey(key))
                    SetReminderEnabled(key, json.GetBool(key));
            }
        }


        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
        }

        [System.Serializable]
        public class InputReminderData
        {
            public bool enabled;
            public string labelKey;
            public InputActionReference inputAction;
            public int keyboardBindingIndex;
            public int gamepadBindingIndex;
            [SerializeReference, SubclassSelector(AllowNull = false)] public InputReminderAction clickAction;
            [HideInInspector] public InputReminderUIButton instanceButton;
        }
    }

}
