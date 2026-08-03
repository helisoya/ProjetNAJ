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


        [Header("Background")]
        [SerializeField] private string[] guiToPauseOnEnable;
        [SerializeField] private CanvasGroup canvasGroup;
        private bool repauseNextFrame = false;

        [Header("Item Details")]
        [SerializeField] private LocalizedText itemTitle;
        [SerializeField] private LocalizedText itemDesc;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject checkIcon;

        [Header("Item List")]
        [SerializeField] private RectTransform evidenceRoot;
        [SerializeField] private RectTransform profilesRoot;
        [SerializeField] private InventoryUIButton buttonPrefab;
        [SerializeField] private InventoryUIArrow leftArrow;
        [SerializeField] private InventoryUIArrow rightArrow;
        [SerializeField] private InventoryUITabIcon tabIconPrefab;
        [SerializeField] private RectTransform tabIconsRoot;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private int slotsPerScreen = 8;
        private int currentEvidenceButtonIdx;
        private int currentProfileButtonIdx;
        private bool inEvidenceMode;
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
        private InventoryUITabIcon[] checkTabs;
        private bool inCheckMode;
        private uint currentCheckId;

        private Dictionary<string, NAJCaseProfile> allProfiles;
        private Dictionary<string, NAJCaseEvidence> allEvidence;
        private int maxEvidenceCount;
        private int maxProfileCount;

        private InventoryUITabIcon[] tabIcons;
        private InventoryUIButton[] buttonsEvidence;
        private InventoryUIButton[] buttonsProfiles;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement = 0;
        private AudioManager audioManager;
        private bool skipFirstSelectSFX = false;
        private float cursorMoveCooldown = 0.25f;

        public override void OnInitialize()
        {
            currentMode = InventoryMode.ViewOnly;
            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;
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

            if (currentButtonInputSide != 0)
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
        /// <param name="isEvidence">True if it is an evidence array</param>
        /// <param name="buttons">The button's array</param>
        public void InitializeButtons(List<string> list, bool isEvidence, ref InventoryUIButton[] buttons)
        {
            int target = slotsPerScreen * (Mathf.FloorToInt(list.Count / (float)slotsPerScreen) + 1);
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
                    data.isEvidence = isEvidence;
                    if (isEvidence)
                        data.icon = allEvidence[data.id].LoadIcon();
                    else
                        data.icon = allProfiles[data.id].LoadIcon();

                    if (data.icon == null)
                        data.icon = defaultIcon;
                }

                buttons[i] = Instantiate(buttonPrefab, isEvidence ? evidenceRoot : profilesRoot);
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

                inEvidenceMode = currentMode != InventoryMode.MustPresentProfile;
                currentEvidenceButtonIdx = 0;
                currentProfileButtonIdx = 0;

                inCheckMode = false;
                checkGroup.alpha = 0.0f;
                checkGroup.blocksRaycasts = false;
                selectedItem = null;

                evidenceRoot.anchoredPosition = new Vector2(0.0f, inEvidenceMode ? 40.0f : 110.0f);
                profilesRoot.anchoredPosition = new Vector2(0.0f, inEvidenceMode ? -30.0f : 40.0f);

                foreach (Transform child in evidenceRoot)
                    Destroy(child.gameObject);
                foreach (Transform child in profilesRoot)
                    Destroy(child.gameObject);

                List<string> knownEvidence = container.GetEvidenceInventory();
                List<string> knownProfiles = container.GetProfilesInventory();

                maxEvidenceCount = knownEvidence.Count;
                maxProfileCount = knownProfiles.Count;

                InitializeButtons(knownEvidence, true, ref buttonsEvidence);
                InitializeButtons(knownProfiles, false, ref buttonsProfiles);

                bool arrowVisible = (inEvidenceMode ? maxEvidenceCount : maxProfileCount) > slotsPerScreen;
                leftArrow.gameObject.SetActive(arrowVisible);
                rightArrow.gameObject.SetActive(arrowVisible);

                foreach (Transform child in tabIconsRoot)
                    Destroy(child.gameObject);

                int tabEvidence = Mathf.FloorToInt(maxEvidenceCount / (float)slotsPerScreen) + 1;
                int tabProfiles = Mathf.FloorToInt(maxProfileCount / (float)slotsPerScreen) + 1;
                int maxTab = Mathf.Max(tabEvidence, tabProfiles);

                tabIcons = new InventoryUITabIcon[maxTab];
                for (int i = 0; i < maxTab; i++)
                {
                    tabIcons[i] = Instantiate(tabIconPrefab, tabIconsRoot);
                    tabIcons[i].Initialize(i, this);
                    tabIcons[i].gameObject.SetActive(i < (inEvidenceMode ? tabEvidence : tabProfiles));
                    tabIcons[i].SetIsActiveTab(i == 0);
                }

                currentButtonInputSide = 0;
                cooldownToNextButtonIncrement = 0;

                if (inEvidenceMode)
                {
                    buttonsEvidence[currentEvidenceButtonIdx].OnEnter();
                    buttonsEvidence[currentEvidenceButtonIdx].OnSelect();
                }
                else
                {
                    buttonsProfiles[currentProfileButtonIdx].OnEnter();
                    buttonsProfiles[currentProfileButtonIdx].OnSelect();
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    cachedPreviousReminders = inputReminder.GetRemindersState();
                    foreach (string reminder in inputRemindersToDisable)
                    {
                        inputReminder.SetReminderEnabled(reminder, false);
                    }

                    bool canSwitch = currentMode == InventoryMode.ViewOnly ||
                    currentMode == InventoryMode.CanPresent ||
                    currentMode == InventoryMode.MustPresentAny;

                    inputReminder.SetReminderEnabled("inventoryProfiles", inEvidenceMode && canSwitch);
                    inputReminder.SetReminderEnabled("inventoryBack",
                        currentMode == InventoryMode.ViewOnly ||
                        currentMode == InventoryMode.CanPresent);
                    inputReminder.SetReminderEnabled("inventoryPresent", currentMode != InventoryMode.ViewOnly);
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
                    bool canSwitch = currentMode == InventoryMode.ViewOnly ||
                        currentMode == InventoryMode.CanPresent ||
                        currentMode == InventoryMode.MustPresentAny;

                    inputReminder.SetReminderEnabled("inventoryCheck", true);
                    inputReminder.SetReminderEnabled("inventoryProfiles", !inEvidenceMode && canSwitch);
                    inputReminder.SetReminderEnabled("inventoryEvidence", inEvidenceMode && canSwitch);
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
                    checkTabs[i] = Instantiate(tabIconPrefab, checkTabsRoot);
                    checkTabs[i].Initialize(i, this);
                    checkTabs[i].SetIsActiveTab(i == 0);
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    inputReminder.SetReminderEnabled("inventoryCheck", false);
                    inputReminder.SetReminderEnabled("inventoryProfiles", false);
                    inputReminder.SetReminderEnabled("inventoryEvidence", false);
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
                if (inEvidenceMode)
                    selectedItem = $"Evidence/{currentID}";
                else
                    selectedItem = $"Profile/{currentID}";

                SetEnabled(false);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton() &&
            !inCheckMode && inEvidenceMode && allEvidence[currentID].canCheck)
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
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                TrySetEnabled(false);
            }
        }

        private void OnInventorySwitch(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton() && !inCheckMode)
            {
                SwitchMode();
            }
        }

        private void OnPresent(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                TryPresentCurrentEvidence();
            }
        }

        private void OnMouseScroll(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
            {
                Vector2 value = context.ReadValue<Vector2>();

                if ((value.y < 0 && inEvidenceMode) ||
                    (value.y > 0 && !inEvidenceMode))
                    SwitchMode();
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
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide = value.x > 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide = 0;
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

            int currentIdx = inEvidenceMode ? currentEvidenceButtonIdx : currentProfileButtonIdx;

            if (force || currentIdx != id)
            {
                if (inEvidenceMode)
                {
                    buttonsEvidence[currentEvidenceButtonIdx].OnExit();
                    currentEvidenceButtonIdx = id;
                    buttonsEvidence[currentEvidenceButtonIdx].OnEnter();
                    buttonsEvidence[currentEvidenceButtonIdx].OnSelect();
                }
                else
                {
                    buttonsProfiles[currentProfileButtonIdx].OnExit();
                    currentProfileButtonIdx = id;
                    buttonsProfiles[currentProfileButtonIdx].OnEnter();
                    buttonsProfiles[currentProfileButtonIdx].OnSelect();
                }

                int tabOld = Mathf.FloorToInt(currentIdx / (float)slotsPerScreen);
                int tabNew = Mathf.FloorToInt(id / (float)slotsPerScreen);

                if (tabNew != tabOld)
                {
                    tabIcons[tabOld].SetIsActiveTab(false);
                    tabIcons[tabNew].SetIsActiveTab(true);

                    if (inEvidenceMode)
                        evidenceRoot.DOAnchorPosX(-520 * tabNew, 0.5f).SetEase(Ease.OutQuad);
                    else
                        profilesRoot.DOAnchorPosX(-520 * tabNew, 0.5f).SetEase(Ease.OutQuad);
                }
            }
        }

        /// <summary>
		/// Sets the tab number
		/// </summary>
		/// <param name="goLeft">The tabId</param>
        public void SetTab(int tabId)
        {
            if (inCheckMode)
                SetCheckImage((uint)tabId);
            else
                SetCurrentButton(tabId * slotsPerScreen);
        }

        /// <summary>
		/// Increments the tab number
		/// </summary>
		/// <param name="goLeft">True if going left</param>
        public void IncrementTab(bool goLeft)
        {
            int side = goLeft ? -1 : 1;

            if (inCheckMode)
            {
                SetCheckImage((uint)(((int)currentCheckId + side + checkTabs.Length) % checkTabs.Length));
                return;
            }

            if (inEvidenceMode && maxEvidenceCount != 0)
            {
                SetCurrentButton(Mathf.Min(
                    (currentEvidenceButtonIdx + side * slotsPerScreen + buttonsEvidence.Length) % buttonsEvidence.Length,
                    maxEvidenceCount - 1));
            }

            else if (!inEvidenceMode && maxProfileCount != 0)
            {
                SetCurrentButton(Mathf.Min(
                    (currentProfileButtonIdx + side * slotsPerScreen + buttonsProfiles.Length) % buttonsProfiles.Length,
                    maxProfileCount - 1));
            }
        }

        /// <summary>
        /// Switches between evidence and profile mode
        /// </summary>
        public void SwitchMode()
        {
            if (currentMode == InventoryMode.MustPresentEvidence ||
                currentMode == InventoryMode.MustPresentProfile)
                return;

            inEvidenceMode = !inEvidenceMode;

            if (inEvidenceMode)
            {
                buttonsEvidence[currentEvidenceButtonIdx].OnEnter();
                buttonsProfiles[currentProfileButtonIdx].OnExit();
                buttonsEvidence[currentEvidenceButtonIdx].OnSelect();
                evidenceRoot.DOAnchorPosY(40.0f, 0.5f).SetEase(Ease.OutQuad);
                profilesRoot.DOAnchorPosY(-30.0f, 0.5f).SetEase(Ease.OutQuad);

                bool arrowVisible = maxEvidenceCount > slotsPerScreen;
                leftArrow.gameObject.SetActive(arrowVisible);
                rightArrow.gameObject.SetActive(arrowVisible);

                int currentTab = Mathf.FloorToInt(currentEvidenceButtonIdx / (float)slotsPerScreen);
                int tabs = Mathf.FloorToInt(maxEvidenceCount / (float)slotsPerScreen) + 1;
                for (int i = 0; i < tabIcons.Length; i++)
                {
                    tabIcons[i].gameObject.SetActive(i < tabs);
                    tabIcons[i].SetIsActiveTab(i == currentTab);
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    inputReminder.SetReminderEnabled("inventoryProfiles", true);
                    inputReminder.SetReminderEnabled("inventoryEvidence", false);
                }
            }
            else
            {
                buttonsEvidence[currentEvidenceButtonIdx].OnExit();
                buttonsProfiles[currentProfileButtonIdx].OnEnter();
                buttonsProfiles[currentProfileButtonIdx].OnSelect();
                evidenceRoot.DOAnchorPosY(110.0f, 0.5f).SetEase(Ease.OutQuad);
                profilesRoot.DOAnchorPosY(40.0f, 0.5f).SetEase(Ease.OutQuad);

                bool arrowVisible = maxProfileCount > slotsPerScreen;
                leftArrow.gameObject.SetActive(arrowVisible);
                rightArrow.gameObject.SetActive(arrowVisible);

                int currentTab = Mathf.FloorToInt(currentProfileButtonIdx / (float)slotsPerScreen);
                int tabs = Mathf.FloorToInt(maxProfileCount / (float)slotsPerScreen) + 1;
                for (int i = 0; i < tabIcons.Length; i++)
                {
                    tabIcons[i].gameObject.SetActive(i < tabs);
                    tabIcons[i].SetIsActiveTab(i == currentTab);
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    inputReminder.SetReminderEnabled("inventoryProfiles", false);
                    inputReminder.SetReminderEnabled("inventoryEvidence", true);
                }
            }


        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            if (inCheckMode)
            {
                SetCheckImage((uint)(((int)currentCheckId + currentButtonInputSide + checkTabs.Length) % checkTabs.Length));
            }

            else if (inEvidenceMode && maxEvidenceCount != 0)
                SetCurrentButton((currentEvidenceButtonIdx + currentButtonInputSide + maxEvidenceCount) % maxEvidenceCount);
            else if (!inEvidenceMode && maxProfileCount != 0)
                SetCurrentButton((currentProfileButtonIdx + currentButtonInputSide + maxProfileCount) % maxProfileCount);
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

            if (data.isEvidence)
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
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("InventorySwitch").performed += OnInventorySwitch;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MouseScroll").performed += OnMouseScroll;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPresent;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("InventorySwitch").performed -= OnInventorySwitch;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MouseScroll").performed -= OnMouseScroll;
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

