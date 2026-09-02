using System.Collections.Generic;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace NAJ.GUI
{
    /// <summary>
    /// Represents the inventory / case record
    /// </summary>
    public class InventoryUI : GUIComponent
    {
        /// <summary>
        /// Represents a general mode for the inventory
        /// </summary>
        public enum InventoryMode
        {
            /// <summary>
			/// Can only view evidence and profile, not present them
			/// </summary>
			ViewOnly,

            /// <summary>
			/// Can view AND present evidence and profile
			/// </summary>
            CanPresent,

            /// <summary>
			/// User must present any type of evidence/profile. They cannot exit the inventory
			/// </summary>
            MustPresentAny,

            /// <summary>
            /// User must present evidence. They cannot exit the inventory
            /// </summary>
            MustPresentEvidence,

            /// <summary>
            /// User must present a profile. They cannot exit the inventory
            /// </summary>
            MustPresentProfile
        }

        /// <summary>
		/// Represents the available tab types in the inventory
		/// </summary>
        public enum InventoryTabType
        {
            Evidence = 0,
            Profile = 1,
            Max = 1
        }


        [Header("Background")]
        [SerializeField] private string[] guiToPauseOnEnable;
        [SerializeField] private CanvasGroup canvasGroup;
        private bool repauseNextFrame = false;

        [Header("Item Details")]
        [SerializeField] private GameObject itemDetailRoot;
        [SerializeField] private LocalizedText itemTitle;
        [SerializeField] private LocalizedText itemDesc;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject checkIcon;

        [Header("Tab Selection")]
        [SerializeField] private InventoryUITabSelector[] tabSelectors;
        [SerializeField] private GameObject[] tabSwitchReminders;
        [SerializeField] private RectTransform tabSelectorViewer;


        [Header("Item List")]
        [SerializeField] private RectTransform[] tabRoots;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private InventoryUIButton buttonPrefab;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private int slotsPerLine = 5;
        private int[] currentButtonsIdx;
        private InventoryTabType currentTabType;
        private string currentID;
        public InventoryMode currentMode { get; private set; }

        /// <summary>
		/// Items comes under the format TYPE/ID (Ex : Evidence/TestEvidence1)
		/// </summary>
        public string selectedItem { get; private set; }

        [Header("Input Reminders")]
        [SerializeField] private string[] inputRemindersToDisable;
        private bool[] cachedPreviousReminders;

        [Header("Check Mode")]
        [SerializeField] private CanvasGroup checkGroup;
        [SerializeField] private Image checkImage;
        [SerializeField] private InventoryUIArrow checkLeft;
        [SerializeField] private InventoryUIArrow checkRight;
        [SerializeField] private RectTransform checkTabsRoot;
        [SerializeField] private InventoryUITabIcon checkTabIconPrefab;
        private InventoryUITabIcon[] checkTabs;
        private bool inCheckMode;
        private uint currentCheckId;

        private Dictionary<string, NAJCaseProfile> allProfiles;
        private Dictionary<string, NAJCaseEvidence> allEvidence;

        private InventoryUIButton[][] buttons;
        private Vector2Int currentButtonInputSide;
        private float cooldownToNextButtonIncrement = 0;
        private AudioManager audioManager;
        private bool skipFirstSelectSFX = false;
        private float cursorMoveCooldown = 0.25f;

        public override void OnInitialize()
        {
            currentMode = InventoryMode.ViewOnly;
            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;

            int tabAmount = (int)InventoryTabType.Max + 1;
            currentButtonsIdx = new int[tabAmount];
            buttons = new InventoryUIButton[tabAmount][];
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Inventory").performed += OnInventoryInput;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                cursorMoveCooldown = (float)settings.Register("GeneralMenu_CursorCooldown", SettingsContainer.SettingsDataType.Float, OnCursorCooldownChange);
        }

        private void OnCursorCooldownChange(object value)
        {
            cursorMoveCooldown = (float)value;
        }

        public override void OnUpdate()
        {
            if (repauseNextFrame)
            {
                repauseNextFrame = false;
                manager.GetWorld().SetPausedAll(true);
                gui.SetComponentsPaused(guiToPauseOnEnable, true);
            }

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
		/// Sets the current mode for the inventory UI. 
        /// SET THIS BEFORE OPENING THE MENU
		/// </summary>
		/// <param name="inventoryMode">The new mode</param>
        public void SetCurrentMode(InventoryMode inventoryMode)
        {
            this.currentMode = inventoryMode;
        }

        /// <summary>
		/// Clears the currently selected item
		/// </summary>
        public void ClearSelectedItem()
        {
            selectedItem = null;
        }

        /// <summary>
		/// Tries to toggles the menu if possible
		/// </summary>
		/// <param name="enabled">True if the menu should be enabled</param>
        public void TrySetEnabled(bool enabled)
        {
            if (inCheckMode && !enabled && isEnabled)
                ToggleCheckMode();
            else if (currentMode == InventoryMode.ViewOnly || currentMode == InventoryMode.CanPresent)
                SetEnabled(enabled);
        }

        /// <summary>
        /// Initialize a button array
        /// </summary>
        /// <param name="list">The linked list</param>
        /// <param name="type">The list's type</param>
        /// <param name="buttons">The button's array</param>
        public void InitializeButtons(List<string> list, InventoryTabType type, ref InventoryUIButton[] buttons)
        {
            int target = list.Count;
            buttons = new InventoryUIButton[target];

            for (int i = 0; i < target; i++)
            {
                InventoryUIButtonData data = new InventoryUIButtonData();
                data.isValid = i < list.Count;
                if (!data.isValid)
                {
                    data.icon = defaultIcon;
                }
                else
                {
                    data.id = list[i];
                    data.type = type;
                    if (type == InventoryTabType.Evidence)
                        data.icon = allEvidence[data.id].LoadIcon();
                    else
                        data.icon = allProfiles[data.id].LoadIcon();

                    if (data.icon == null)
                        data.icon = defaultIcon;
                }

                buttons[i] = Instantiate(buttonPrefab, tabRoots[(int)type]);
                buttons[i].Initialize(i, this, data);
            }
        }

        public override void OnEnabled()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer container))
            {
                skipFirstSelectSFX = true;

                manager.GetWorld().SetPausedAll(true);
                gui.SetComponentsPaused(guiToPauseOnEnable, true);

                allEvidence = container.GetAllEvidence();
                allProfiles = container.GetAllProfiles();

                currentTabType = currentMode == InventoryMode.MustPresentProfile ? InventoryTabType.Profile : InventoryTabType.Evidence;

                int maxTabs = (int)InventoryTabType.Max + 1;
                for (int i = 0; i < maxTabs; i++)
                {
                    currentButtonsIdx[i] = 0;
                    tabRoots[i].anchoredPosition = new Vector2(tabRoots[i].anchoredPosition.x, 0.0f);
                    tabRoots[i].gameObject.SetActive(i == (int)currentTabType);
                    foreach (Transform child in tabRoots[i])
                        Destroy(child.gameObject);
                }

                scrollRect.content = tabRoots[(int)currentTabType];

                Vector2 selectorPos = tabSelectorViewer.anchoredPosition;
                selectorPos.x = tabSelectors[(int)currentTabType].GetComponent<RectTransform>().anchoredPosition.x;
                tabSelectorViewer.anchoredPosition = selectorPos;

                inCheckMode = false;
                checkGroup.alpha = 0.0f;
                checkGroup.blocksRaycasts = false;
                selectedItem = null;

                List<string> knownEvidence = container.GetEvidenceInventory();
                List<string> knownProfiles = container.GetProfilesInventory();

                InitializeButtons(knownEvidence, InventoryTabType.Evidence, ref buttons[0]);
                InitializeButtons(knownProfiles, InventoryTabType.Profile, ref buttons[1]);

                currentButtonInputSide.x = 0;
                currentButtonInputSide.y = 0;
                cooldownToNextButtonIncrement = 0;

                bool currentTabHasButtons = buttons[(int)currentTabType].Length > 0;

                if (currentTabHasButtons)
                {
                    buttons[(int)currentTabType][currentButtonsIdx[(int)currentTabType]].OnEnter();
                    buttons[(int)currentTabType][currentButtonsIdx[(int)currentTabType]].OnSelect();
                }
                else
                {
                    // No buttons
                    itemDetailRoot.SetActive(false);
                }

                bool canSwitch = currentMode != InventoryMode.MustPresentEvidence && currentMode != InventoryMode.MustPresentProfile;
                foreach (GameObject obj in tabSwitchReminders)
                    obj.SetActive(canSwitch);

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    cachedPreviousReminders = inputReminder.GetRemindersState();
                    foreach (string reminder in inputRemindersToDisable)
                    {
                        inputReminder.SetReminderEnabled(reminder, false);
                    }

                    inputReminder.SetReminderEnabled("inventoryBack",
                        currentMode == InventoryMode.ViewOnly ||
                        currentMode == InventoryMode.CanPresent);
                    inputReminder.SetReminderEnabled("inventoryPresent", currentMode != InventoryMode.ViewOnly && currentTabHasButtons);
                }
                else
                {
                    cachedPreviousReminders = null;
                }

                canvasGroup.DOFade(1.0f, 0.5f).SetEase(Ease.OutQuad);
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                SetEnabled(false);
            }
        }

        public override void OnDisabled()
        {
            manager.GetWorld().SetPausedAll(false);
            gui.SetComponentsPaused(guiToPauseOnEnable, false);

            if (cachedPreviousReminders != null && gui.GetComponent(out InputReminderUI inputReminder))
            {
                inputReminder.SetRemindersState(cachedPreviousReminders);
            }
            canvasGroup.DOFade(0.0f, 0.5f).SetEase(Ease.OutQuad);
            canvasGroup.blocksRaycasts = false;
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
            repauseNextFrame = true;
        }

        /// <summary>
		/// Toggles the check mode
		/// </summary>
        public void ToggleCheckMode()
        {
            inCheckMode = !inCheckMode;
            checkGroup.blocksRaycasts = inCheckMode;

            if (!inCheckMode)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();


                checkGroup.DOFade(0.0f, 0.5f).SetEase(Ease.OutQuad);
                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    inputReminder.SetReminderEnabled("inventoryCheck", true);
                    inputReminder.SetReminderEnabled("inventoryBack2", false);
                    inputReminder.SetReminderEnabled("inventoryBack",
                        currentMode == InventoryMode.ViewOnly ||
                        currentMode == InventoryMode.CanPresent);
                    inputReminder.SetReminderEnabled("inventoryPresent", currentMode != InventoryMode.ViewOnly);
                }
            }
            else
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                uint imageCount = allEvidence[currentID].checkImagesCount;

                currentCheckId = 0;
                checkLeft.gameObject.SetActive(imageCount != 1);
                checkRight.gameObject.SetActive(imageCount != 1);

                foreach (Transform child in checkTabsRoot)
                    Destroy(child.gameObject);

                checkTabs = new InventoryUITabIcon[imageCount];
                for (int i = 0; i < imageCount; i++)
                {
                    checkTabs[i] = Instantiate(checkTabIconPrefab, checkTabsRoot);
                    checkTabs[i].Initialize(i, this);
                    checkTabs[i].SetIsActiveTab(i == 0);
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    inputReminder.SetReminderEnabled("inventoryCheck", false);
                    inputReminder.SetReminderEnabled("inventoryBack2", true);
                    inputReminder.SetReminderEnabled("inventoryBack", false);
                    inputReminder.SetReminderEnabled("inventoryPresent", false);
                }

                SetCheckImage(0, true, false);

                checkGroup.DOFade(1.0f, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
		/// Sets the current check mode image
		/// </summary>
		/// <param name="id">The new image's id</param>
		/// <param name="force">True if the change must be forced</param>
        /// <param name="playSFXSound">Should selection SFX be played ?</param>
        public void SetCheckImage(uint id, bool force = false, bool playSFXSound = true)
        {
            if (inCheckMode && (force || id != currentCheckId))
            {
                if (audioManager != null && playSFXSound)
                    audioManager.PlayUICursorMoveSFX();

                currentCheckId = id;
                NAJCaseEvidence evidence = allEvidence[currentID];
                if (evidence.canCheck && id < evidence.checkImagesCount)
                {
                    checkImage.sprite = evidence.LoadCheckImage(id);

                    for (uint i = 0; i < evidence.checkImagesCount; i++)
                        checkTabs[i].SetIsActiveTab(i == id);
                }
            }
        }

        /// <summary>
		/// Try to present the current evidence
		/// </summary>
        public void TryPresentCurrentEvidence()
        {
            if (currentMode != InventoryMode.ViewOnly)
            {
                if (currentTabType == InventoryTabType.Evidence)
                    selectedItem = $"Evidence/{currentID}";
                else
                    selectedItem = $"Profile/{currentID}";

                SetEnabled(false);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton() &&
            !inCheckMode && currentTabType == InventoryTabType.Evidence && allEvidence[currentID].canCheck)
            {
                ToggleCheckMode();
            }
        }

        private void OnInventoryInput(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton() && !inCheckMode)
            {
                if (gui.GetComponent(out PauseMenuUI pauseMenu) &&
                    pauseMenu.isEnabled)
                    return;

                if (audioManager != null && isEnabled)
                    audioManager.PlayUICursorCancelSFX();
                if (audioManager != null && !isEnabled)
                    audioManager.PlayUICursorConfirmSFX();

                TrySetEnabled(!isEnabled);
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (audioManager != null && !inCheckMode)
                    audioManager.PlayUICursorCancelSFX();

                TrySetEnabled(false);
            }
        }

        private void OnSwitchLeft(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton() && !inCheckMode)
            {
                IncrementTab(true);
            }
        }

        private void OnSwitchRight(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton() && !inCheckMode)
            {
                IncrementTab(false);
            }
        }

        private void OnPresent(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                TryPresentCurrentEvidence();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.x = value.x > 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (Mathf.Abs(value.y) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.y = value.y > 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }


                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide.x = 0;
                    currentButtonInputSide.y = 0;
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

            int tabType = (int)currentTabType;
            int currentIdx = currentButtonsIdx[tabType];

            if (force || currentIdx != id)
            {
                buttons[tabType][currentIdx].OnExit();
                currentButtonsIdx[tabType] = id;
                buttons[tabType][id].OnEnter();
                buttons[tabType][id].OnSelect();
            }
        }

        /// <summary>
		/// Sets the tab number
		/// </summary>
		/// <param name="goLeft">The tabId</param>
        public void SetCheckModeTab(int tabId)
        {
            if (inCheckMode)
                SetCheckImage((uint)tabId);
        }

        /// <summary>
		/// Increments the check mode tab
		/// </summary>
		/// <param name="goLeft">True if going left</param>
        public void IncrementCheckMode(bool goLeft)
        {
            if (inCheckMode)
            {
                int side = goLeft ? -1 : 1;
                SetCheckImage((uint)(((int)currentCheckId + side + checkTabs.Length) % checkTabs.Length));
            }
        }

        /// <summary>
        /// Increments the tab
        /// </summary>
        /// <param name="goLeft">True if going left</param>
        public void IncrementTab(bool goLeft)
        {
            int side = goLeft ? -1 : 1;
            int max = (int)InventoryTabType.Max + 1;
            SwitchTab((InventoryTabType)(((int)currentTabType + side + max) % max));
        }

        /// <summary>
        /// Changes the current tab
        /// </summary>
        /// <param name="tab">The new tab</param>
        public void SwitchTab(InventoryTabType tab)
        {
            if (currentMode == InventoryMode.MustPresentEvidence ||
                currentMode == InventoryMode.MustPresentProfile ||
                inCheckMode)
                return;

            if (buttons[(int)currentTabType].Length > 0)
                buttons[(int)currentTabType][currentButtonsIdx[(int)currentTabType]].OnExit();
            tabRoots[(int)currentTabType].gameObject.SetActive(false);

            currentTabType = tab;

            tabRoots[(int)currentTabType].gameObject.SetActive(true);
            scrollRect.content = tabRoots[(int)currentTabType];

            bool tabHasButtons = buttons[(int)currentTabType].Length > 0;
            itemDetailRoot.SetActive(tabHasButtons);

            if (tabHasButtons)
            {
                buttons[(int)currentTabType][currentButtonsIdx[(int)currentTabType]].OnEnter();
                buttons[(int)currentTabType][currentButtonsIdx[(int)currentTabType]].OnSelect();
            }

            float newX = tabSelectors[(int)currentTabType].GetComponent<RectTransform>().anchoredPosition.x;
            tabSelectorViewer.DOAnchorPosX(newX, 0.5f).SetEase(Ease.OutQuad);

            if (gui.GetComponent(out InputReminderUI inputReminder))
                inputReminder.SetReminderEnabled("inventoryPresent", currentMode != InventoryMode.ViewOnly && tabHasButtons);
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            if (inCheckMode)
            {
                SetCheckImage((uint)(((int)currentCheckId + currentButtonInputSide.x + checkTabs.Length) % checkTabs.Length));
            }
            else if (buttons[(int)currentTabType].Length > 0)
            {
                int yMult = 1;
                int currentValue = currentButtonsIdx[(int)currentTabType];
                int max = buttons[(int)currentTabType].Length;
                int lastTab = Mathf.FloorToInt(currentValue / slotsPerLine);

                if ((currentButtonInputSide.y == 1 && currentValue < slotsPerLine) ||
                    (currentButtonInputSide.y == -1 && lastTab == Mathf.CeilToInt(max / slotsPerLine)))
                    yMult = 0;

                SetCurrentButton((currentValue + currentButtonInputSide.x - yMult * slotsPerLine * currentButtonInputSide.y + max) % max);

                int currentTab = Mathf.FloorToInt(currentValue / slotsPerLine);
                if (currentTab != lastTab)
                {
                    GridLayout gridLayout = tabRoots[(int)currentTabType].GetComponent<GridLayout>();
                    float yPos = (gridLayout.cellSize.y + gridLayout.cellGap.y) * currentTab;
                    tabRoots[(int)currentTabType].anchoredPosition = new Vector2(
                        tabRoots[(int)currentTabType].anchoredPosition.x,
                        yPos
                    );
                }
            }
        }

        /// <summary>
        /// Show the details for a specific item
        /// </summary>
        /// <param name="data">The item's data</param>
        public void ShowDetails(InventoryUIButtonData data)
        {
            if (!data.isValid)
                return;

            currentID = data.id;

            if (audioManager != null && !skipFirstSelectSFX)
                audioManager.PlayUICursorMoveSFX();
            else if (skipFirstSelectSFX)
                skipFirstSelectSFX = false;

            if (data.type == InventoryTabType.Evidence)
            {
                NAJCaseEvidence evidence = allEvidence[data.id];
                itemDesc.SetNewKey(evidence.GetDescKey());
                itemTitle.SetNewKey(evidence.GetNameKey());
                itemIcon.sprite = data.icon;
                checkIcon.SetActive(evidence.canCheck);
                if (gui.GetComponent(out InputReminderUI inputReminder))
                    inputReminder.SetReminderEnabled("inventoryCheck", evidence.canCheck);
            }
            else
            {
                NAJCaseProfile profile = allProfiles[data.id];
                itemDesc.SetNewKey(profile.GetDescKey());
                itemTitle.SetNewKey(profile.GetNameKey());
                itemIcon.sprite = data.icon;
                checkIcon.SetActive(false);
                if (gui.GetComponent(out InputReminderUI inputReminder))
                    inputReminder.SetReminderEnabled("inventoryCheck", false);
            }
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("SwitchLeft").performed += OnSwitchLeft;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("SwitchRight").performed += OnSwitchRight;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPresent;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("SwitchLeft").performed -= OnSwitchLeft;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("SwitchRight").performed -= OnSwitchRight;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPresent;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed -= OnPauseInput;
        }

        public override bool OnChangeScene()
        {
            if (isEnabled)
                OnUnRegisterInputs();
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Inventory").performed -= OnInventoryInput;
            return true;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return false;
        }

        public override void OnSave(JSON json)
        {
            if (cachedPreviousReminders != null && cachedPreviousReminders.Length != 0)
            {
                json.Add("cachedPreviousReminders", new JArray(cachedPreviousReminders));
            }
            json.Add("currentMode", (int)currentMode);
            json.Add("selectedItem", selectedItem);
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("cachedPreviousReminders"))
            {
                JArray array = json.GetJArray("cachedPreviousReminders");
                cachedPreviousReminders = array.AsBoolArray();
            }

            if (json.ContainsKey("currentMode"))
                currentMode = (InventoryMode)json.GetInt("currentMode");
            if (json.ContainsKey("selectedItem"))
                selectedItem = json.GetString("selectedItem");
        }
    }
}

