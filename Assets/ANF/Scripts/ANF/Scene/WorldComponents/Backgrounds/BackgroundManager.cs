using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANF.Scene
{
    /// <summary>
	/// Represents how a background is handled
	/// </summary>
    public enum BackgroundType
    {
        Prefab,
        Scene
    }

    /// <summary>
	/// Handles the game's backgrounds.
    /// Backgrounds can be 3D scenes, or 3D prefabs
	/// </summary>
    [System.Serializable]
    public class BackgroundManager : WorldComponent
    {
        [SerializeField] private BackgroundType backgroundType = BackgroundType.Prefab;
        [SerializeField] private bool asyncLoading = false;
        [SerializeField] private string prefabPath = "Backgrounds/";
        [SerializeField] private string skyboxDataPath = "Skyboxes/";
        [SerializeField] private SkyboxData defaultSkybox;
        [SerializeField] private Material skyboxMaterial;
        private Background currentBackground;
        private string currentBackgroundID;
        private BackgroundData currentCachedData = null;
        private bool enableWeatherEffects = true;
        private bool skipModeEnabled = false;

        private LerpInstanceColor lerpSunColor;
        private LerpInstanceFloat lerpSkybox;

        private AsyncOperation currentOperation;
        private string cachedNextBackgroundID;
        public bool loadingBackground { get; private set; }
        public bool unloadingBackground { get; private set; }
        public bool lerpingSkybox { 
            get
            {
                return (lerpSkybox != null && lerpSkybox.lerping) || (lerpSunColor != null && lerpSunColor.lerping);
            } 
        }

        public override WorldComponent CloneComponent()
        {
            return new BackgroundManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                asyncLoading = asyncLoading,
                backgroundType = backgroundType,
                prefabPath = prefabPath,
                skyboxDataPath = skyboxDataPath,
                defaultSkybox = defaultSkybox,
                skyboxMaterial = skyboxMaterial,
                enableWeatherEffects = enableWeatherEffects
            };
        }

        /// <summary>
		/// Gets the current background (Read Only)
		/// </summary>
		/// <returns>The current background</returns>
        public Background GetBackground()
        {
            return currentBackground;
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (lerpSkybox != null && lerpSkybox.lerping)
                lerpSkybox.ChangeDuration(0.1f);
            
            if (lerpSunColor != null && lerpSunColor.lerping)
                lerpSunColor.ChangeDuration(0.1f);
        }

        /// <summary>
        /// Changes the current background's skybox
        /// </summary>
        /// <param name="skyboxName">The skybox data's name</param>
        public void SetSkybox(string skyboxName, bool immediate = true, float transitionDuration = 2.0f)
        {
            if (!string.IsNullOrEmpty(skyboxDataPath))
            {
                SkyboxData data = Resources.Load<SkyboxData>(skyboxDataPath + skyboxName);
                if (data != null)
                {
                    SetSkybox(data, immediate, transitionDuration);
                }
            }
        }

        /// <summary>
        /// Changes the current background's skybox
        /// </summary>
        /// <param name="skyboxData">The skybox data</param>
        public void SetSkybox(SkyboxData skyboxData, bool immediate = true, float transitionDuration = 2.0f)
        {
            if (skyboxData == null)
                return;

            if (immediate || currentCachedData == null || currentCachedData.skyboxData == null)
            {
                RenderSettings.skybox.SetFloat("_Lerp", 0.0f);
                RenderSettings.skybox.SetTexture("_Current", skyboxData.skybox);
                RenderSettings.skybox.SetTexture("_Target", skyboxData.skybox);

                if (currentBackground)
                    currentBackground.SetSunColor(skyboxData.sunColor);
            }
            else
            {
                RenderSettings.skybox.SetFloat("_Lerp", 0.0f);
                RenderSettings.skybox.SetTexture("_Current", currentCachedData.skyboxData.skybox);
                RenderSettings.skybox.SetTexture("_Target", skyboxData.skybox);

                if (lerpSkybox == null)
                    lerpSkybox = new LerpInstanceFloat();

                lerpSkybox.StartLerp(0, 1, skipModeEnabled ? 0.1f : transitionDuration);

                if (lerpSunColor == null)
                    lerpSunColor = new LerpInstanceColor();

                lerpSunColor.StartLerp(currentCachedData.skyboxData.sunColor, skyboxData.sunColor, skipModeEnabled ? 0.1f : transitionDuration);
            }

            currentCachedData.skyboxData = skyboxData;
        }

        /// <summary>
		/// Changes the current background's weather effect
		/// </summary>
		/// <param name="weatherEffect">The new weather effect</param>
        public void SetWeatherEffect(string weatherEffect)
        {
            if (currentBackground != null)
            {
                currentCachedData.currentWeatherEffect = weatherEffect;
                currentBackground.SetWeatherEffect(enableWeatherEffects ? weatherEffect : null);
            }
        }

        /// <summary>
		/// Changes the current background's light direction (This will set the light's forward vector)
		/// </summary>
		/// <param name="direction">The new direction</param>
        public void SetLightDirection(Vector3 direction)
        {
            if (currentBackground != null)
            {
                currentCachedData.currentLightDirection = direction;
                currentBackground.SetLightDirection(direction);
            }
        }

        /// <summary>
		/// Loads a background and removes the previous background if needed
		/// </summary>
		/// <param name="ID">The new background's ID. In Scene Mode, this is the Scene's name. 
        /// In Prefab Mode, this is the prefab's path in Resources/[GeneralPrefabPath]/...</param>
        /// <param name="useDefaultData">True if the background's default data should be used</param>
		/// <param name="force">True if the change should be forced even if </param>
        public void SetBackground(string ID, bool useDefaultData, bool force = false)
        {
            if (force || ID != currentBackgroundID)
            {
                cachedNextBackgroundID = ID;

                if (useDefaultData || ID == null)
                    currentCachedData = null;

                currentOperation = RemoveCurrentBackground();

                unloadingBackground = currentOperation != null;

                if (!unloadingBackground)
                {
                    EndBackgroundUnloading();
                }
            }
        }

        /// <summary>
        /// Callback for when the background's unloading has stoped
        /// </summary>
        /// <param name="forceSync">True if async operations should be forbiden</param>
        private void EndBackgroundUnloading(bool forceSync = false)
        {
            unloadingBackground = false;
            currentBackground = null;
            currentOperation = null;

            if (!string.IsNullOrEmpty(cachedNextBackgroundID))
            {
                currentOperation = LoadBackground(cachedNextBackgroundID);

                loadingBackground = backgroundType == BackgroundType.Scene || (asyncLoading && !forceSync);

                if (!loadingBackground)
                    EndBackgroundLoading();
            }
        }

        /// <summary>
        /// Ends the background loading
        /// </summary>
        private void EndBackgroundLoading()
        {
            loadingBackground = false;
            currentBackgroundID = cachedNextBackgroundID;
            cachedNextBackgroundID = null;
            currentBackground = null;

            // Scene is already loaded
            // Prefab needs to be instanced
            if (backgroundType == BackgroundType.Prefab)
            {
                Background background = null;
                if (currentOperation is DummyResourceRequest)
                    background = ((DummyResourceRequest)currentOperation).GetObject() as Background;
                else if (currentOperation is ResourceRequest)
                    background = ((ResourceRequest)currentOperation).asset as Background;

                if (background != null)
                {
                    currentBackground = Object.Instantiate(background, manager.transform);
                }
            }
            else
            {
                UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(currentBackgroundID);
                if (scene != null && scene.GetRootGameObjects().Length == 1)
                {
                    currentBackground = scene.GetRootGameObjects()[0].GetComponent<Background>();
                }
            }

            if (currentBackground)
            {
                currentBackground.OnCreate(manager);

                if (currentCachedData == null)
                {
                    currentCachedData = currentBackground.GetDefaultData();
                    if(currentCachedData.skyboxData != null)
                    {
                        SetSkybox(currentCachedData.skyboxData);
                    }
                }
                    

                if (currentCachedData.skyboxData == null)
                {
                    currentCachedData.skyboxData = defaultSkybox;
                    SetSkybox(currentCachedData.skyboxData);
                }

                currentBackground.SetLightDirection(currentCachedData.currentLightDirection);
                currentBackground.SetWeatherEffect(currentCachedData.currentWeatherEffect);
                
                if(lerpSunColor != null && lerpSunColor.lerping)
                    currentBackground.SetSunColor(lerpSunColor.Get());
                else
                    currentBackground.SetSunColor(currentCachedData.skyboxData.sunColor);
            }

            currentOperation = null;
        }

        /// <summary>
        /// Loads a background
        /// </summary>
        /// <param name="ID">The background's ID</param>
        /// <param name="forceSync">True if async operations should be forbiden</param>
        /// <returns>An operation </returns>
        private AsyncOperation LoadBackground(string ID, bool forceSync = false)
        {
            AsyncOperation operation = null;

            if (backgroundType == BackgroundType.Scene)
            {
                if (!forceSync && asyncLoading)
                    SceneManager.LoadScene(ID, LoadSceneMode.Additive);
                else
                    operation = SceneManager.LoadSceneAsync(ID, LoadSceneMode.Additive);
            }
            else if (backgroundType == BackgroundType.Prefab)
            {
                if (!forceSync && asyncLoading)
                    operation = Resources.LoadAsync<Background>(prefabPath + ID);
                else
                    operation = new DummyResourceRequest(Resources.Load<Background>(prefabPath + ID));
            }

            return operation;
        }

        /// <summary>
        /// Removes the current Background
        /// </summary>
        private AsyncOperation RemoveCurrentBackground()
        {
            if (currentBackgroundID == null)
                return null;

            AsyncOperation operation = null;

            currentBackground.OnRemove(manager);

            if (backgroundType == BackgroundType.Scene)
            {
                operation = SceneManager.UnloadSceneAsync(currentBackgroundID);
            }
            else if (backgroundType == BackgroundType.Prefab)
            {
                Object.Destroy(currentBackground.gameObject);
            }

            return operation;
        }

        /// <summary>
        /// Callback for changing if the weather effects are enabled in the settings
        /// </summary>
        /// <param name="value">true</param>
        private void OnEnableWeatherEffectsChange(object value)
        {
            enableWeatherEffects = (bool)value;

            if (currentBackground != null)
            {
                currentBackground.SetWeatherEffect(enableWeatherEffects ? currentCachedData.currentWeatherEffect : null);
            }
        }

        public override void OnInitialize()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
                enableWeatherEffects = (bool)settings.Register("BackgroundManager_EnableWeatherEffects",
                    SettingsContainer.SettingsDataType.Bool,
                    OnEnableWeatherEffectsChange);

            RenderSettings.skybox = new Material(skyboxMaterial);
            RenderSettings.skybox.SetFloat("_Lerp",0.0f);
            if(defaultSkybox != null)
            {
                RenderSettings.skybox.SetTexture("_Current", defaultSkybox.skybox);
                RenderSettings.skybox.SetTexture("_Target", defaultSkybox.skybox);
            }
        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {
            if (unloadingBackground)
            {
                if (currentOperation != null && !currentOperation.isDone)
                    return;

                EndBackgroundUnloading();
            }

            if (loadingBackground)
            {
                if (currentOperation != null && !currentOperation.isDone)
                    return;

                EndBackgroundLoading();
            }

            if(lerpSkybox != null && lerpSkybox.lerping)
            {
                RenderSettings.skybox.SetFloat("_Lerp",lerpSkybox.Update());
            }

            if (lerpSunColor != null && lerpSunColor.lerping)
            {
                Color color = lerpSunColor.Update();
                if (currentBackground)
                    currentBackground.SetSunColor(color);
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnSave(JSON json)
        {
            string current = null;

            if (currentBackgroundID != null)
                current = currentBackgroundID;
            else if (cachedNextBackgroundID != null)
                current = cachedNextBackgroundID;

            if (current != null)
            {
                json.Add("backgroundID", current);
            }

            if (currentCachedData != null)
            {
                JSON cacheJson = new JSON();

                if (currentCachedData.currentWeatherEffect != null)
                    cacheJson.Add("currentWeather", currentCachedData.currentWeatherEffect);

                cacheJson.Add("currentLightDirection", currentCachedData.currentLightDirection);

                if (currentCachedData.skyboxData)
                    cacheJson.Add("currentSkybox", currentCachedData.skyboxData.name);

                json.Add("cachedData", cacheJson);
            }
        }

        public override void OnLoad(JSON json)
        {
            cachedNextBackgroundID = null;
            currentBackgroundID = null;

            if (json.ContainsKey("cachedData"))
            {
                JSON cachedData = json.GetJSON("cachedData");
                currentCachedData = new BackgroundData();

                if (cachedData.ContainsKey("currentWeather"))
                    currentCachedData.currentWeatherEffect = cachedData.GetString("currentWeather");

                if (cachedData.ContainsKey("currentLightDirection"))
                    currentCachedData.currentLightDirection = cachedData.GetJArray("currentLightDirection").AsVector3();

                if (cachedData.ContainsKey("currentSkybox"))
                {
                    SetSkybox(Resources.Load<SkyboxData>(skyboxDataPath + cachedData.GetString("currentSkybox")));
                }
            }

            if (json.ContainsKey("backgroundID"))
            {
                cachedNextBackgroundID = json.GetString("backgroundID");
                EndBackgroundUnloading(true);
            }
        }


        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnRegisterInputs()
        {

        }

        public override void OnUnRegisterInputs()
        {

        }

        public override void OnChangeScene()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
                settings.Unregister("BackgroundManager_EnableWeatherEffects", OnEnableWeatherEffectsChange);

            if (currentBackground != null)
            {
                currentBackground.OnRemove(manager);
                // Not optimal
                if (backgroundType == BackgroundType.Scene)
                    SceneManager.UnloadSceneAsync(currentBackgroundID);
            }
        }

        /// <summary>
        /// Represents a dummy ressource request used when loading resources
        /// </summary>
        private class DummyResourceRequest : AsyncOperation
        {
            private Object obj;

            public DummyResourceRequest(Object obj)
            {
                this.obj = obj;
            }

            public Object GetObject()
            {
                return obj;
            }
        }
    }

    /// <summary>
    /// A background's data
    /// </summary>
    [System.Serializable]
    public class BackgroundData
    {
        public string currentWeatherEffect = null;
        public Vector3 currentLightDirection = new Vector3(50, -30, 0);
        public SkyboxData skyboxData = null;
    }
}

