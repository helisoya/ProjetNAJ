using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Handles ANF's map system
    /// </summary>
    public class MapUI : GUIComponent
    {
        [Header("Map")]
        [SerializeField] private RectTransform mapRoot;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private MapUIButton prefabButton;
        [SerializeField] private float thresholdInputDistance = 10f;
        private List<MapUIButton> buttons;
        private MapDefs currentMapDefs;
        private ANF.Persistent.MapData currentMap;
        private string currentLocationId;
        private Persistent.AudioManager audioManager;

        private int currentButtonIndex;
        private Vector2Int currentButtonInputSide = new Vector2Int();
        private float cooldownToNextButtonIncrement = 0;

        public bool showingMap { get; private set; } = false;
        public string selectedScript { get; private set; } = null;

        /// <summary>
        /// Opens the map
        /// </summary>
        /// <param name="enabled">Enabl</param>
        /// <param name="map">The map to open</param>
        /// <param name="currentLocationId">The player's current position/button</param>
        public void SetEnabled(bool enabled, ANF.Persistent.MapData mapData, ANF.Persistent.MapDefs mapDefs, string currentLocation)
        {
            // ! Current point
            if (!isEnabled && enabled)
            {
                showingMap = true;
                selectedScript = null;
                currentMap = mapData;
                currentMapDefs = mapDefs;
                currentLocationId = currentLocation;
            }

            SetEnabled(enabled);
        }

        public override void OnInitialize()
        {
            mapRoot.localScale = new Vector3(0, 1, 1);
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent<Persistent.AudioManager>(out audioManager);
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

        /// <summary>
		/// Computes the unstretched background image's size
		/// </summary>
        /// <param name="backgroundSize">The background's full size</param>
		/// <returns>The image's size</returns>
        private Vector2 CalculateImageSize(Vector2 backgroundSize)
        {
            Vector2 size = new Vector2();
            if (backgroundSize.x > backgroundSize.y)
            {
                float mul = backgroundSize.y / backgroundImg.sprite.rect.height;
                size.x = backgroundImg.sprite.rect.width * mul;
                size.y = backgroundSize.y;
            }
            else
            {
                float mul = backgroundSize.x / backgroundImg.sprite.rect.width;
                size.x = backgroundSize.x;
                size.y = backgroundImg.sprite.rect.height * mul;
            }
            return size;
        }

        public override void OnEnabled()
        {
            currentButtonIndex = 0;

            RectTransform guiRoot = gui.GetRoot();

            backgroundImg.sprite = currentMap.backgroundSprite;
            Vector2 backgroundSize = guiRoot.sizeDelta - new Vector2(20f, 10f);
            Vector2 imageSize = CalculateImageSize(backgroundSize);

            Vector2 sizeDifference = backgroundSize - imageSize;
            buttons = new List<MapUIButton>();

            foreach (Transform child in backgroundImg.transform)
            {
                DOTween.Kill(child);
                Destroy(child.gameObject);
            }


            foreach (ANF.Persistent.MapButton button in currentMap.buttons)
            {
                bool canShow = false;
                string scriptFound = "";
                foreach (MapButtonDef def in currentMapDefs.buttons)
                {
                    if (def.linkedButton.Equals(button.id))
                    {
                        int variableValue = 0;
                        if ((def.type == MapDefsType.Variable || def.type == MapDefsType.VariableToggle) &&
                            PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer container))
                            container.GetVariable(def.linkedVariable, out variableValue);

                        if (def.CheckIfVisible(variableValue))
                        {
                            scriptFound = def.linkedScript;
                            canShow = true;
                        }

                        break;
                    }
                }

                if (canShow)
                {
                    bool buttonIsCurrentLocation = currentLocationId != null && button.id.Equals(currentLocationId);

                    if (buttonIsCurrentLocation)
                        currentButtonIndex = buttons.Count;

                    MapUIButton instance = Instantiate(prefabButton, backgroundImg.rectTransform);
                    RectTransform instanceTransform = instance.GetComponent<RectTransform>();
                    instanceTransform.eulerAngles = new Vector3(0, 0, button.rotation);
                    instanceTransform.anchoredPosition = new Vector2(
                        sizeDifference.x / 2.0f + imageSize.x * button.normalizedPosition.x,
                        -sizeDifference.y / 2.0f - imageSize.y * button.normalizedPosition.y
                    );
                    instance.Initialize(buttons.Count, currentMap.id + "_" + button.id,
                        scriptFound, button.sprite, buttonIsCurrentLocation, guiRoot, this);
                    buttons.Add(instance);
                }
            }

            if (currentButtonIndex >= 0 && currentButtonIndex < buttons.Count)
                buttons[currentButtonIndex].OnEnter();
            mapRoot.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnDisabled()
        {
            showingMap = false;
            mapRoot.DOScaleX(0, 0.5f).SetEase(Ease.OutQuad);
        }

        /// <summary>
		/// Selects a map button and applies its effect
		/// </summary>
		/// <param name="id">The button's index</param>
        public void SelectButton(int id)
        {
            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();
            selectedScript = buttons[id].GetLinkedScript();
            SetEnabled(false);
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

            if (force || currentButtonIndex != id)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorMoveSFX();
                buttons[currentButtonIndex].OnExit();
                currentButtonIndex = id;
                buttons[currentButtonIndex].OnEnter();
            }
        }

        /// <summary>
        /// Increments the current button with the keyboard input
        /// </summary>
        private void IncrementButtonWithInput()
        {
            Vector2 position = buttons[currentButtonIndex].GetComponent<RectTransform>().anchoredPosition;
            Vector2Int closestIndex = new Vector2Int(currentButtonIndex, currentButtonIndex);
            Vector2 closestDistance = new Vector2(99999, 99999);

            Vector2 tmpPos;
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == currentButtonIndex)
                    continue;

                tmpPos = buttons[i].GetComponent<RectTransform>().anchoredPosition;

                if ((tmpPos.x <= position.x - thresholdInputDistance && currentButtonInputSide.x == 1) ||
                    (tmpPos.x >= position.x + thresholdInputDistance && currentButtonInputSide.x == -1))
                {
                    float distance = Vector2.Distance(tmpPos, position);
                    if (closestDistance.x > distance)
                    {
                        closestDistance.x = distance;
                        closestIndex.x = i;
                    }
                }

                if ((tmpPos.y <= position.y - thresholdInputDistance && currentButtonInputSide.y == 1) ||
                    (tmpPos.y >= position.y + thresholdInputDistance && currentButtonInputSide.y == -1))
                {
                    float distance = Vector2.Distance(tmpPos, position);
                    if (closestDistance.y > distance)
                    {
                        closestDistance.y = distance;
                        closestIndex.y = i;
                    }
                }
            }

            if (currentButtonInputSide.x != 0 && currentButtonInputSide.y != 0)
            {
                if (closestDistance.x <= closestDistance.y)
                    SetCurrentButton(closestIndex.x);
                else
                    SetCurrentButton(closestIndex.y);
            }
            else if (currentButtonInputSide.x != 0)
            {
                SetCurrentButton(closestIndex.x);
            }
            else if (currentButtonInputSide.y != 0)
            {
                SetCurrentButton(closestIndex.y);
            }

        }


        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                SelectButton(currentButtonIndex);
            }
        }
        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = 0.5f;
                        currentButtonInputSide.x = value.x < 0 ? 1 : -1;

                        IncrementButtonWithInput();
                    }
                }

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

                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide.x = 0;
                    currentButtonInputSide.y = 0;
                }
            }
        }

        public override void OnPaused()
        {
            cooldownToNextButtonIncrement = 0.0f;
            currentButtonInputSide = Vector2Int.zero;
        }

        public override void OnUnPaused()
        {
        }

        public override void OnSave(JSON json)
        {
            json.Add("showingMap", showingMap);
            json.Add("selectedScript", selectedScript);

            if (showingMap)
            {
                json.Add("currentLocationId", currentLocationId);
                json.Add("currentMap", currentMap.id);
                json.Add("currentMapDefs", currentMapDefs.id);
            }
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("showingMap"))
                showingMap = json.GetBool("showingMap");

            if (json.ContainsKey("selectedScript"))
                selectedScript = json.GetString("selectedScript");

            if (json.ContainsKey("currentLocationId"))
                currentLocationId = json.GetString("currentLocationId");

            if (showingMap && json.ContainsKey("currentMap") && json.ContainsKey("currentMapDefs") &&
            PersistentDataManager.instance.GetGlobalData().GetComponent<MapContainer>(out MapContainer container))
            {
                if (container.GetMap(json.GetString("currentMap"), out ANF.Persistent.MapData loadedMap) &&
                    container.GetDef(json.GetString("currentMapDefs"), out MapDefs loadedDefs))
                {
                    isEnabled = false;
                    SetEnabled(true, loadedMap, loadedDefs, currentLocationId);

                    if (json.ContainsKey("isEnabled"))
                        json.Remove("isEnabled");
                }
            }
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
        public override void OnChangeScene()
        {
            OnUnRegisterInputs();
        }
    }
}

