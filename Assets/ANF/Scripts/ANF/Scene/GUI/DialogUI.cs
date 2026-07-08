using ANF.Persistent;
using ANF.Scene;
using DG.Tweening;
using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents the component responsible for showing dialogs.
    /// Dialogs can comprise commands.
    /// Ex : I want [wait 5,speed 0.5] A CARIBOU [defaultSpeed] tomorrow
	/// </summary>
    [System.Serializable]
    public class DialogUI : GUIComponent
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject speakerRoot;
        [SerializeField] private Image[] dialogBackgrounds;
        [SerializeField] private Locals.LocalizedText speakerText;
        [SerializeField] private Locals.LocalizedText dialogText;
        [SerializeField] private Button continueButton;

        [Header("Infos")]
        [SerializeField] private float punctuationSpeedFactor = 3;

        private List<string> textIds = new List<string>();
        private List<DialogSegment> textSegments;
        private string currentSpeakerID;
        private bool canSkip;
        private int revealIndex;
        private int currentSegmentIdx;
        private float currentWaitTime;

        private float secondsBetweenCharacters;
        private float defaultSecondsBetweenCharacters;

        public bool showingDialog { get; private set; }

        private char[] punctuations = { '.', ',', '?', '!', ';', ':' };
        private bool skipModeEnabled;

        public override void OnInitialize()
        {
            skipModeEnabled = false;

            textIds = new List<string>();

            speakerText.SetCanReload(true, false);
            dialogText.SetLocalizationEnabled(false, false);
            dialogText.SetCanReload(false, false);

            canvasGroup.alpha = 0;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                OnBackgroundOpacityChange(settings.Register("DialogUI_BackgroundOpacity", SettingsContainer.SettingsDataType.Float, OnBackgroundOpacityChange));
        }

        public override void OnStart()
        {
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
        }

        private void OnBackgroundOpacityChange(object obj)
        {
            float opacity = (float)obj;
            foreach (Image background in dialogBackgrounds)
                background.color = new Color(background.color.r, background.color.g, background.color.b, opacity);
        }

        /// <summary>
        /// Toggles the can skip flag (Skips the dialog typewriter, and )
        /// </summary>
        public void ToggleCanSkip()
        {
            canSkip = true;
        }

        /// <summary>
        /// Starts a new dialog
        /// </summary>
        /// <param name="speakerID">The speaker's ID. null to hide the speaker text</param>
        /// <param name="dialogID">The dialog's ID</param>
        /// <param name="additive">True if the text is additive</param>
        /// <param name="secondsBetweenCharacters">The number of seconds between characters</param>
        public void StartDialog(string speakerID, string dialogID, bool additive = false, float secondsBetweenCharacters = 0.05f)
        {
            canSkip = false;
            showingDialog = true;
            currentWaitTime = 0;
            this.secondsBetweenCharacters = secondsBetweenCharacters;
            this.defaultSecondsBetweenCharacters = secondsBetweenCharacters;
            currentSpeakerID = speakerID;

            speakerRoot.SetActive(speakerID != null);
            if (speakerID != null)
            {
                speakerText.SetNewKey(speakerID);
            }

            if (dialogID != null)
            {
                if (!additive)
                    textIds.Clear();

                textIds.Add(dialogID);

                RegenerateDialogFromStack(false);
            }
        }

        /// <summary>
        /// Refreshs the dialog due to changes in languages
        /// </summary>
        public void RefreshDialogLocals()
        {
            RegenerateDialogFromStack(true);
        }

        /// <summary>
		/// Regenerates the dialog from the text stack and recompute the reveal Index
		/// </summary>
        /// <param name="lastTextCompleted">True if the last text was completed</param>
        private void RegenerateDialogFromStack(bool lastTextCompleted)
        {
            textSegments = new List<DialogSegment>();
            DialogSegment currentSegment;

            PersistentDataManager.instance.GetGlobalData().GetComponent<Locals.Locals>(out Locals.Locals locals);
            PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer playerVariables);

            string result = "";
            string tmp;
            currentSegmentIdx = 0;
            for (int i = 0; i < textIds.Count; i++)
            {
                tmp = locals != null ? locals.GetLocal(textIds[i]) : textIds[i];

                string[] split = tmp.Split(new char[] { '[', ']' });

                for (int j = 0; j < split.Length; j += 2)
                {
                    if (playerVariables != null)
                        split[j] = split[j].Replace("{MC}", playerVariables.GetPlayerName());

                    currentSegment = new DialogSegment
                    {
                        dialogText = split[j],
                        commands = j < split.Length - 1 ? split[j + 1].Split(',') : null
                    };
                    textSegments.Add(currentSegment);

                    if (lastTextCompleted || textIds.Count - 1 > i)
                    {
                        currentSegmentIdx++;
                        result += currentSegment.dialogText;
                    }
                }
            }

            dialogText.GetText().text = result;
            dialogText.GetText().ForceMeshUpdate(true);

            revealIndex = dialogText.GetText().textInfo.characterCount;
            dialogText.GetText().maxVisibleCharacters = revealIndex;
        }

        public override void OnUpdate()
        {
            if (showingDialog)
            {
                TMP_Text text = dialogText.GetText();

                if (skipModeEnabled)
                {
                    revealIndex = text.textInfo.characterCount;
                    text.maxVisibleCharacters = revealIndex;
                    currentWaitTime = 0;
                }

                bool stillCharactersToReveal = revealIndex < text.textInfo.characterCount;

                if (!stillCharactersToReveal && currentSegmentIdx < textSegments.Count)
                {
                    if (currentSegmentIdx != 0 && textSegments[currentSegmentIdx - 1].commands != null)
                    {
                        foreach (string command in textSegments[currentSegmentIdx - 1].commands)
                        {
                            ProcessCommand(command);
                        }
                    }

                    text.text += textSegments[currentSegmentIdx].dialogText;

                    text.ForceMeshUpdate(true);
                    stillCharactersToReveal = true;
                    currentSegmentIdx++;
                }

                if (canSkip || !stillCharactersToReveal)
                {
                    // Skip typewriter

                    for (int i = currentSegmentIdx; i < textSegments.Count; i++)
                    {
                        text.text += textSegments[i].dialogText;
                    }
                    text.ForceMeshUpdate(true);
                    revealIndex = text.textInfo.characterCount;
                    text.maxVisibleCharacters = revealIndex;
                    currentSegmentIdx = textSegments.Count;
                    showingDialog = false;
                    canSkip = false;
                }
                else if (stillCharactersToReveal)
                {
                    if (currentWaitTime <= 0)
                    {
                        revealIndex++;
                        currentWaitTime = secondsBetweenCharacters *
                            (IsPunctuation(text.textInfo.characterInfo[revealIndex - 1].character) ? punctuationSpeedFactor : 1.0f);
                        text.maxVisibleCharacters = revealIndex;
                    }
                    else
                    {
                        currentWaitTime -= Time.deltaTime;
                    }
                }
            }
        }

        /// <summary>
		/// Checks if a character is punctuation or not
		/// </summary>
		/// <param name="character">The character</param>
		/// <returns>True if it is punctuation</returns>
        private bool IsPunctuation(char character)
        {
            foreach (char punctuation in punctuations)
                if (punctuation == character)
                    return true;
            return false;
        }

        public override void OnEnabled()
        {
            OnUnPaused();
        }

        public override void OnDisabled()
        {
            OnPaused();
        }

        public override void OnPaused()
        {
            canvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnSave(JSON json)
        {
            json.Add("showingDialog", showingDialog);
            json.Add("secondsBetweenCharacters", secondsBetweenCharacters);
            json.Add("defaultSecondsBetweenCharacters", defaultSecondsBetweenCharacters);
            json.Add("currentWaitTime", currentWaitTime);
            json.Add("revealIndex", revealIndex);
            json.Add("textIds", textIds);
            if (currentSpeakerID != null)
                json.Add("currentSpeakerID", currentSpeakerID);
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingDialog"))
                showingDialog = json.GetBool("showingDialog");
            if (json.ContainsKey("secondsBetweenCharacters"))
                secondsBetweenCharacters = json.GetFloat("secondsBetweenCharacters");
            if (json.ContainsKey("defaultSecondsBetweenCharacters"))
                defaultSecondsBetweenCharacters = json.GetFloat("defaultSecondsBetweenCharacters");
            if (json.ContainsKey("currentWaitTime"))
                currentWaitTime = json.GetFloat("currentWaitTime");
            if (json.ContainsKey("revealIndex"))
                revealIndex = json.GetInt("revealIndex");
            if (json.ContainsKey("textIds"))
                textIds = new List<string>(json.GetJArray("textIds").AsStringArray());
            if (json.ContainsKey("currentSpeakerID"))
            {
                currentSpeakerID = json.GetString("currentSpeakerID");
                speakerText.SetNewKey(currentSpeakerID);
            }
            else
            {
                currentSpeakerID = null;
            }

            speakerRoot.SetActive(currentSpeakerID != null);

            secondsBetweenCharacters = defaultSecondsBetweenCharacters;

            RegenerateDialogFromStack(!showingDialog);
        }

        /// <summary>
		/// Gets the skip button
		/// </summary>
		/// <returns>The skip button</returns>
        public Button GetSkipButton()
        {
            return continueButton;
        }

        /// <summary>
        /// Process a command embedded in a dialog
        /// Must use the following synthax : COMMAND PARAM1 PARAM2
        /// </summary>
        /// <param name="command">The command to parse</param>
        private void ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            while (command.Length > 0 && command.StartsWith(' '))
                command = command.Substring(1);
            while (command.Length > 0 && command.EndsWith(' '))
                command = command.Substring(command.Length - 1);

            string[] split = command.Split(' ');

            switch (split[0])
            {
                case "wait":
                    if (split.Length == 2 && float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float waitTime))
                        currentWaitTime = waitTime;
                    break;
                case "speed":
                    if (split.Length == 2 && float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float speed))
                        secondsBetweenCharacters = speed;
                    break;
                case "defaultSpeed":
                    if (split.Length == 1)
                        secondsBetweenCharacters = defaultSecondsBetweenCharacters;
                    break;
            }
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                settings.Unregister("DialogUI_BackgroundOpacity", OnBackgroundOpacityChange);
        }

        /// <summary>
        /// Represents a dialog segment
        /// </summary>
        private struct DialogSegment
        {
            public string dialogText;
            public string[] commands;
        }
    }

}
