using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the quest menu located within the pause menu
    /// </summary>
    public class QuestsMenuUI : GUIComponent
    {

        [Header("Quest Buttons")]
        [SerializeField] private RectTransform buttonsRoot;
        [SerializeField] private QuestsMenuUIButton buttonPrefab;
        [SerializeField] private QuestsMenuUICategory categoryPrefab;
        [SerializeField] private Scrollbar questsScrollbar;

        [Header("Quest Info")]
        [SerializeField] private Locals.LocalizedText infoNameText;
        [SerializeField] private Locals.LocalizedText infoDescText;
        [SerializeField] private RectTransform infoObjectivesRoot;
        [SerializeField] private Locals.LocalizedText infoObjectivePrefab;
        [SerializeField] private Scrollbar objectivesScrollbar;

        private bool onObjectiveScrollbar;
        private Persistent.AudioManager audioManager;

        private int currentButtonIdx;
        private List<QuestsMenuUIButton> buttons;
        private Vector2Int currentButtonInputSide = Vector2Int.zero;
        private float cooldownToNextButtonIncrement = 0;
        private float cursorMoveCooldown = 0.25f;

        private Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> visibleQuests;

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                cursorMoveCooldown = (float)settings.Register("GeneralMenu_CursorCooldown", SettingsContainer.SettingsDataType.Float, OnCursorCooldownChange);
        }

        private void OnCursorCooldownChange(object value)
        {
            cursorMoveCooldown = (float)value;
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide.x != 0 || currentButtonInputSide.y != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = cursorMoveCooldown;
                }
            }
        }

        /// <summary>
        /// Computes the list of visible quests by category
        /// </summary>
        /// <returns>The list of visible quests</returns>
        private Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> ComputeVisibleQuests()
        {
            Dictionary<string, List<KeyValuePair<Persistent.QuestInfo, int>>> result = new();

            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out QuestInfosContainer questInfos) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer playerVariables))
            {
                Dictionary<string, List<Persistent.QuestInfo>> quests = questInfos.GetQuests();

                List<KeyValuePair<Persistent.QuestInfo, int>> tmpList = null;
                int tmpValue = 0;

                foreach (string category in quests.Keys)
                {
                    tmpList = new();

                    foreach (ANF.Persistent.QuestInfo info in quests[category])
                    {
                        if (playerVariables.GetVariable(info.variableID, out tmpValue) && tmpValue > -1)
                        {
                            tmpList.Add(new(info, tmpValue));
                        }
                    }

                    if (tmpList.Count > 0)
                    {
                        result.Add(category, tmpList);
                    }
                }
            }

            return result;
        }

        public override void OnEnabled()
        {
            visibleQuests = ComputeVisibleQuests();
            onObjectiveScrollbar = false;

            foreach (Transform child in buttonsRoot)
                Destroy(child.gameObject);

            buttons = new();

            int id = 0;
            foreach (string category in visibleQuests.Keys)
            {
                Instantiate(categoryPrefab, buttonsRoot).SetLabelKey($"QuestCategory_{category}");

                foreach (KeyValuePair<Persistent.QuestInfo, int> info in visibleQuests[category])
                {
                    QuestsMenuUIButton button = Instantiate(buttonPrefab, buttonsRoot);
                    button.Initialize(id, this, info);
                    id++;
                    buttons.Add(button);
                }
            }

            currentButtonInputSide.x = 0;
            currentButtonInputSide.y = 0;
            cooldownToNextButtonIncrement = 0;


            if (visibleQuests.Count != 0)
            {
                SetCurrentButton(0, true);
                ShowQuest(buttons[currentButtonIdx].GetData());
            }
            else
            {
                // No visible quests

                infoNameText.SetNewKey("GeneralMenu_Unknown");
                infoDescText.SetNewKey("GeneralMenu_Unknown");

                foreach (Transform child in infoObjectivesRoot)
                    Destroy(child.gameObject);
            }
            buttonsRoot.anchoredPosition = Vector2.zero;
        }

        public override void OnDisabled()
        {
            EventSystem.current.SetSelectedGameObject(null);
            visibleQuests = null;
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && buttons.Count != 0 && !isPaused && context.ReadValueAsButton())
            {
                ShowQuest(buttons[currentButtonIdx].GetData());
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                TriggerDelayedClosing();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && buttons.Count != 0)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.y) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.y = value.y < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.x = value.y < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide.y = 0;
                    currentButtonInputSide.x = 0;
                }
            }
        }

        /// <summary>
		/// Changes the current button
		/// </summary>
		/// <param name="id">The new button's id</param>
        /// <param name="force">True if the id check should be skipped</param>
        public void SetCurrentButton(int id, bool force = false)
        {
            if (id < 0)
                return;

            if (force || currentButtonIdx != id)
            {
                questsScrollbar.value = currentButtonIdx / ((float)buttons.Count - 1);

                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                buttons[currentButtonIdx].OnExit();
                currentButtonIdx = id;
                buttons[currentButtonIdx].OnEnter();
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            if (!onObjectiveScrollbar)
                SetCurrentButton((currentButtonIdx + currentButtonInputSide.y + buttons.Count) % buttons.Count);

            if (currentButtonInputSide.x != 0 && infoObjectivesRoot.childCount > 0)
            {
                onObjectiveScrollbar = !onObjectiveScrollbar && objectivesScrollbar.gameObject.activeInHierarchy;
                EventSystem.current.SetSelectedGameObject(onObjectiveScrollbar ? objectivesScrollbar.gameObject : null);
            }
        }

        /// <summary>
        /// Shows a quest's infos on screen
        /// </summary>
        /// <param name="data">The quest data</param>
        public void ShowQuest(KeyValuePair<Persistent.QuestInfo, int> data)
        {
            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();

            infoNameText.SetNewKey(data.Key.GetNameKey());
            infoDescText.SetNewKey(data.Key.GetDescKey());

            foreach (Transform child in infoObjectivesRoot)
                Destroy(child.gameObject);

            int max = data.Value >= 100 ? data.Key.maxQuestState : data.Value;

            for (int i = 0; i <= max; i++)
            {
                Locals.LocalizedText text = Instantiate(infoObjectivePrefab, infoObjectivesRoot);
                text.SetNewKey(data.Key.GetStateKey(i));
                text.GetText().fontStyle = (i == max && data.Value < 100) ? TMPro.FontStyles.Normal : TMPro.FontStyles.Strikethrough;
                text.GetText().ForceMeshUpdate(true, true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(text.GetComponent<RectTransform>());
            }

            if (data.Value == 100)
                Instantiate(infoObjectivePrefab, infoObjectivesRoot).SetNewKey(data.Key.GetDoneKey());
            else if (data.Value == 101)
                Instantiate(infoObjectivePrefab, infoObjectivesRoot).SetNewKey(data.Key.GetCanceledKey());

            LayoutRebuilder.ForceRebuildLayoutImmediate(infoObjectivesRoot.GetComponent<RectTransform>());

            infoObjectivesRoot.anchoredPosition = Vector2.zero;
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Pause").performed += OnPauseInput;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Pause").performed -= OnPauseInput;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed -= OnPauseInput;
        }

        public override bool OnChangeScene()
        {
            OnUnRegisterInputs();
            return true;
        }

        public override bool IsLoadingOrCleaningUp()
        {
            return false;
        }

        public override void OnSave(JSON json)
        {

        }

        public override bool OnLoad(JSON json)
        {
            return true;
        }
    }
}

