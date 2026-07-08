using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the save menu
    /// </summary>
    [System.Serializable]
    public class PauseMenuUI : GUIComponent
    {
        [Header("Base UI")]
        [SerializeField] private string[] guiComponentsToPause = { "fadeBg", "fadeFg", "dialog" };
        [Tooltip("True to use the pause menu as a main menu (Enabled by default and cannot be closed")]
        [SerializeField] private bool mainMenuMode = false;
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private RectTransform buttonsRoot;
        [SerializeField] private RectTransform bgRoot;

        [Header("Buttons")]
        [SerializeReference, SubclassSelector(AllowNull = false)] private PauseMenuButtonData[] buttonDatas;
        [SerializeField] private PauseMenuButton buttonPrefab;
        private int currentButtonIdx;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement;
        private PauseMenuButton[] buttons;
        private Persistent.AudioManager audioManager;

        private GUIComponent currentPauseSubmenu;


        public override void OnInitialize()
        {
            currentPauseSubmenu = null;
            bgRoot.anchoredPosition = new Vector2(-bgRoot.sizeDelta.x / 2f, 0);

            buttons = new PauseMenuButton[buttonDatas.Length];
            for (int i = 0; i < buttonDatas.Length; i++)
            {
                buttons[i] = Instantiate(buttonPrefab, buttonsRoot);
                buttons[i].Initialize(i, buttonDatas[i], this, manager);
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Pause").performed += OnPauseInput;

            if (mainMenuMode)
            {
                SetEnabled(true);
            }
        }

        public override void OnUpdate()
        {
            if (currentPauseSubmenu && !currentPauseSubmenu.isEnabled)
                currentPauseSubmenu = null;

            if (currentButtonInputSide != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = 0.5f;
                }
            }
        }

        public override void OnDisabled()
        {
            if (currentPauseSubmenu != null)
                ChangeSubMenu(null);

            float halfSizeButtonsRoot = bgRoot.sizeDelta.x / 2f;
            bgRoot.DOAnchorPosX(-halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsPaused(guiComponentsToPause, false);
            manager.GetWorld().SetPausedAll(false);
            if (gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if (dialog.isEnabled)
                    dialog.SetPaused(false);
            }
        }

        public override void OnEnabled()
        {
            SetCurrentButton(0, true);

            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;

            float halfSizeButtonsRoot = bgRoot.sizeDelta.x / 2f;
            bgRoot.DOAnchorPosX(halfSizeButtonsRoot, transitionDuration).SetEase(Ease.OutQuad);

            gui.SetComponentsPaused(guiComponentsToPause, true);
            manager.GetWorld().SetPausedAll(true);
            if (gui.GetComponent<DialogUI>(out DialogUI dialog))
            {
                if (dialog.isEnabled)
                    dialog.SetPaused(true);
            }
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        public override void OnSave(JSON json)
        {
        }

        public override void OnLoad(JSON json)
        {
        }

        public override void OnChangeScene()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Pause").performed -= OnPauseInput;
            OnUnRegisterInputs();
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled += OnMove;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;
        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (currentPauseSubmenu == null && context.ReadValueAsButton() && !mainMenuMode)
            {
                if (audioManager != null)
                {
                    if (isEnabled)
                        audioManager.PlayUICursorCancelSFX();
                    else
                        audioManager.PlayUICursorConfirmSFX();
                }
                SetEnabled(!isEnabled);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && currentPauseSubmenu == null && context.ReadValueAsButton())
                SelectCurrentButton();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && currentPauseSubmenu == null)
            {
                float value = context.ReadValue<Vector2>().y;

                if (Mathf.Abs(value) >= 0.9f)
                {
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide = value < 0 ? 1 : -1;
                        IncrementButtonWithInput();
                    }
                }
                else
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
            if (currentPauseSubmenu == null && (force || currentButtonIdx != id))
            {
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                buttons[currentButtonIdx].OnExit();
                currentButtonIdx = id;
                buttons[currentButtonIdx].OnEnter();
            }
        }

        /// <summary>
        /// Calls the OnClick event on the selected button
        /// </summary>
        public void SelectCurrentButton()
        {
            if (isEnabled && !isPaused && currentPauseSubmenu == null)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                buttons[currentButtonIdx].OnClick();
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            SetCurrentButton((currentButtonIdx + currentButtonInputSide + buttons.Length) % buttons.Length);
        }

        /// <summary>
        /// Changes the current submenu
        /// </summary>
        /// <param name="component">The new submenu</param>
        public void ChangeSubMenu(GUIComponent component)
        {
            if (currentPauseSubmenu != null)
                currentPauseSubmenu.SetEnabled(false);

            currentPauseSubmenu = component;
            if (currentPauseSubmenu)
            {
                currentButtonInputSide = 0;
                cooldownToNextButtonIncrement = 0;
                currentPauseSubmenu.SetEnabled(true);
            }

        }
    }
}
