using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.GUI;
using NAJ.Persistent;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ANF.GUI;
using ANF.Locals;


namespace NAJ.GUI
{
    /// <summary>
    /// Represents the case selection menu
    /// </summary>
    public class CaseSelectionMenu : GUIComponent
    {
        [Header("Case Selection")]
        [SerializeField] private LocalizedText caseNameText;
        [SerializeField] private CaseSelectionArrow leftArrow;
        [SerializeField] private CaseSelectionArrow rightArrow;
        [SerializeField] private RectTransform caseImagesRoot;
        [SerializeField] private Image caseImagePrefab;
        [SerializeField] private CaseInfo[] casesInfo;

        [Header("Confirm Popup")]
        [SerializeField] private RectTransform confirmPopupRoot;
        [SerializeField] private RectTransform confirmPopupCancelButton;
        [SerializeField] private RectTransform confirmPopupAcceptButton;

        private uint maxCaseIdx;
        private int currentCaseIdx;
        private float currentButtonInputSide = 0;
        private float cooldownToNextButtonIncrement = 0;
        private AudioManager audioManager;
        private float cursorMoveCooldown = 0.25f;

        private bool onConfirmButton;
        private bool inPopup;

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
            if (currentButtonInputSide != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    if (inPopup && currentButtonInputSide != 0)
                        ChangePopupButton(!onConfirmButton);
                    else if (!inPopup && currentButtonInputSide != 0)
                        IncrementCaseWithButton(currentButtonInputSide < 0 ? true : false);
                    cooldownToNextButtonIncrement = cursorMoveCooldown;
                }
            }
        }

        public override void OnEnabled()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out CaseCompletionContainer caseCompletionContainer))
                maxCaseIdx = caseCompletionContainer.GetCurrentProgression();

            foreach (Transform child in caseImagesRoot)
                Destroy(child.gameObject);

            for (int i = 0; i <= maxCaseIdx && i < casesInfo.Length; i++)
            {
                Instantiate(caseImagePrefab, caseImagesRoot).sprite = casesInfo[i].image;
            }

            caseImagesRoot.anchoredPosition = new Vector2(0, caseImagesRoot.anchoredPosition.y);

            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;

            SetCurrentCase(0, false);
        }

        public override void OnDisabled()
        {
            if (inPopup)
                CloseConfirmPopup();
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        /// <summary>
        /// Sets the current case
        /// </summary>
        /// <param name="index">The current case's index</param>
        public void SetCurrentCase(int index, bool canPlaySFX = true)
        {
            if (audioManager != null && canPlaySFX)
                audioManager.PlayUICursorMoveSFX();

            currentCaseIdx = index;

            leftArrow.gameObject.SetActive(currentCaseIdx > 0);
            rightArrow.gameObject.SetActive(currentCaseIdx < maxCaseIdx && currentCaseIdx < casesInfo.Length - 1);

            caseNameText.SetNewKey(casesInfo[index].nameKey);
            caseImagesRoot.DOAnchorPosX(-index * caseImagePrefab.GetComponent<RectTransform>().sizeDelta.x, 0.5f).SetEase(Ease.OutQuad);

            if (inPopup)
                CloseConfirmPopup();
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (inPopup)
                    ConfirmCurrentPopupButton();
                else
                    OpenConfirmPopup();
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
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide = value.x < 0 ? 1 : -1;

                        if (inPopup)
                            ChangePopupButton(!onConfirmButton);
                        else if (currentButtonInputSide != 0)
                            IncrementCaseWithButton(currentButtonInputSide < 0 ? true : false);
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
		/// Increments the current button with visual buttons
		/// </summary>
        public void IncrementCaseWithButton(bool isLeft)
        {
            if (!isLeft && currentCaseIdx > 0)
                SetCurrentCase(currentCaseIdx - 1);
            else if (isLeft && currentCaseIdx < maxCaseIdx && currentCaseIdx < casesInfo.Length - 1)
                SetCurrentCase(currentCaseIdx + 1);
        }

        /// <summary>
        /// Opens the confirm popup
        /// </summary>
        public void OpenConfirmPopup()
        {
            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;
            inPopup = true;

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

            currentButtonInputSide = 0;
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

                PersistentDataManager.instance.GetPlayerData().ResetAll();
                if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer playerVariableContainer))
                    playerVariableContainer.SetPlayerName("Rann");

                if (PersistentDataManager.instance.GetGlobalData().GetComponent(out LoadStateContainer loadStateContainer))
                    loadStateContainer.SetToLoadScript(casesInfo[currentCaseIdx].entrypoint);

                manager.ChangeScene(PersistentDataManager.instance.GetANFSettings().gameScene);
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

        public override bool OnChangeScene()
        {
            OnUnRegisterInputs();
            return true;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return false;
        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }
    }

    /// <summary>
    /// Represents a case selection's info
    /// </summary>
    [System.Serializable]
    public struct CaseInfo
    {
        public string nameKey;
        public Sprite image;
        public string entrypoint;
    }
}

