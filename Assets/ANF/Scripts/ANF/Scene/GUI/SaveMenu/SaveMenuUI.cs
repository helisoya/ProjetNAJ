using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the save menu located within the pause menu
    /// </summary>
    public class SaveMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Save Menu")]
        [SerializeField] private Locals.LocalizedText titleText;
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private SaveMenuButton buttonPrefab;
        [SerializeReference, SubclassSelector(AllowNull = false)] private SaveMenuSlotInfo slotInfo;
        [SerializeField] private int slotsPerRow = 4;

        [Header("Confirm Popup")]
        [SerializeField] private RectTransform confirmPopupRoot;
        [SerializeField] private SaveMenuButton confirmPopupPreviewButton;
        [SerializeField] private RectTransform confirmPopupCancelButton;
        [SerializeField] private RectTransform confirmPopupAcceptButton;

        private int currentButtonIdx;
        private SaveMenuButton[] buttons;
        private Vector2Int currentButtonInputSide = new Vector2Int();
        private float cooldownToNextButtonIncrement = 0;
        private Persistent.AudioManager audioManager;

        private bool onConfirmButton;
        private SaveMenuButtonData currentPopupData;
        private bool inPopup;

        private bool inSaveMode;

        public override void OnInitialize()
        {
            inSaveMode = false;
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide.x != 0 || currentButtonInputSide.y != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    if (inPopup && currentButtonInputSide.x != 0)
                        ChangePopupButton(!onConfirmButton);
                    else
                        IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.5f;
                }
            }
        }

        /// <summary>
        /// Changes if the pause menu is enabled or not
        /// </summary>
        /// <param name="enabled">True if the pause menu is now enabled</param>
        /// <param name="inSaveMode">True if the pause menu should be in save mode</param>
        public void SetEnabled(bool enabled, bool inSaveMode)
        {
            this.inSaveMode = inSaveMode;
            SetEnabled(enabled);
        }

        /// <summary>
        /// Changes if the menu is in save mode
        /// </summary>
        /// <param name="inSaveMode">True if in save mode</param>
        public void SetIsInSaveMode(bool inSaveMode)
        {
            this.inSaveMode = inSaveMode;
        }

        /// <summary>
		/// Generates a save button
		/// </summary>
		/// <param name="settings">The ANF settings</param>
		/// <param name="saveName">The save's name</param>
		/// <param name="saveIcon">The save's icon</param>
        /// <param name="id">The slot's id</param>
        /// <param name="interactable">True if interactable</param>
        /// <returns>The generated button</returns>
        private SaveMenuButton GenerateSaveButton(ANFSettings settings, string saveName, string saveIcon, int id, bool interactable)
        {
            string savePath = SaveUtils.GetSavePath(saveName, settings.saveFolder);

            bool saveFileExists = SaveUtils.FileExists(savePath);
            JSON saveFile = SaveUtils.LoadJSON(savePath);
            string label = slotInfo.GetLabel(saveFile);
            Sprite bgSprite = slotInfo.GetBackground(saveFile);

            if (interactable && ((!saveFileExists && !inSaveMode) || (inSaveMode && id == 0))) // No manual save to autosave and no load to empty stuff
                interactable = false;

            SaveMenuButton button = Instantiate(buttonPrefab, buttonsRoot);
            button.Initialize(id, this,
            new SaveMenuButtonData()
            {
                interactable = interactable,
                saveFileExists = saveFileExists,
                saveFileIcon = saveIcon,
                saveFileName = savePath,
                label = label,
                bgSprite = bgSprite,
            });

            return button;
        }

        public override void OnEnabled()
        {
            ANFSettings settings = PersistentDataManager.instance.GetANFSettings();
            titleText.SetNewKey(inSaveMode ? "SaveMenu_Title_Save" : "SaveMenu_Title_Load");

            foreach (Transform child in buttonsRoot)
                Destroy(child.gameObject);

            buttons = new SaveMenuButton[settings.saveSlotsAmount + 1];
            buttons[0] = GenerateSaveButton(settings, "autosave", "A", 0, true);

            for (int i = 0; i < settings.saveSlotsAmount; i++)
            {
                buttons[i + 1] = GenerateSaveButton(settings, i.ToString(), i.ToString(), i + 1, true);
            }

            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            currentButtonInputSide.x = 0;
            currentButtonInputSide.y = 0;
            cooldownToNextButtonIncrement = 0;

            buttons[currentButtonIdx].OnEnter();
        }

        public override void OnDisabled()
        {
            if (inPopup)
                CloseConfirmPopup();

            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (inPopup)
                    ConfirmCurrentPopupButton();
                else
                    buttons[currentButtonIdx].OnClick();
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (inPopup)
                    CloseConfirmPopup();
                else
                {
                    if (audioManager != null)
                        audioManager.PlayUICursorCancelSFX();

                    SetEnabled(false);
                }
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
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.x = value.x < 0 ? 1 : -1;

                        if (inPopup)
                            ChangePopupButton(!onConfirmButton);
                        else
                            IncrementButtonWithInput();
                    }
                }

                if (Mathf.Abs(value.y) >= 0.9f && !inPopup)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.y = value.y < 0 ? 1 : -1;
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

            if (force || currentButtonIdx != id)
            {
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
            SetCurrentButton((currentButtonIdx - currentButtonInputSide.x + currentButtonInputSide.y * slotsPerRow + buttons.Length) % buttons.Length);
        }

        /// <summary>
        /// Opens the confirm popup
        /// </summary>
        /// <param name="data">The slot's data</param>
        public void OpenConfirmPopup(SaveMenuButtonData data)
        {
            currentButtonInputSide.x = 0;
            currentButtonInputSide.y = 0;
            cooldownToNextButtonIncrement = 0;
            inPopup = true;
            currentPopupData = new SaveMenuButtonData()
            {
                saveFileExists = data.saveFileExists,
                saveFileIcon = data.saveFileIcon,
                saveFileName = data.saveFileName,
                bgSprite = data.bgSprite,
                interactable = false,
                label = data.label
            };

            confirmPopupPreviewButton.Initialize(-1, this, currentPopupData);

            onConfirmButton = true;
            ChangePopupButton(false);
            confirmPopupRoot.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutBack);
            confirmPopupRoot.DOShakeRotation(0.4f, new Vector3(0, 0, 5.0f)).OnComplete(() =>
            {
                confirmPopupRoot.DORotate(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
            });
        }

        /// <summary>
        /// Closes the confirm popup
        /// </summary>
        public void CloseConfirmPopup()
        {
            inPopup = false;
            currentPopupData = null;

            currentButtonInputSide.x = 0;
            currentButtonInputSide.y = 0;
            cooldownToNextButtonIncrement = 0;

            confirmPopupRoot.DOScale(Vector3.zero, 0.75f).SetEase(Ease.InBack);
        }

        /// <summary>
        /// Confims and applies the current popup selection
        /// </summary>
        public void ConfirmCurrentPopupButton()
        {
            if (onConfirmButton)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                // Save / Load
                if (inSaveMode)
                {
                    SaveUtils.SavePlayerData(PersistentDataManager.instance.GetPlayerData(), manager, SceneManager.GetActiveScene().name, currentPopupData.saveFileName);

                    JSON newFile = SaveUtils.LoadJSON(currentPopupData.saveFileName);
                    string newLabel = slotInfo.GetLabel(newFile);
                    Sprite newSprite = slotInfo.GetBackground(newFile);

                    buttons[currentButtonIdx].UpdateInfos(newLabel, newSprite);
                    confirmPopupPreviewButton.UpdateInfos(newLabel, newSprite);
                }
                else
                {
                    if (PersistentDataManager.instance.GetGlobalData().GetComponent<LoadStateContainer>(out LoadStateContainer container))
                    {
                        string nextScene = PersistentDataManager.instance.GetANFSettings().gameScene;
                        JSON saveFile = SaveUtils.LoadJSON(currentPopupData.saveFileName);
                        if (saveFile != null && saveFile.ContainsKey("currentScene"))
                            nextScene = saveFile.GetString("currentScene");

                        container.SetToLoadSaveFile(currentPopupData.saveFileName);

                        manager.ChangeScene(nextScene);
                        return;
                    }
                }
            }
            else
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();
            }

            CloseConfirmPopup();
        }

        /// <summary>
        /// Changes the currently selected popup button
        /// </summary>
        /// <param name="onConfirmButton">True if the user is on the confirm button</param>
        /// <param name="force">True if no check should be applied</param>
        public void ChangePopupButton(bool onConfirmButton)
        {
            if (onConfirmButton != this.onConfirmButton)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                this.onConfirmButton = onConfirmButton;

                confirmPopupAcceptButton.DOScale(Vector3.one * (onConfirmButton ? 1.2f : 1.0f), 0.5f).SetEase(Ease.OutBounce);
                confirmPopupCancelButton.DOScale(Vector3.one * (!onConfirmButton ? 1.2f : 1.0f), 0.5f).SetEase(Ease.OutBounce);
                confirmPopupAcceptButton.GetComponent<Image>().DOColor(Color.white * new Vector4(
                    (onConfirmButton ? 0.9f : 1.0f),
                    1.0f,
                    (onConfirmButton ? 0.9f : 1.0f),
                    1.0f), 0.5f).SetEase(Ease.OutQuad);
                confirmPopupCancelButton.GetComponent<Image>().DOColor(Color.white * new Vector4(
                    (!onConfirmButton ? 0.9f : 1.0f),
                    1.0f,
                    (!onConfirmButton ? 0.9f : 1.0f),
                    1.0f), 0.5f).SetEase(Ease.OutQuad);
            }
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

        public override void OnChangeScene()
        {
            OnUnRegisterInputs();
        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }
    }





    /// <summary>
	/// Represents a way to get slot info from a save file
	/// </summary>
    [System.Serializable]
    public abstract class SaveMenuSlotInfo
    {
        /// <summary>
		/// Gets the slot's label info from a savefile
		/// </summary>
		/// <param name="saveFile">The save file</param>
		/// <returns>The save slot's label</returns>
        public abstract string GetLabel(JSON saveFile);

        /// <summary>
        /// Gets the slot's background from a savefile
        /// </summary>
        /// <param name="saveFile">The save file</param>
        /// <returns>The save slot's label</returns>
        public abstract Sprite GetBackground(JSON saveFile);
    }

    /// <summary>
	/// The default ANF Slot Info.
    /// The label is the player name and the background is different depending on if the savefile exists
	/// </summary>
    [System.Serializable]
    public class SaveMenuSlotDefaultInfo : SaveMenuSlotInfo
    {
        [SerializeField] private Sprite bgSavefileExists;
        [SerializeField] private Sprite bgSavefileDontExist;

        public override string GetLabel(JSON saveFile)
        {
            if (saveFile != null)
            {
                try
                {
                    Locals.Locals locals;
                    PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out locals);

                    string result = "";
                    JSON playerVariableJson = saveFile.GetJSON("playerData").GetJSON(typeof(PlayerVariableContainer).FullName);
                    if (playerVariableJson.ContainsKey("playerName"))
                        result += playerVariableJson.GetString("playerName");

                    if (playerVariableJson.ContainsKey("location"))
                        result += "<br>" + (locals == null ? playerVariableJson.GetString("location") :
                            locals.GetLocal(playerVariableJson.GetString("location")));

                    return result;
                }
                catch
                {
                }
            }

            return "";
        }

        public override Sprite GetBackground(JSON saveFile)
        {
            if (saveFile == null)
                return bgSavefileDontExist;
            return bgSavefileExists;
        }
    }
}

