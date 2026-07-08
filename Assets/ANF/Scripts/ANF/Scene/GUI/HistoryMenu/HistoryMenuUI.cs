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
    /// Represents the history menu located within the pause menu
    /// </summary>
    public class HistoryMenuUI : GUIComponent
    {
        [Header("Background")]
        [SerializeField] private RectTransform bgTransform;
        [SerializeField] private float transitionDuration = 0.5f;

        [Header("History")]
        [SerializeField] private Transform historyRoot;
        [SerializeField] private Locals.LocalizedText prefabText;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private float noSpeakerSize;

        private AudioManager audioManager;

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
        }


        public override void OnEnabled()
        {
            foreach (Transform child in historyRoot)
                Destroy(child.gameObject);

            List<HistoryData> data = null;
            if (PersistentDataManager.instance.GetPlayerData().GetComponent<HistoryContainer>(out HistoryContainer historyContainer))
                data = historyContainer.GetHistory();

            if (data != null)
            {
                string currentSpeaker = null;
                foreach (HistoryData entry in data)
                {
                    Locals.LocalizedText text;
                    if (entry.speakerKey != currentSpeaker)
                    {
                        // Add Speaker text
                        currentSpeaker = entry.speakerKey;
                        text = Instantiate(prefabText, historyRoot);
                        text.SetLocalizationEnabled(true, false);
                        text.SetCanReload(true, false);
                        text.SetNewKey(currentSpeaker);
                        text.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
                        text.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
                        text.GetText().fontStyle = TMPro.FontStyles.Bold;

                        if (currentSpeaker == null)
                        {
                            text.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                            text.GetComponent<RectTransform>().sizeDelta = new Vector2(text.GetComponent<RectTransform>().sizeDelta.x, noSpeakerSize);
                        }
                    }

                    PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out Locals.Locals locals);
                    PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer playerVariables);

                    string result = "";
                    string tmp = locals != null ? locals.GetLocal(entry.dialogKey) : entry.dialogKey;
                    string[] split = tmp.Split(new char[] { '[', ']' });

                    for (int j = 0; j < split.Length; j += 2)
                        result += playerVariables != null ? split[j].Replace("{MC}", playerVariables.GetPlayerName()) : split[j];

                    // Add Dialog
                    text = Instantiate(prefabText, historyRoot);
                    text.SetLocalizationEnabled(false, false);
                    text.SetCanReload(false, false);
                    text.GetText().text = result;
                    text.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
                    text.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
                }
            }
            scrollbar.value = 1;
            EventSystem.current.SetSelectedGameObject(scrollbar.gameObject);

            float halfSizeRoot = bgTransform.sizeDelta.x / 2f;
            bgTransform.DOAnchorPosX(-halfSizeRoot, transitionDuration).SetEase(Ease.OutQuad);
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

