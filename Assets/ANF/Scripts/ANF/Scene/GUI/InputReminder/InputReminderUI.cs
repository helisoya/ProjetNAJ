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
	/// Handles the various visual input reminders
	/// </summary>
    [System.Serializable]
    public class InputReminderUI : GUIComponent
    {
        [Header("Components")]
        [SerializeField] private RectTransform reminderRoot;
        [SerializeField] private InputReminderUIButton reminderPrefab;
        [SerializeField] private SerializedDictionary<string, InputReminderData> registeredReminders;

        public override void OnInitialize()
        {
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
        }

        public override void OnDisabled()
        {
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        public override void OnSave(JSON json)
        {
            foreach (string key in registeredReminders.Keys)
            {
                json.Add(key, registeredReminders[key].enabled);
            }
        }

        public override bool OnLoad(JSON json)
        {
            foreach (string key in json.Keys)
            {
                if (registeredReminders.ContainsKey(key))
                    SetReminderEnabled(key, json.GetBool(key));
            }

            return true;
        }

        /// <summary>
		/// Gets the state of all registered reminders
		/// </summary>
		/// <returns>Their state</returns>
        public bool[] GetRemindersState()
        {
            bool[] result = new bool[registeredReminders.Count];

            int i = 0;
            foreach (string key in registeredReminders.Keys)
            {
                result[i] = registeredReminders[key].enabled;
                i++;
            }

            return result;
        }

        /// <summary>
		/// Sets the state of all registered reminders
		/// </summary>
		/// <param name="state">Their new state</param>
        public void SetRemindersState(bool[] state)
        {
            if (state == null || state.Length != registeredReminders.Count)
                return;

            int i = 0;
            foreach (string key in registeredReminders.Keys)
            {
                SetReminderEnabled(key, state[i]);
                i++;
            }
        }


        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override bool OnChangeScene()
        {
            return true;
        }

        public override bool IsLoadingOrCleaningUp()
        {
            return false;
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
