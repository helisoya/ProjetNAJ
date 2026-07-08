using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the new game menu located within the main menu
    /// </summary>
    public class NewGameMenu : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("Components")]
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private string defaultPlayerName = "Player";

        private Persistent.AudioManager audioManager;
        private bool selectFirstObject;

        public override void OnInitialize()
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.sizeDelta.x / 2f, 0);
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {
            if (selectFirstObject)
            {
                selectFirstObject = false;
                EventSystem.current.SetSelectedGameObject(nameField.gameObject);
            }
        }


        public override void OnEnabled()
        {
            nameField.text = defaultPlayerName;

            selectFirstObject = true;

            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmName);
        }


        public override void OnDisabled()
        {
            EventSystem.current.SetSelectedGameObject(null);
            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        /// <summary>
        /// Confirms the typed name and starts a new game
        /// </summary>
        private void OnConfirmName()
        {
            if (!string.IsNullOrEmpty(nameField.text) && !string.IsNullOrWhiteSpace(nameField.text))
            {
                PersistentDataManager.instance.GetPlayerData().ResetAll();
                if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer playerVariableContainer))
                    playerVariableContainer.SetPlayerName(nameField.text);

                if (PersistentDataManager.instance.GetGlobalData().GetComponent(out LoadStateContainer loadStateContainer))
                    loadStateContainer.SetToLoadScript(PersistentDataManager.instance.GetANFSettings().startingScript);

                manager.ChangeScene(PersistentDataManager.instance.GetANFSettings().gameScene);
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

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Pause").performed += OnPauseInput;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed += OnPauseInput;
        }

        public override void OnUnRegisterInputs()
        {
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

