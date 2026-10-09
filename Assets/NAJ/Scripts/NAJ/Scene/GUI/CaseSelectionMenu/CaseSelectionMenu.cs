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
        public enum SelectionMode
        {
            Case,
            EntryPoint,
            Confirm
        }


        [Header("Data")]
        [SerializeField] private bool entrypointSelectionByDefault = false;
        [SerializeField] private CaseInfo[] casesInfo;

        [Header("Case Selection")]
        [SerializeField] private CanvasGroup caseSelectionRoot;
        [SerializeField] private LocalizedText caseNameText;
        [SerializeField] private CaseSelectionArrow leftArrow;
        [SerializeField] private CaseSelectionArrow rightArrow;
        [SerializeField] private RectTransform caseImagesRoot;
        [SerializeField] private Image caseImagePrefab;

        [Header("Entry Points Selection")]
        [SerializeField] private CanvasGroup entrypointsPopupRoot;
        [SerializeField] private RectTransform entrypointsRoot;
        [SerializeField] private RectTransform entrypointsMask;
        [SerializeField] private CaseSelectionEntryPointButton entrypointButtonPrefab;
        [SerializeField] private RectTransform entrypointPopupCancelButton;


        [Header("Confirm Popup")]
        [SerializeField] private LocalizedText entryPointNameText;
        [SerializeField] private RectTransform confirmPopupRoot;
        [SerializeField] private RectTransform confirmPopupCancelButton;
        [SerializeField] private RectTransform confirmPopupAcceptButton;

        private uint maxCaseIdx;
        private int currentCaseIdx;
        private int entrypointIdx;
        private Vector2Int currentButtonInputSide = Vector2Int.zero;
        private float cooldownToNextButtonIncrement = 0;
        private AudioManager audioManager;
        private float cursorMoveCooldown = 0.25f;

        private CaseSelectionEntryPointButton[] entrypointButtons;
        private bool onConfirmButton;
        private SelectionMode selectionMode;
        private string currentEntrypoint;

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
                    if (selectionMode == SelectionMode.Confirm && currentButtonInputSide.x != 0)
                        ChangePopupButton(!onConfirmButton);
                    else if (selectionMode == SelectionMode.Case && currentButtonInputSide.x != 0)
                        IncrementCaseWithButton(currentButtonInputSide.x < 0 ? true : false);
                    else if (selectionMode == SelectionMode.EntryPoint && currentButtonInputSide.y != 0)
                        IncrementEntryPointWithButton(currentButtonInputSide.y < 0 ? true : false);
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

            currentButtonInputSide = Vector2Int.zero;
            cooldownToNextButtonIncrement = 0;

            selectionMode = SelectionMode.Case;

            SetCurrentCase(0, false);
        }

        public override void OnDisabled()
        {
            if (selectionMode == SelectionMode.Confirm)
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

            if (selectionMode == SelectionMode.Confirm)
                CloseConfirmPopup();
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (selectionMode == SelectionMode.Confirm)
                    ConfirmCurrentPopupButton();
                else if (selectionMode == SelectionMode.Case)
                    SelectCurrentCase();
                else if (selectionMode == SelectionMode.EntryPoint)
                {
                    if (onConfirmButton)
                    {
                        OpenConfirmPopup(ref casesInfo[currentCaseIdx].entrypoints[entrypointIdx]);
                    }
                    else
                    {
                        CloseEntryPointPopup();
                    }
                }

            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                if (selectionMode == SelectionMode.Confirm)
                    CloseConfirmPopup();
                else if (selectionMode == SelectionMode.EntryPoint)
                    CloseEntryPointPopup();
                else
                    TriggerDelayedClosing();
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
                        currentButtonInputSide.x = value.x < 0 ? 1 : -1;

                        if (selectionMode == SelectionMode.Confirm)
                            ChangePopupButton(!onConfirmButton);
                        else if (selectionMode == SelectionMode.EntryPoint)
                            ChangeEntryPointButton(!onConfirmButton);
                        else if (selectionMode == SelectionMode.Case && currentButtonInputSide.x != 0)
                            IncrementCaseWithButton(currentButtonInputSide.x < 0 ? true : false);
                    }
                }

                if (Mathf.Abs(value.y) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.y = value.y < 0 ? 1 : -1;

                        if (selectionMode == SelectionMode.EntryPoint && currentButtonInputSide.y != 0)
                            IncrementEntryPointWithButton(currentButtonInputSide.y < 0 ? true : false);
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
        /// Increments the current entry point button with visual buttons
        /// </summary>
        public void IncrementEntryPointWithButton(bool isLeft)
        {
            if (isLeft && entrypointIdx > 0)
                SetCurrentEntryPoint(entrypointIdx - 1);
            else if (!isLeft && entrypointIdx < casesInfo[currentCaseIdx].entrypoints.Length - 1)
                SetCurrentEntryPoint(entrypointIdx + 1);
        }

        /// <summary>
		/// Selects the current case and asks for further user inputs
		/// </summary>
        public void SelectCurrentCase()
        {
            if ((!entrypointSelectionByDefault && currentCaseIdx == maxCaseIdx) || casesInfo[currentCaseIdx].entrypoints.Length == 0)
            {
                OpenConfirmPopup(ref casesInfo[currentCaseIdx].defaultEntrypoint);
            }
            else
            {
                OpenEntryPointPopup(ref casesInfo[currentCaseIdx]);
            }
        }

        public void SelectEntryPoint(int idx)
        {
            SetCurrentEntryPoint(idx, true, true);
            OpenConfirmPopup(ref casesInfo[currentCaseIdx].entrypoints[idx]);
        }

        public void SetCurrentEntryPoint(int idx, bool moveRoot = true, bool canPlaySFX = true)
        {
            if (audioManager != null && canPlaySFX)
                audioManager.PlayUICursorMoveSFX();

            if (!onConfirmButton)
            {
                entrypointIdx = idx;
                ChangeEntryPointButton(true);
            }
            else if (entrypointIdx != idx)
            {
                entrypointButtons[idx].Highlight();
                entrypointButtons[entrypointIdx].UnHighlight();

                float positionY = (entrypointButtonPrefab.GetComponent<RectTransform>().sizeDelta.y + 5) * idx;

                if (moveRoot && entrypointsRoot.sizeDelta.y > entrypointsMask.sizeDelta.y)
                    entrypointsRoot.anchoredPosition = new Vector2(entrypointsRoot.anchoredPosition.x, positionY);

                entrypointIdx = idx;
            }

            if (selectionMode == SelectionMode.Confirm)
                CloseConfirmPopup();
        }

        /// <summary>
		/// Opens the entry point selection popup
		/// </summary>
		/// <param name="caseInfo">The case's infos</param>
        public void OpenEntryPointPopup(ref CaseInfo caseInfo)
        {
            caseSelectionRoot.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
            caseSelectionRoot.blocksRaycasts = false;

            foreach (Transform child in entrypointsRoot)
                Destroy(child.gameObject);

            entrypointButtons = new CaseSelectionEntryPointButton[caseInfo.entrypoints.Length];

            for (int i = 0; i < caseInfo.entrypoints.Length; i++)
            {
                entrypointButtons[i] = Instantiate(entrypointButtonPrefab, entrypointsRoot);
                entrypointButtons[i].Initialize(i, caseInfo.entrypoints[i].nameKey, this);
            }

            entrypointsPopupRoot.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
            entrypointsPopupRoot.blocksRaycasts = true;

            currentButtonInputSide = Vector2Int.zero;
            cooldownToNextButtonIncrement = 0;
            selectionMode = SelectionMode.EntryPoint;

            onConfirmButton = true;
            entrypointIdx = 0;

            entrypointButtons[0].Highlight();
            entrypointPopupCancelButton.DOComplete();
            entrypointPopupCancelButton.localScale = Vector3.one;
            entrypointPopupCancelButton.GetComponent<Image>().color = Color.white;

            entrypointsRoot.anchoredPosition = new Vector2(entrypointsRoot.anchoredPosition.x, 0);
        }

        /// <summary>
		/// Closes the entry point selection popup
		/// </summary>
        public void CloseEntryPointPopup()
        {
            currentButtonInputSide = Vector2Int.zero;
            cooldownToNextButtonIncrement = 0;

            entrypointsPopupRoot.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
            entrypointsPopupRoot.blocksRaycasts = false;

            caseSelectionRoot.blocksRaycasts = true;
            caseSelectionRoot.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
            selectionMode = SelectionMode.Case;
        }

        /// <summary>
        /// Opens the confirm popup
        /// </summary>
        public void OpenConfirmPopup(ref CaseEntryPoint entryPoint)
        {
            if ((!entrypointSelectionByDefault && currentCaseIdx == maxCaseIdx) ||
                casesInfo[currentCaseIdx].entrypoints.Length == 0)
            {
                caseSelectionRoot.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
                caseSelectionRoot.blocksRaycasts = false;
            }


            currentButtonInputSide = Vector2Int.zero;
            cooldownToNextButtonIncrement = 0;
            selectionMode = SelectionMode.Confirm;

            entryPointNameText.SetNewKey(entryPoint.nameKey);
            currentEntrypoint = entryPoint.entryPoint;

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
            currentButtonInputSide = Vector2Int.zero;
            cooldownToNextButtonIncrement = 0;

            confirmPopupRoot.DOScale(Vector3.zero, 0.75f).SetEase(Ease.InBack);

            if ((!entrypointSelectionByDefault && currentCaseIdx == maxCaseIdx) ||
                casesInfo[currentCaseIdx].entrypoints.Length == 0)
            {
                caseSelectionRoot.blocksRaycasts = true;
                caseSelectionRoot.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
                selectionMode = SelectionMode.Case;
            }
            else
            {
                onConfirmButton = true;
                selectionMode = SelectionMode.EntryPoint;
            }
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
                    loadStateContainer.SetToLoadScript(currentEntrypoint);

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

        /// <summary>
        /// Changes the currently selected entry point button (entry point or cancel)
        /// </summary>
        /// <param name="onConfirmButton">True if the user is on the entry point button</param>
        public void ChangeEntryPointButton(bool onConfirmButton)
        {
            if (onConfirmButton != this.onConfirmButton)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                this.onConfirmButton = onConfirmButton;

                if (onConfirmButton)
                {
                    entrypointPopupCancelButton.DOScale(Vector3.one * 1.0f, 0.5f).SetEase(Ease.OutBounce);
                    entrypointPopupCancelButton.GetComponent<Image>().DOColor(Color.white * new Vector4(
                    1.0f,
                    1.0f,
                    1.0f,
                    1.0f), 0.5f).SetEase(Ease.OutQuad);
                    entrypointButtons[entrypointIdx].Highlight();
                }
                else
                {
                    entrypointPopupCancelButton.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutBounce);
                    entrypointPopupCancelButton.GetComponent<Image>().DOColor(Color.white * new Vector4(
                    0.9f,
                    1.0f,
                    0.9f,
                    1.0f), 0.5f).SetEase(Ease.OutQuad);
                    entrypointButtons[entrypointIdx].UnHighlight();
                }
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

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
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

    /// <summary>
    /// Represents a case selection's info
    /// </summary>
    [System.Serializable]
    public struct CaseInfo
    {
        public string nameKey;
        public Sprite image;
        public CaseEntryPoint[] entrypoints;
        public CaseEntryPoint defaultEntrypoint;
    }

    /// <summary>
	/// Represents an entrypoint to a case
	/// </summary>
    [System.Serializable]
    public struct CaseEntryPoint
    {
        public string nameKey;
        public string entryPoint;
    }
}

