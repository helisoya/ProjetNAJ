using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the choice UI.
    /// The player can select an option, and load the linked script
    /// </summary>
    public class ChoiceUI : GUIComponent
    {
        [Header("List")]
        [SerializeField] private Transform listButtonsRoot;
        [SerializeField] private ChoiceUIButton listButtonPrefab;

        [Header("Circle Around Image")]
        [SerializeField] private Transform circleAroundImageButtonsRoot;
        [SerializeField] private Image circleAroundImage;
        [SerializeField] private Transform circleAroundImageRoot;
        [SerializeField] private ChoiceUIButton circleAroundImageButtonPrefab;
        [SerializeField] private float circleSize = 75.0f;

        private ChoiceUIButton[] buttons;
        private Sprite[] circleAroundImageCache;

        private AudioManager audioManager;
        private ChoiceData currentData;
        private int currentButtonIndex;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement;
        private float cursorMoveCooldown;

        public bool showingChoice { get; private set; } = false;
        public uint selectedLine { get; private set; } = 0;

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent<Persistent.AudioManager>(out audioManager);

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
                    IncrementButtonWithInput();
                    cooldownToNextButtonIncrement = cursorMoveCooldown;
                }
            }
        }

        /// <summary>
        /// Changes if the component is enabled or not.
        /// Also sets the currently displayed choice
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        /// <param name="choiceData">The choice's data</param>
        public void SetEnabled(bool enabled, ChoiceData choiceData)
        {
            if (enabled && !isEnabled)
            {
                currentData = choiceData;
                showingChoice = true;
                selectedLine = 0;
                currentButtonIndex = 0;
            }

            SetEnabled(enabled);
        }

        public override void OnEnabled()
        {
            currentButtonInputSide = 0;
            cooldownToNextButtonIncrement = 0;

            foreach (Transform child in listButtonsRoot)
            {
                child.DOKill(false);
                Destroy(child.gameObject);
            }

            foreach (Transform child in circleAroundImageButtonsRoot)
            {
                child.DOKill(false);
                Destroy(child.gameObject);
            }

            buttons = new ChoiceUIButton[currentData.entries.Length];

            if (currentData.type == ChoiceData.ChoiceType.AroundImage)
                circleAroundImageCache = new Sprite[currentData.entries.Length];
            else
                circleAroundImageCache = null;

            ChoiceUIButton prefab = currentData.type == ChoiceData.ChoiceType.List ? listButtonPrefab : circleAroundImageButtonPrefab;
            Transform prefabRoot = currentData.type == ChoiceData.ChoiceType.List ? listButtonsRoot : circleAroundImageButtonsRoot;
            float circleStep = Mathf.PI * 2.0f / buttons.Length;

            for (int i = 0; i < buttons.Length; i++)
            {
                ChoiceUIButton button = Instantiate(prefab, prefabRoot);
                button.Initialize(i, currentData.entries[i].textKey, this);
                buttons[i] = button;

                if (currentData.type == ChoiceData.ChoiceType.AroundImage)
                {
                    button.RebuildMesh();
                    float sizeX = button.GetSize().x;
                    circleAroundImageCache[i] = ANFUtils.LoadSprite("Choices/", currentData.entries[i].linkedSprite, currentData.entries[i].linkedSpritesheet);
                    Vector2 position = new Vector2(
                        Mathf.Cos(i * circleStep) * circleSize,
                        Mathf.Sin(i * circleStep) * circleSize
                        );

                    if (position.x >= circleSize * 0.5f)
                        position.x += sizeX / 2.0f;
                    else if (position.x <= -circleSize * 0.5f)
                        position.x -= sizeX / 2.0f;

                    button.GetComponent<RectTransform>().anchoredPosition = position;
                }
            }

            if (buttons.Length != 0)
            {
                buttons[0].OnEnter();
                if (currentData.type == ChoiceData.ChoiceType.AroundImage)
                {
                    circleAroundImage.sprite = circleAroundImageCache[0];
                    circleAroundImageRoot.gameObject.SetActive(true);
                    circleAroundImageRoot.transform.localScale = Vector3.zero;
                    circleAroundImageRoot.transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
                }
                else
                {
                    circleAroundImageRoot.gameObject.SetActive(false);
                }
            }
        }

        public override void OnDisabled()
        {
            if (currentData.type == ChoiceData.ChoiceType.AroundImage)
                circleAroundImageRoot.transform.DOScale(0.0f, 0.5f).SetEase(Ease.OutQuad);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == currentButtonIndex)
                    buttons[i].Fade(0.5f, () =>
                    {
                        circleAroundImageRoot.gameObject.SetActive(false);
                        showingChoice = false;
                        foreach (Transform child in listButtonsRoot)
                        {
                            child.DOKill(false);
                            Destroy(child.gameObject);
                        }
                        foreach (Transform child in circleAroundImageButtonsRoot)
                        {
                            child.DOKill(false);
                            Destroy(child.gameObject);
                        }
                    });
                else
                    buttons[i].Fade(0.0f, null);
            }
        }

        public override void OnPaused()
        {
            cooldownToNextButtonIncrement = 0.0f;
            currentButtonInputSide = 0;
        }

        public override void OnUnPaused()
        {
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

        /// <summary>
        /// Selects a choice and closes the menu
        /// </summary>
        /// <param name="choiceIndex">The choice's index</param>
        public void SelectChoice(int choiceIndex)
        {
            if (showingChoice && isEnabled && !isPaused)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();

                selectedLine = currentData.entries[choiceIndex].linkedLine;
                SetEnabled(false);
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && showingChoice && context.ReadValueAsButton())
                SelectChoice(currentButtonIndex);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && showingChoice)
            {
                Vector2 input = context.ReadValue<Vector2>();
                float value = input.x == 0 ? input.y : input.x;

                if (Mathf.Abs(value) >= 0.9f)
                {
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
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
            if (!isEnabled || isPaused)
                return;

            if (force || currentButtonIndex != id)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                buttons[currentButtonIndex].OnExit();
                currentButtonIndex = id;
                buttons[currentButtonIndex].OnEnter();

                if (currentData.type == ChoiceData.ChoiceType.AroundImage && circleAroundImageCache != null)
                {
                    circleAroundImage.sprite = circleAroundImageCache[currentButtonIndex];
                    if (circleAroundImageRoot.localScale.x >= 0.999f)
                        circleAroundImageRoot.DOPunchScale(new Vector3(-0.1f, -0.1f, -0.1f), 0.2f);
                }
            }
        }

        /// <summary>
		/// Increments the current button with the keyboard input
		/// </summary>
        private void IncrementButtonWithInput()
        {
            SetCurrentButton((currentButtonIndex + currentButtonInputSide + buttons.Length) % buttons.Length);
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
            json.Add("showingChoice", showingChoice);
            json.Add("selectedLine", selectedLine);

            if (showingChoice)
            {
                JSON choiceDataJson = new JSON();
                JArray choiceEntriesJson = new JArray();
                choiceDataJson.Add("type", (int)currentData.type);

                foreach (ChoiceData.ChoiceDataEntry entry in currentData.entries)
                {
                    JSON entryJson = new JSON();
                    entryJson.Add("textKey", entry.textKey);
                    entryJson.Add("linkedScript", entry.linkedLine);

                    if (!string.IsNullOrEmpty(entry.linkedSprite))
                        entryJson.Add("linkedSprite", entry.linkedSprite);

                    if (!string.IsNullOrEmpty(entry.linkedSpritesheet))
                        entryJson.Add("linkedSpritesheet", entry.linkedSpritesheet);

                    choiceEntriesJson.Add(entryJson);
                }

                choiceDataJson.Add("entries", choiceEntriesJson);

                json.Add("choiceData", choiceDataJson);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingChoice"))
                showingChoice = json.GetBool("showingChoice");

            if (json.ContainsKey("selectedLine"))
                selectedLine = json.GetJNumber("selectedLine").AsUInt();

            if (showingChoice && json.ContainsKey("choiceData"))
            {
                JSON choiceData = json.GetJSON("choiceData");

                if (choiceData.ContainsKey("type"))
                    currentData.type = (ChoiceData.ChoiceType)choiceData.GetInt("type");

                if (choiceData.ContainsKey("entries"))
                {
                    JSON[] arrayData = choiceData.GetJArray("entries").AsJSONArray();

                    currentData.entries = new ChoiceData.ChoiceDataEntry[arrayData.Length];
                    for (int i = 0; i < arrayData.Length; i++)
                    {
                        if (arrayData[i].ContainsKey("textKey"))
                            currentData.entries[i].textKey = arrayData[i].GetString("textKey");

                        if (arrayData[i].ContainsKey("linkedLine"))
                            currentData.entries[i].linkedLine = arrayData[i].GetJNumber("linkedLine").AsUInt();

                        if (arrayData[i].ContainsKey("linkedSprite"))
                            currentData.entries[i].linkedSprite = arrayData[i].GetString("linkedSprite");

                        if (arrayData[i].ContainsKey("linkedSpritesheet"))
                            currentData.entries[i].linkedSpritesheet = arrayData[i].GetString("linkedSpritesheet");
                    }
                }

                isEnabled = false;
                SetEnabled(true, currentData);

                if (json.ContainsKey("isEnabled"))
                    json.Remove("isEnabled");
            }
        }
    }

    /// <summary>
    /// Represents a choice's data
    /// </summary>
    public struct ChoiceData
    {
        public ChoiceType type;
        public ChoiceDataEntry[] entries;

        /// <summary>
        /// Represents 
        /// </summary>
        public enum ChoiceType
        {
            // Choices are displayed in a list
            List,
            // Choices are display around an image
            AroundImage
        }

        /// <summary>
        /// Represents an entry (button) in the choice
        /// </summary>
        public struct ChoiceDataEntry
        {
            public string textKey;
            public uint linkedLine;
            public string linkedSprite;
            public string linkedSpritesheet;
        }
    }
}

