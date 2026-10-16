using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the settings menu located within the pause menu
    /// </summary>
    public class SettingsMenuUI : GUIComponent
    {
        [Header("Tabs")]
        [SerializeField] private Transform tabsRoot;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private SettingsTabUI tabPrefab;
        [SerializeField, SerializeReference, SubclassSelector(AllowNull = false)] private SettingsHandlerUI[] handlers;
        private Dictionary<string, SettingsTabUI> tabs;

        [Header("Components")]
        [Tooltip("The UI/Navigate action used by controller and keyboard to use menus")]
        [SerializeField] InputActionReference navigateAction;
        [SerializeField] private GameObject rebindRoot;
        [SerializeField] private LocalizedText rebindText;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private SettingsEntryUIToggle prefabToggleToggle;
        [SerializeField] private SettingsEntryUISlider prefabToggleSlider;
        [SerializeField] private SettingsEntryUIDropdown prefabToggleDropdown;
        [SerializeField] private SettingsEntryUIColorPicker prefabToggleColorPicker;
        [SerializeField] private SettingsEntryUIButton prefabToggleButton;
        [SerializeField] private SettingsEntryUIInputBinder prefabToggleInputBinder;

        private AudioManager audioManager;
        private List<Selectable> objects;
        private GameObject lastSelectedObject;
        private bool selectFirstObject;

        private bool movingWithInput;
        private bool rebinding;

        public override void OnInitialize()
        {
            objects = new List<Selectable>();
            tabs = new Dictionary<string, SettingsTabUI>();
            movingWithInput = false;
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            objects.Clear();
            tabs.Clear();

            foreach (Transform child in tabsRoot)
                Destroy(child.gameObject);

            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i].Initialize(this, manager);

                foreach (SettingsTabUI tab in tabs.Values)
                    tab.Rebuild();

                LayoutRebuilder.ForceRebuildLayoutImmediate(tabsRoot.GetComponent<RectTransform>());
            }

            foreach (SettingsTabUI tab in tabs.Values)
            {
                foreach (Selectable selectable in tab.GetSelectables())
                    objects.Add(selectable);

                if (tab.GetResetButton())
                    objects.Add(tab.GetResetButton());
            }

            RegenerateObjectsNavigation();
        }

        public override void OnUpdate()
        {
            if (selectFirstObject)
            {
                selectFirstObject = false;
                if (objects.Count != 0)
                    EventSystem.current.SetSelectedGameObject(objects[0].gameObject);
            }

            if (navigateAction.action.triggered)
                movingWithInput = !movingWithInput;

            if (!movingWithInput)
                return;

            if (EventSystem.current.currentSelectedGameObject && lastSelectedObject != EventSystem.current.currentSelectedGameObject)
            {
                lastSelectedObject = EventSystem.current.currentSelectedGameObject;
                Selectable selectable = lastSelectedObject.GetComponent<Selectable>();

                if (selectable != null)
                {
                    int index = objects.IndexOf(selectable);

                    if (index != -1)
                    {
                        scrollbar.value = 1.0f - (float)index / (objects.Count - 1);
                    }
                }
            }
        }


        public override void OnEnabled()
        {
            rebinding = false;
            selectFirstObject = true;

            scrollbar.value = 1;
        }


        public override void OnDisabled()
        {
            if (colorPicker.IsOpen)
                colorPicker.Close();

            EventSystem.current.SetSelectedGameObject(null);

            string globalDataSaveFile = FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder + "global.json";
            SaveUtils.SaveGlobalData(PersistentDataManager.instance.GetGlobalData(),
                PersistentDataManager.instance.GetANFInput(), globalDataSaveFile);

            PersistentDataManager.instance.GetANFInput().TriggerSchemeChange();
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        private void OnPauseInput(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && !rebinding && context.ReadValueAsButton())
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();

                if (colorPicker.IsOpen)
                    colorPicker.Close();
                else
                    TriggerDelayedClosing();
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

        public void StartRebindingProcess(InputAction action, int bindingIndex, string labelKey, Image inputImage)
        {
            rebinding = true;

            GameObject previousObj = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals))
                labelKey = locals.GetLocal(labelKey);

            rebindRoot.SetActive(true);
            rebindText.SetInjectors(new object[] { labelKey });

            action.Disable();

            InputActionRebindingExtensions.RebindingOperation operation = action.PerformInteractiveRebinding(bindingIndex);
            operation.OnCancel(
                    operation =>
                    {
                        action.Enable();
                        rebinding = false;
                        rebindRoot.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(previousObj);
                    })
                .OnComplete(
                    operation =>
                    {
                        action.Enable();
                        InputBinding binding = action.bindings[bindingIndex];

                        string groups = binding.groups;
                        if (groups.StartsWith(';'))
                            groups = groups.Substring(1);

                        inputImage.sprite = PersistentDataManager.instance.GetANFInput().GetIcon(
                            groups.Split(";")[0],
                            binding.effectivePath.Split("/", 2)[1]
                        );

                        rebinding = false;
                        rebindRoot.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(previousObj);
                    });

            operation.Start();
        }

        /// <summary>
		/// Regenerates the current object's navigation manually
		/// </summary>
        private void RegenerateObjectsNavigation()
        {
            if (objects.Count > 1)
            {
                for (int i = 0; i < objects.Count - 1; i++)
                {
                    Navigation navigationTop = new Navigation()
                    {
                        mode = Navigation.Mode.Explicit,
                        wrapAround = true,
                        selectOnDown = objects[i + 1],
                        selectOnUp = objects[i].navigation.selectOnUp
                    };

                    Navigation navigationDown = new Navigation()
                    {
                        mode = Navigation.Mode.Explicit,
                        wrapAround = true,
                        selectOnDown = null,
                        selectOnUp = objects[i]
                    };

                    objects[i].navigation = navigationTop;
                    objects[i + 1].navigation = navigationDown;
                }
            }
        }

        /// <summary>
        /// Redraws the localized elements
        /// </summary>
        public void RedrawLocalizedElements()
        {
            foreach (SettingsHandlerUI handler in handlers)
                handler.RedrawLocalizedElements();
        }

        /// <summary>
        /// Opens the color picker
        /// </summary>
        /// <param name="startColor">The color picker</param>
        /// <param name="initiator">The initiator</param>
        /// <param name="callback">The callback once the color is selected</param>
        public void OpenColorPicker(Color startColor, Selectable initiator, Action<Color> callback)
        {
            colorPicker.Open(startColor, initiator, callback);
        }

        /// <summary>
        /// Gets or create a tab from the settings
        /// </summary>
        /// <param name="tabKey">The tab's key</param>
        /// <returns>The tab</returns>
        public SettingsTabUI GetTab(string tabKey)
        {
            if (tabs.TryGetValue(tabKey, out SettingsTabUI tab))
                return tab;

            SettingsTabUI instance = Instantiate(tabPrefab, tabsRoot);
            instance.SetLabelKey(tabKey);
            tabs.Add(tabKey, instance);
            return instance;
        }

        /// <summary>
        /// Registers a new reset action for a button
        /// </summary>
        /// <param name="tabKey">The tab's key</param>
        /// <param name="action">The reset action</param>
        public void RegisterResetAction(string tabKey, UnityAction action)
        {
            if (tabs.TryGetValue(tabKey, out SettingsTabUI tab))
            {
                tab.RegisterResetAction(action);
            }
        }

        /// <summary>
        /// Creates a new toggle in the settings menu
        /// </summary>
        /// <param name="labelKey">The toggle's label</param>
        /// <param name="tab">The toggle's tab</param>
        /// <returns>The toggle</returns>
        public Toggle CreateToggle(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleToggle);
        }

        /// <summary>
        /// Creates a new slider in the settings menu
        /// </summary>
        /// <param name="labelKey">The slider's label</param>
        /// <param name="tab">The slider's tab</param>
        /// <returns>The slider</returns>
        public Slider CreateSlider(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleSlider);
        }

        /// <summary>
        /// Creates a new color picker in the settings menu
        /// </summary>
        /// <param name="labelKey">The color picker's label</param>
        /// <param name="tab">The color picker's tab</param>
        /// <returns>The color picker</returns>
        public Button CreateColorPicker(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleColorPicker);
        }

        /// <summary>
        /// Creates a new input binder in the settings menu
        /// </summary>
        /// <param name="labelKey">The input binder's label</param>
        /// <param name="tab">The input binder's tab</param>
        /// <returns>The input binder</returns>
        public Button CreateInputBinder(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleInputBinder);
        }

        /// <summary>
        /// Creates a new button in the settings menu
        /// </summary>
        /// <param name="labelKey">The button's label</param>
        /// <param name="tab">The button's tab</param>
        /// <returns>The button</returns>
        public Button CreateButton(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleButton);
        }

        /// <summary>
        /// Creates a new dropdown in the settings menu
        /// </summary>
        /// <param name="labelKey">The dropdown's label</param>
        /// <param name="tab">The dropdown's tab</param>
        /// <returns>The dropdown</returns>
        public TMP_Dropdown CreateDropdown(string labelKey, SettingsTabUI tab)
        {
            return CreateEntryInstance(labelKey, tab, prefabToggleDropdown);
        }

        /// <summary>
        /// Creates an instance of an entry prefab
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="labelKey">The label's key</param>
        /// <param name="tab">The item's tab</param>
        /// <param name="prefab">The prefab</param>
        /// <returns>The item created</returns>
        private T CreateEntryInstance<T>(string labelKey, SettingsTabUI tab, SettingsEntryUI<T> prefab) where T : Selectable
        {
            SettingsEntryUI<T> instance = Instantiate(prefab, tab.GetRoot());
            instance.SetLabel(labelKey);

            tab.AddSelectable(instance.GetItem());

            return instance.GetItem();
        }
    }
}

