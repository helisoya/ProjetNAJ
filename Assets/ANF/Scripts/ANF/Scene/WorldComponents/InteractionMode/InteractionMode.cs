using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.Scene
{
    /// <summary>
    /// Represents the different highlight types for the Interaction Mode.<br></br>
    /// None : No Highlight<br></br>
    /// OnlySelected : Only the currently selected object is highlighted<br></br>
    /// All : All objects are highlighted. The currently selected object is of a different color<br></br>
    /// </summary>
    public enum HighlightType
    {
        None,
        OnlySelected,
        All
    }

    /// <summary>
    /// Handles the interaction mode
    /// </summary>
    [System.Serializable]
    public class InteractionMode : WorldComponent
    {
        private const int LOAD_CACHE_FRAME_LIFE = 2;

        [Header("Infos")]
        [SerializeField] private LayerMask interactablesMask;
        [SerializeField] private string[] guiComponentsToDisable;

        [Header("Highlight")]
        [Tooltip("Full highlight means that every interactable object will glow. Otherwise, only the currently selected object will glow.")]
        [SerializeField] private HighlightType highlightType = HighlightType.All;
        [Tooltip("0 means invisible highlight. 1 Means the highlight will replace the base model's color")]
        [SerializeField] private float highlightStrength = 0.5f;
        [ColorUsage(true, true)][SerializeField] private Color baseColor;
        [ColorUsage(true, true)][SerializeField] private Color selectedColor;

        [Header("GUI Icon")]
        [SerializeField] private RawImage prefabIcon;
        private RawImage currentIcon;

        private AudioManager audioManager;
        private BackgroundManager backgroundManager;
        private Dictionary<string, InteractableObject> registeredObjects = new Dictionary<string, InteractableObject>();
        private List<InteractableObject> currentInteractionObjects = new List<InteractableObject>();
        private int currentIndex;
        private bool reloadInteractionMode = false;
        private int currentButtonInputSide = 0;
        private float cooldownToNextButtonIncrement = 0;
        private float cursorMoveCooldown = 0.25f;

        private Vector2 mousePosition;
        private bool canTryMouseClick;
        private bool keyboardMode = true;

        private JSON loadedDataCache = null;
        private int currentLoadCacheFrameLife = 0;

        public bool inInteractionMode { get; private set; } = false;
        public string selectedScript { get; private set; } = null;


#if UNITY_EDITOR
        private bool debugShowInteractableCollisions = false;

#endif


        public override WorldComponent CloneComponent()
        {
            return new InteractionMode()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                highlightType = highlightType,
                baseColor = baseColor,
                selectedColor = selectedColor,
                interactablesMask = interactablesMask,
                guiComponentsToDisable = guiComponentsToDisable,
                prefabIcon = prefabIcon
            };
        }

        /// <summary>
        /// Registers a new interactable object
        /// </summary>
        /// <param name="obj">The new interactableObject</param>
        public void Register(InteractableObject obj)
        {
            if (obj == null)
                return;

            string id = obj.GetID();
            if (!registeredObjects.ContainsKey(id))
            {
                RestoreFromCache(obj);
                registeredObjects.Add(id, obj);
            }
            else
            {
                Debug.LogError($"Trying to add duplicate interactable object : {id}");
            }
        }

        /// <summary>
        /// Unregisters an interactable object
        /// </summary>
        /// <param name="obj">The interactable object</param>
        public void UnRegister(InteractableObject obj)
        {
            if (obj == null)
                return;

            string id = obj.GetID();
            if (registeredObjects.ContainsKey(obj.GetID()))
                registeredObjects.Remove(obj.GetID());
            else
                Debug.LogError($"Trying to remove an non registered interactable object : {id}");
        }

        /// <summary>
		/// Changes the next script for a specific interactable object
		/// </summary>
		/// <param name="id">The object's Id</param>
		/// <param name="script">The next script</param>
        public void SetInteractableObjectNextScript(string id, string script)
        {
            if (registeredObjects.TryGetValue(id, out InteractableObject obj))
                obj.SetNextScript(script);
        }

        /// <summary>
        /// Changes if a specific interactable object is hidden or not
        /// </summary>
        /// <param name="id">The object's Id</param>
        /// <param name="hidden">True if the object is hidden</param>
        public void SetInteractableObjectHidden(string id, bool hidden)
        {
            if (registeredObjects.TryGetValue(id, out InteractableObject obj))
                obj.SetHidden(hidden);
        }

        /// <summary>
        /// Updates the interactable icon for a specific object
        /// </summary>
        /// <param name="obj">The object</param>
        private void UpdateIconFor(InteractableObject obj)
        {
            if (currentIcon)
            {
                currentIcon.texture = obj.GetIcon();

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(manager.GetGUIManager().GetRoot(),
                    Camera.main.WorldToScreenPoint(obj.GetApproximateVisualPosition()),
                    null, out Vector2 canvasPos))
                {
                    currentIcon.GetComponent<RectTransform>().anchoredPosition = canvasPos;
                }
                else
                {
                    currentIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, -50);
                }
            }
        }

        /// <summary>
		/// Generates a sorted list of non hidden interactable objects
		/// </summary>
        private void GenerateInteractionList()
        {
            currentInteractionObjects.Clear();
            foreach (InteractableObject obj in registeredObjects.Values)
            {
                if (!obj.GetIsHidden())
                    currentInteractionObjects.Add(obj);
            }

            currentInteractionObjects.Sort((InteractableObject o1, InteractableObject o2) =>
            {
                return Camera.main.WorldToScreenPoint(o1.GetApproximateVisualPosition()).x.CompareTo(Camera.main.WorldToScreenPoint(o2.GetApproximateVisualPosition()).x);
            });
        }

        /// <summary>
		/// Starts the interaction mode
		/// </summary>
        public void StartInteractionMode()
        {
            foreach (string guiComponent in guiComponentsToDisable)
            {
                if (manager.GetGUIManager().GetComponent<GUIComponent>(guiComponent, out GUIComponent component))
                    component.SetEnabled(false);
            }

            canTryMouseClick = false;

            GenerateInteractionList();

            if (currentInteractionObjects.Count > 0)
            {
                if (!currentIcon)
                    currentIcon = GameObject.Instantiate(prefabIcon, manager.GetGUIManager().GetRoot());

                inInteractionMode = true;
                currentIndex = 0;

                foreach (InteractableObject obj in currentInteractionObjects)
                {
                    obj.SetHighlightStrength(highlightStrength);

                    if (highlightType == HighlightType.All)
                    {
                        obj.SetHighlightAlpha(1);
                        obj.SetHighlightColor(baseColor);
                    }
                }

                if (highlightType != HighlightType.None && keyboardMode)
                {
                    currentInteractionObjects[currentIndex].SetHighlightAlpha(1);
                    currentInteractionObjects[currentIndex].SetHighlightColor(selectedColor);
                }

                Physics.SyncTransforms(); // To force colliders to world (could be temp set to local instead)
                UpdateIconFor(currentInteractionObjects[currentIndex]);
                if (currentIcon != null)
                    currentIcon.gameObject.SetActive(keyboardMode);
                OnRegisterInputs();
            }
            else
            {
                inInteractionMode = false;
            }
        }

        /// <summary>
		/// Confirms the object and ends the interaction mode
		/// </summary>
		/// <param name="index">The object's index</param>
        public void ConfirmObject(int index)
        {
            if (index == -1)
                return;

            OnUnRegisterInputs();

            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            inInteractionMode = false;
            selectedScript = currentInteractionObjects[index].GetNextScript();

            foreach (InteractableObject obj in currentInteractionObjects)
            {
                obj.SetHighlightAlpha(0);
            }

            if (currentIcon)
                GameObject.Destroy(currentIcon.gameObject);
            currentIcon = null;

            currentInteractionObjects.Clear();
        }

        /// <summary>
		/// Selects an interactable object
		/// </summary>
		/// <param name="index">The object's index</param>
        /// <param name="force">True if the change should be forced</param>
        public void SelectObject(int index, bool force = false)
        {
            if (force || index != currentIndex)
            {
                if (index != -1 && audioManager != null)
                    audioManager.PlayUICursorMoveSFX();

                if (currentIndex != -1)
                {
                    if (highlightType == HighlightType.OnlySelected)
                        currentInteractionObjects[currentIndex].SetHighlightAlpha(0);

                    if (highlightType != HighlightType.None)
                        currentInteractionObjects[currentIndex].SetHighlightColor(baseColor);
                }

                currentIndex = index;

                if (index != -1)
                {
                    if (highlightType == HighlightType.OnlySelected)
                        currentInteractionObjects[currentIndex].SetHighlightAlpha(1);

                    if (highlightType != HighlightType.None)
                        currentInteractionObjects[currentIndex].SetHighlightColor(selectedColor);

                    UpdateIconFor(currentInteractionObjects[currentIndex]);
                }
            }
        }

        public void OnHighlightTypeChange(object value)
        {
            highlightType = (HighlightType)value;

            if (inInteractionMode)
            {
                for (int i = 0; i < currentInteractionObjects.Count; i++)
                {
                    currentInteractionObjects[i].SetHighlightAlpha(highlightType == HighlightType.All ||
                        (highlightType == HighlightType.OnlySelected && i == currentIndex) ? 1 : 0);
                }
            }
        }

        public void OnHighlightStrengthChange(object value)
        {
            highlightStrength = (float)value;

            for (int i = 0; i < currentInteractionObjects.Count; i++)
            {
                currentInteractionObjects[i].SetHighlightStrength(highlightStrength);
            }
        }

        public void OnHighlightColorChange(object value)
        {
            baseColor = (Color)value;

            if (inInteractionMode && highlightType != HighlightType.None)
            {
                for (int i = 0; i < currentInteractionObjects.Count; i++)
                {
                    if (i != currentIndex)
                        currentInteractionObjects[i].SetHighlightColor(baseColor);
                }
            }
        }

        public void OnSelectedColorChange(object value)
        {
            selectedColor = (Color)value;


            if (inInteractionMode && highlightType != HighlightType.None)
            {
                currentInteractionObjects[currentIndex].SetHighlightColor(selectedColor);
            }
        }

        public override void OnInitialize()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                highlightType = (HighlightType)settings.Register("InteractionMode_HighlightType",
                    SettingsContainer.SettingsDataType.Int,
                    OnHighlightTypeChange);

                highlightStrength = (float)settings.Register("InteractionMode_HighlightStrength",
                    SettingsContainer.SettingsDataType.Float,
                    OnHighlightStrengthChange);

                baseColor = (Color)settings.Register("InteractionMode_HighlightColor",
                    SettingsContainer.SettingsDataType.Color,
                    OnHighlightColorChange);

                selectedColor = (Color)settings.Register("InteractionMode_SelectedColor",
                    SettingsContainer.SettingsDataType.Color,
                    OnSelectedColorChange);
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
            manager.GetWorld().GetComponent(out backgroundManager);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                cursorMoveCooldown = (float)settings.Register("GeneralMenu_CursorCooldown", SettingsContainer.SettingsDataType.Float, OnCursorCooldownChange);

            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MousePosition").performed += OnMousePosition;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled += OnMove;
        }

        private void OnCursorCooldownChange(object value)
        {
            cursorMoveCooldown = (float)value;
        }

        public override void OnUpdate()
        {
#if UNITY_EDITOR
            // Debug show interaction renderers
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                debugShowInteractableCollisions = !debugShowInteractableCollisions;

                foreach (InteractableObject interactable in registeredObjects.Values)
                {
                    MeshRenderer mr = interactable.GetComponent<MeshRenderer>();
                    if (mr)
                        mr.enabled = debugShowInteractableCollisions;
                }
            }
#endif

            if (inInteractionMode)
            {
                if (reloadInteractionMode)
                {
                    if (backgroundManager != null &&
                        (backgroundManager.unloadingBackground || backgroundManager.loadingBackground))
                        return;

                    reloadInteractionMode = false;
                    StartInteractionMode();
                    return;
                }


                if (currentButtonInputSide != 0)
                {
                    cooldownToNextButtonIncrement -= Time.deltaTime;
                    if (cooldownToNextButtonIncrement <= 0)
                    {
                        if (keyboardMode)
                            IncrementObjectWithInput();

                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                    }
                }


                if (!keyboardMode)
                {
                    if (currentIcon)
                        currentIcon.gameObject.SetActive(false);

                    RaycastHit hit;
                    InteractableObject current = null;

                    if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out hit, 500, interactablesMask))
                    {
                        current = hit.transform.GetComponent<InteractableObject>();
                        if (!currentInteractionObjects.Contains(current))
                            current = null;
                    }

                    Cursor.SetCursor(current == null ? null : current.GetIcon(), Vector2.zero, CursorMode.Auto);

                    if (current != null)
                    {
                        for (int i = 0; i < currentInteractionObjects.Count; i++)
                        {
                            if (currentInteractionObjects[i] == current)
                            {
                                SelectObject(i);

                                if (canTryMouseClick)
                                    ConfirmObject(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        SelectObject(-1);
                    }
                }
                canTryMouseClick = false;
            }
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }

        public override void OnPaused()
        {
            if (currentIcon && inInteractionMode)
                currentIcon.gameObject.SetActive(false);
            if (inInteractionMode)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public override void OnUnPaused()
        {
            if (currentIcon && inInteractionMode)
                currentIcon.gameObject.SetActive(true);
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode && keyboardMode && context.ReadValueAsButton())
            {
                ConfirmObject(currentIndex);
            }
        }

        private void OnMousePosition(InputAction.CallbackContext context)
        {
            keyboardMode = false;
            mousePosition = context.ReadValue<Vector2>();

            if (isEnabled && !isPaused && inInteractionMode && currentIcon)
            {
                currentIcon.gameObject.SetActive(false);
            }
        }

        private void OnMouseClick(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && inInteractionMode && context.ReadValueAsButton())
            {
                canTryMouseClick = true;
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();

            if (isEnabled && !isPaused && inInteractionMode)
            {
                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    keyboardMode = true;
                    if (currentIcon)
                        currentIcon.gameObject.SetActive(true);
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                    noMovement = false;
                    if (currentButtonInputSide == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide = value.x < 0 ? -1 : 1;

                        IncrementObjectWithInput();
                    }
                }

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide = 0;
                }
            }
            else
            {
                keyboardMode = Mathf.Abs(value.x) >= 0.9f;
            }
        }

        /// <summary>
        /// Increments the current object with the keyboard input
        /// </summary>
        private void IncrementObjectWithInput()
        {
            SelectObject((currentIndex + currentButtonInputSide + currentInteractionObjects.Count) % currentInteractionObjects.Count);
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MouseClick").performed += OnMouseClick;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed -= OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MouseClick").performed -= OnMouseClick;
        }

        public override bool OnChangeScene()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MousePosition").performed -= OnMousePosition;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;

            if (currentIcon && inInteractionMode)
                currentIcon.gameObject.SetActive(false);
            OnUnRegisterInputs();

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                settings.Unregister("InteractionMode_HighlightType", OnHighlightTypeChange);
                settings.Unregister("InteractionMode_HighlightColor", OnHighlightColorChange);
                settings.Unregister("InteractionMode_SelectedColor", OnSelectedColorChange);
            }
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            // Keeping the load cache intact until background & character/statics manager are loaded
            // It is destroyed after X frames (2 by default)
            if (loadedDataCache != null)
            {
                if (manager.GetWorld().GetComponent(out BackgroundManager backgroundManager))
                {
                    if (backgroundManager.IsLoadingOrCleaningUp(false))
                        return true;
                }

                if (manager.GetWorld().GetComponent(out CharacterManager characterManager))
                {
                    if (characterManager.IsLoadingOrCleaningUp(false))
                        return true;
                }

                if (manager.GetWorld().GetComponent(out StaticObjectManager staticObjectManager))
                {
                    if (staticObjectManager.IsLoadingOrCleaningUp(false))
                        return true;
                }

                if (updateComponent)
                    currentLoadCacheFrameLife--;

                if (currentLoadCacheFrameLife <= 0)
                {
                    if (updateComponent)
                    {
                        if (loadedDataCache.Count > 0)
                            Debug.LogWarning($"Interaction Cache deleted with still {loadedDataCache.Count} cached objects");
                        loadedDataCache = null;
                    }
                    return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to restore an interactable object from the loaded data cache
        /// </summary>
        /// <param name="obj">The object</param>
        private void RestoreFromCache(InteractableObject obj)
        {
            if (loadedDataCache == null)
                return;

            if (loadedDataCache.ContainsKey(obj.GetID()))
            {
                JSON objJSON = loadedDataCache.GetJSON(obj.GetID());
                obj.SetHidden(objJSON.GetBool("hidden"));

                if (objJSON.ContainsKey("script"))
                    obj.SetNextScript(objJSON.GetString("script"));

                loadedDataCache.Remove(obj.GetID());
            }
        }

        public override void OnSave(JSON json)
        {
            JSON registeredObjectsJSON = new JSON();
            foreach (InteractableObject obj in registeredObjects.Values)
            {
                JSON objectJSON = new JSON();
                if (!string.IsNullOrEmpty(obj.GetNextScript()))
                    objectJSON.Add("script", obj.GetNextScript());
                objectJSON.Add("hidden", obj.GetIsHidden());
                registeredObjectsJSON.Add(obj.GetID(), objectJSON);
            }
            json.Add("registeredObjects", registeredObjectsJSON);


            if (inInteractionMode)
            {
                json.Add("inInteractionMode", inInteractionMode);
            }
        }

        public override bool OnLoad(JSON json)
        {
            loadedDataCache = null;
            if (json.ContainsKey("registeredObjects"))
            {
                loadedDataCache = new JSON(json.GetJSON("registeredObjects").AsDictionary());

                foreach (InteractableObject obj in registeredObjects.Values)
                {
                    RestoreFromCache(obj);
                }

                if (loadedDataCache.Count == 0)
                    loadedDataCache = null;
                else
                    currentLoadCacheFrameLife = LOAD_CACHE_FRAME_LIFE;
            }

            if (json.ContainsKey("inInteractionMode"))
            {
                inInteractionMode = true;
                reloadInteractionMode = true;
            }

            return loadedDataCache == null;
        }
    }
}
