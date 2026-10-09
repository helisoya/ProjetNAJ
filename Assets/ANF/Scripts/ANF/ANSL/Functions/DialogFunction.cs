using ANF.GUI;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ANF.ANSL
{
    /// <summary>
    /// The Dialog function can be used to show a dialog
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "dialog",
        functionAutoComplete: new string[] {
            "dialog(SpeakerId;CharacterId;DialogId)",
            "dialog(SpeakerId;CharacterId;DialogId;Additive)",
            "dialog(SpeakerId;CharacterId;DialogId;Additive;Type)"
        },
        functionDesc: "Shows a dialog (Type is normal, noEndInput or noInput)")]
    public class DialogFunction : ANSLFunction
    {
        private DialogUI dialogUI;
        private string characterId;
        private AudioManager audioManager;

        private bool inputDetected;
        private bool waitingForEndInput;

        private float currentAutoplayTimer;
        private bool autoPlayEnabled;
        private bool skipModeEnabled;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL,
                    FunctionParameterType.STRING },
            };
        }

        protected override void OnStartProcess()
        {
            currentAutoplayTimer = -1;
            autoPlayEnabled = false;
            skipModeEnabled = false;
            bool noInputs = false;
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            inputDetected = false;
            if (parameters.GetParameter(0, out string speakerId) &&
                parameters.GetParameter(1, out characterId) &&
                parameters.GetParameter(2, out string dialogId) &&
                manager.GetGUIManager().GetComponent<DialogUI>(out dialogUI))
            {
                if (PersistentDataManager.instance.GetPlayerData().GetComponent(out HistoryContainer historyContainer))
                    historyContainer.AddDialog(dialogId, speakerId);

                bool additive;
                bool noEndUserInput = false;
                if (!parameters.GetParameter(3, out additive))
                    additive = false;
                if (parameters.GetParameter(4, out string type))
                {
                    type = type.ToLower();
                    if (type.Equals("normal"))
                    {
                        noInputs = false;
                        noEndUserInput = false;
                    }
                    else if (type.Equals("noendinput"))
                    {
                        noInputs = false;
                        noEndUserInput = true;
                    }
                    else if (type.Equals("noinput"))
                    {
                        noInputs = true;
                        noEndUserInput = false;
                    }
                }
                else
                {
                    noEndUserInput = false;
                }

                waitingForEndInput = !noEndUserInput;

                if (!noInputs)
                {
                    dialogUI.GetSkipButton().onClick.AddListener(OnDialogSkip);
                    PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnDialogSkip;
                }

                dialogUI.StartDialog(speakerId, dialogId, characterId, additive);
                dialogUI.SetEnabled(true);

                if (noInputs)
                {
                    EndProcess();
                    return;
                }
            }
            else
            {
                // Parsing error and/or no dialogUI
                EndProcess();
            }
        }

        protected override void OnUpdate()
        {
            if (dialogUI == null)
            {
                manager.GetGUIManager().GetComponent(out dialogUI);
                dialogUI.GetSkipButton().onClick.AddListener(OnDialogSkip);
                PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnDialogSkip;
            }

            if (!dialogUI.showingDialog)
            {
                if (characterId != null &&
                    manager.GetWorld().GetComponent(out Scene.CharacterManager characterManager))
                {
                    if (characterManager.GetSceneObject(characterId, out Scene.Character character))
                        character.SetIsTalking(false);
                }

                if (autoPlayEnabled || skipModeEnabled)
                {
                    if (currentAutoplayTimer == -1f)
                    {
                        float autoPlayTimer = 4.0f;
                        if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settingsContainer))
                            autoPlayTimer = (float)settingsContainer.GetValue("ANSLManager_AutoplayTimer", SettingsContainer.SettingsDataType.Float);

                        currentAutoplayTimer = skipModeEnabled ? 0.05f : (autoPlayEnabled ? autoPlayTimer : 0.0f);
                    }
                    else if (currentAutoplayTimer > 0)
                    {
                        currentAutoplayTimer -= Time.deltaTime;

                        if (currentAutoplayTimer > 0.05f && skipModeEnabled)
                            currentAutoplayTimer = 0.05f;
                    }
                    else
                    {
                        inputDetected = true;
                    }
                }

                if (inputDetected)
                {
                    waitingForEndInput = false;
                    inputDetected = false;
                }

                if (!waitingForEndInput)
                {
                    dialogUI.GetSkipButton().onClick.RemoveListener(OnDialogSkip);
                    dialogUI.HideContinueIcon();
                    PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnDialogSkip;

                    EndProcess();
                }
            }
            else
            {
                if (inputDetected)
                {
                    dialogUI.ToggleCanSkip();
                    inputDetected = false;
                }
            }
        }

        protected override void OnCleanup()
        {
            dialogUI = null;
            audioManager = null;
        }

        private void OnDialogSkip()
        {
            if (!context.isPaused && !skipModeEnabled && !autoPlayEnabled)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                inputDetected = true;
            }

        }

        private void OnDialogSkip(InputAction.CallbackContext callbackContext)
        {
            if (!context.isPaused && callbackContext.ReadValueAsButton())
            {
                OnDialogSkip();
            }
        }

        public void OnAutoPlayToggle(bool enabled)
        {
            autoPlayEnabled = enabled;
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForEndInput", waitingForEndInput);
            json.Add("characterId", characterId);
        }

        protected override void OnLoad(JSON json)
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            if (json.ContainsKey("waitingForEndInput"))
                waitingForEndInput = json.GetBool("waitingForEndInput");

            if (json.ContainsKey("characterId"))
                characterId = json.GetString("characterId");
        }
    }
}

