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
    /// Represents the logs menu
    /// </summary>
    public class LogsMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Log Buttons")]
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private LogsMenuUIButton buttonPrefab;
        [SerializeField] private Scrollbar buttonsScrollbar;

        [Header("Log Info")]
        [SerializeField] private Locals.LocalizedText logNameText;
        [SerializeField] private Locals.LocalizedText logDescText;
        [SerializeField] private Image logSpriteImage;
        [SerializeField] private Sprite defaultLogSprite;

        [Header("New Log Popup")]
        [SerializeField] private RectTransform popupTransform;

        private Persistent.AudioManager audioManager;
        private LogsContainer logsContainer;

        private int currentButtonIdx;
        private List<LogsMenuUIButton> buttons;
        private Vector2Int currentButtonInputSide = Vector2Int.zero;
        private float cooldownToNextButtonIncrement = 0;

        public override void OnInitialize()
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
            popupTransform.anchoredPosition = new Vector2(popupTransform.sizeDelta.x / 2f, -popupTransform.sizeDelta.y / 2.0f);
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
            PersistentDataManager.instance.GetPlayerData().GetComponent<LogsContainer>(out logsContainer);
        }

        public override void OnUpdate()
        {
            if (currentButtonInputSide.x != 0 || currentButtonInputSide.y != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.5f;
                }
            }
        }

        public override void OnEnabled()
        {
            foreach (Transform child in buttonsRoot)
                Destroy(child.gameObject);

            buttons = new();

            List<string> allLogs = new List<string>(logsContainer.GetAllLogs());
            PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out Locals.Locals locals);

            allLogs.Sort((string o1, string o2) =>
            {
                if (locals != null)
                    return locals.GetLocal($"Log_{o1}_name").CompareTo(locals.GetLocal($"Log_{o2}_name"));
                return o1.CompareTo(o2);
            });

            for (int i = 0; i < allLogs.Count; i++)
            {
                LogsMenuUIButton button = Instantiate(buttonPrefab, buttonsRoot);
                button.Initialize(i, this, allLogs[i], logsContainer.IsUnlocked(allLogs[i]));
                buttons.Add(button);
            }

            float halfSizeButtonsRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            currentButtonInputSide.x = 0;
            currentButtonInputSide.y = 0;
            cooldownToNextButtonIncrement = 0;

            if (allLogs.Count > 0)
            {
                currentButtonIdx = 0;
                buttons[currentButtonIdx].OnEnter();
            }

            logNameText.SetNewKey("GeneralMenu_Unknown");
            logDescText.SetNewKey("GeneralMenu_Unknown");
            logSpriteImage.sprite = defaultLogSprite;

            buttonsScrollbar.value = 1.0f;
        }

        public override void OnDisabled()
        {
            EventSystem.current.SetSelectedGameObject(null);
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
            if (isEnabled && buttons.Count != 0 && !isPaused && context.ReadValueAsButton())
            {
                if (buttons[currentButtonIdx].IsUnlocked())
                    ShowLog(buttons[currentButtonIdx].GetData());
            }
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                SetEnabled(false);
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
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.y = value.y < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
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
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                buttons[currentButtonIdx].OnExit();
                currentButtonIdx = id;
                buttons[currentButtonIdx].OnEnter();

                buttonsScrollbar.value = 1.0f - currentButtonIdx / ((float)buttons.Count - 1);
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            SetCurrentButton((currentButtonIdx + currentButtonInputSide.y + buttons.Count) % buttons.Count);
        }

        /// <summary>
        /// Shows a log's infos on screen
        /// </summary>
        /// <param name="logId">The log's ID</param>
        public void ShowLog(string logId)
        {
            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();

            logNameText.SetNewKey($"Log_{logId}_name");
            logDescText.SetNewKey($"Log_{logId}_desc");

            Sprite sprite = logsContainer.GetLogSprite(logId);

            if (sprite == null)
                sprite = defaultLogSprite;

            logSpriteImage.sprite = sprite;
        }

        public void ShowNewLogPopup()
        {
            popupTransform.DOComplete(false);
            popupTransform.anchoredPosition = new Vector2(popupTransform.sizeDelta.x / 2f, -popupTransform.sizeDelta.y / 2.0f);

            popupTransform.DOAnchorPosX(-popupTransform.sizeDelta.x / 2.0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                popupTransform.DOAnchorPosX(popupTransform.sizeDelta.x / 2.0f, 0.5f).SetEase(Ease.OutQuad).SetDelay(4.0f);
            });
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
}

