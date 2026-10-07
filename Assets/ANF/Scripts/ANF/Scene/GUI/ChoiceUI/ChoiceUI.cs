using System.Collections.Generic;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using Unity.VisualScripting;
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
        [Header("Icon")]
        [SerializeField] private Image iconImage;
        [SerializeField] private RectTransform iconRoot;
        private bool iconVisible = false;

        [Header("List")]
        [SerializeField] private RectTransform listButtonsRoot;
        [SerializeField] private ChoiceUIButton listButtonPrefab;
        [SerializeField] private float listIconSize = 75.0f;
        [SerializeField] private float listIconSpacing = 200.0f;

        [Header("Label")]
        [SerializeField] private RectTransform labelButtonsRoot;
        [SerializeField] private ChoiceUIButton labelButtonPrefab;
        [SerializeField] private float circleIconSize = 75.0f;
        [SerializeField] private float arcIconSize = 75.0f;

        private ChoiceUIButton[] buttons;
        private Sprite[] iconsCache;
        private KeyValuePair<string, string>[] iconsCacheNames;

        private AudioManager audioManager;
        private ResourceManager resourceManager;
        private ChoiceData currentData;
        private int currentButtonIndex;
        private int currentButtonInputSide;
        private float cooldownToNextButtonIncrement;
        private float cursorMoveCooldown;

        public bool showingChoice { get; private set; } = false;
        public uint selectedLine { get; private set; } = 0;

        /// <summary>
        /// Shows the icon
        /// </summary>
        public void ShowIcon()
        {
            if (!iconVisible)
            {
                iconRoot.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
                iconVisible = true;
            }
        }

        /// <summary>
        /// Hides the icon
        /// </summary>
        public void HideIcon()
        {
            if (iconVisible)
            {
                iconRoot.DOScale(0.0f, 0.5f).SetEase(Ease.OutQuad);
                iconVisible = false;
            }
        }

        /// <summary>
        /// Sets the icon's sprite. Hides/Shows the icon if needed
        /// </summary>
        /// <param name="sprite">The new icon's sprite</param>
        public void SetIconSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                if (iconVisible)
                    HideIcon();
                return;
            }

            iconImage.sprite = sprite;
            if (!iconVisible)
                ShowIcon();
            else if (iconRoot.localScale.x >= 0.999f)
                iconRoot.DOPunchScale(new Vector3(-0.1f, -0.1f, -0.1f), 0.2f);
        }

        /// <summary>
        /// Changes the icon's position
        /// </summary>
        /// <param name="position">The new position</param>
        /// <param name="immediate">True if the change should be immediate</param>
        public void SetIconPosition(Vector3 position, bool immediate)
        {
            if (immediate)
                iconRoot.position = position;
            else
                iconRoot.DOMove(position, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnInitialize()
        {
            iconRoot.localScale = Vector3.zero;
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
            PersistentDataManager.instance.GetPlayerData().GetComponent(out resourceManager);

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

            foreach (Transform child in labelButtonsRoot)
            {
                child.DOKill(false);
                Destroy(child.gameObject);
            }

            buttons = new ChoiceUIButton[currentData.entries.Length];
            iconsCache = new Sprite[currentData.entries.Length];
            iconsCacheNames = new KeyValuePair<string, string>[currentData.entries.Length];

            ChoiceUIButton prefab = currentData.type == ChoiceData.ChoiceType.List ? listButtonPrefab : labelButtonPrefab;
            Transform prefabRoot = currentData.type == ChoiceData.ChoiceType.List ? listButtonsRoot : labelButtonsRoot;
            float circleStep = Mathf.PI * 2.0f / buttons.Length;
            float arcStep = Mathf.PI / Mathf.Max(1, buttons.Length - 1);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (resourceManager != null && !string.IsNullOrEmpty(currentData.entries[i].linkedSprite))
                {
                    iconsCacheNames[i] = new KeyValuePair<string, string>(currentData.entries[i].linkedSprite, currentData.entries[i].linkedSpritesheet);

                    if (string.IsNullOrEmpty(iconsCacheNames[i].Value))
                        iconsCache[i] = resourceManager.GetResource<Sprite>($"Choices/{iconsCacheNames[i].Key}");
                    else
                        iconsCache[i] = resourceManager.GetSpritesheetResource<Sprite>($"Choices/{iconsCacheNames[i].Value}", iconsCacheNames[i].Key);
                }

                ChoiceUIButton button = Instantiate(prefab, prefabRoot);
                button.Initialize(i, currentData.entries[i].textKey, this);
                buttons[i] = button;

                if (currentData.type == ChoiceData.ChoiceType.Circle)
                {
                    button.RebuildMesh();
                    float sizeX = button.GetSize().x;
                    Vector2 position = new Vector2(
                        Mathf.Cos(i * circleStep) * circleIconSize,
                        Mathf.Sin(i * circleStep) * circleIconSize
                        );

                    if (position.x >= circleIconSize * 0.5f)
                        position.x += sizeX / 2.0f;
                    else if (position.x <= -circleIconSize * 0.5f)
                        position.x -= sizeX / 2.0f;

                    button.GetComponent<RectTransform>().anchoredPosition = position;
                }
                else if (currentData.type == ChoiceData.ChoiceType.Arc)
                {
                    button.RebuildMesh();
                    float sizeX = button.GetSize().x;
                    Vector2 position = new Vector2(
                        Mathf.Cos(i * arcStep) * arcIconSize,
                        Mathf.Sin(i * arcStep) * arcIconSize
                        );

                    if (position.x >= arcIconSize * 0.5f)
                        position.x += sizeX / 2.0f;
                    else if (position.x <= -arcIconSize * 0.5f)
                        position.x -= sizeX / 2.0f;

                    button.GetComponent<RectTransform>().anchoredPosition = position;
                }
            }

            if (buttons.Length != 0)
            {
                buttons[0].OnEnter();
                if (currentData.type == ChoiceData.ChoiceType.Circle)
                {
                    iconRoot.sizeDelta = new Vector2(circleIconSize, circleIconSize);
                    SetIconPosition(labelButtonsRoot.position, true);
                }
                else if (currentData.type == ChoiceData.ChoiceType.Arc)
                {
                    iconRoot.sizeDelta = new Vector2(arcIconSize, arcIconSize);
                    SetIconPosition(labelButtonsRoot.position, true);
                }
                else if (currentData.type == ChoiceData.ChoiceType.List)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(listButtonsRoot);
                    iconRoot.sizeDelta = new Vector2(listIconSize, listIconSize);
                    iconRoot.position = buttons[0].transform.position + new Vector3(listIconSpacing * (Screen.width / 800.0f), 0, 0);
                }

                SetIconSprite(iconsCache[0]);
            }
        }

        public override void OnDisabled()
        {
            HideIcon();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == currentButtonIndex)
                    buttons[i].Fade(0.5f, () =>
                    {
                        iconImage.sprite = null;
                        for (int i = 0; i < iconsCache.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(iconsCacheNames[i].Key))
                            {
                                iconsCache[i] = null;
                                if (string.IsNullOrEmpty(iconsCacheNames[i].Value))
                                    resourceManager.ReleaseResource<Sprite>($"Choices/{iconsCacheNames[i].Key}");
                                else
                                    resourceManager.ReleaseSpritesheetResource<Sprite>($"Choices/{iconsCacheNames[i].Value}", iconsCacheNames[i].Key);
                            }
                        }

                        showingChoice = false;
                        foreach (Transform child in listButtonsRoot)
                        {
                            child.DOKill(false);
                            Destroy(child.gameObject);
                        }
                        foreach (Transform child in labelButtonsRoot)
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

                bool lastHasIcon = iconsCache[currentButtonIndex] != null;

                buttons[currentButtonIndex].OnExit();
                currentButtonIndex = id;
                buttons[currentButtonIndex].OnEnter();

                if (iconsCache != null)
                {
                    SetIconSprite(iconsCache[currentButtonIndex]);
                }

                if (currentData.type == ChoiceData.ChoiceType.List && iconsCache[currentButtonIndex] != null)
                {
                    SetIconPosition(buttons[currentButtonIndex].transform.position + new Vector3(listIconSpacing * (Screen.width / 800.0f), 0, 0), !lastHasIcon);
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
            audioManager = null;
            resourceManager = null;
            OnUnRegisterInputs();
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
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

        public override bool OnLoad(JSON json)
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

                        if (arrayData[i].ContainsKey("linkedScript"))
                            currentData.entries[i].linkedLine = arrayData[i].GetJNumber("linkedScript").AsUInt();

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

            return true;
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
            // Choices are displayed in a circle
            Circle,
            // Choices are displayed in a demi-circle
            Arc
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

