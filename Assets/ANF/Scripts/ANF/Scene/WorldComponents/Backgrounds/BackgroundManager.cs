using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANF.Scene
{
    /// <summary>
	/// Represents the terrain quality settings for backgrounds (will affect pixel error)
	/// </summary>
    public enum TerrainQuality
    {
        VeryLow, // 200
        Low, // 100
        Medium, // 25
        High // 1
    }

    /// <summary>
	/// Handles the game's backgrounds.
    /// Backgrounds can be 3D scenes, or 3D prefabs
	/// </summary>
    [System.Serializable]
    public class BackgroundManager : WorldComponent
    {
        [SerializeField] private bool asyncLoading = false;
        [SerializeField] private string skyboxDataPath = "Skyboxes/";
        [SerializeField] private string defaultSkybox = "Day";
        [SerializeField] private Material skyboxMaterial;
        private Background currentBackground;
        private string currentBackgroundID;
        private BackgroundData currentCachedData = null;
        private bool enableWeatherEffects = true;
        private bool enableTerrainFoliage = true;
        private TerrainQuality terrainQuality = TerrainQuality.Medium;
        private bool skipModeEnabled = false;

        private LerpInstanceColor lerpSunColor;
        private LerpInstanceFloat lerpSkybox;

        private AsyncOperation currentOperation;
        private int asyncWaitForNextFrames = 0;
        private string cachedNextBackgroundID;
        public bool loadingBackground { get; private set; }
        public bool unloadingBackground { get; private set; }
        public bool lerpingSkybox
        {
            get
            {
                return (lerpSkybox != null && lerpSkybox.lerping) || (lerpSunColor != null && lerpSunColor.lerping);
            }
        }
        private ResourceManager resourceManager;

        public override WorldComponent CloneComponent()
        {
            return new BackgroundManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                asyncLoading = asyncLoading,
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
            if (!string.IsNullOrEmpty(skyboxName) && resourceManager != null)
            {
                SkyboxData data = resourceManager.GetResource<SkyboxData>(skyboxDataPath + skyboxName);
                if (data != null)
                {
                    SetSkybox(data, immediate, transitionDuration);
                }
            }
        }

        /// <summary>
		/// Releases the last cached skybox data
		/// </summary>
        private void ReleaseLastCachedSkybox()
        {
            if (currentCachedData.lastSkyboxData)
            {
                string name = currentCachedData.lastSkyboxData.name;
                currentCachedData.lastSkyboxData = null;
                if (resourceManager != null)
                {
                    resourceManager.ReleaseResource<SkyboxData>(name);
                }
            }
        }

        /// <summary>
		/// Releases the current cached skybox data
		/// </summary>
        private void ReleaseCurrentCachedSkybox()
        {
            if (currentCachedData.currentSkyboxData)
            {
                string name = currentCachedData.currentSkyboxData.name;
                currentCachedData.currentSkyboxData = null;
                if (resourceManager != null)
                {
                    resourceManager.ReleaseResource<SkyboxData>(name);
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

            if (currentCachedData == null)
            {
                currentCachedData = new BackgroundData();
            }

            if (immediate || currentCachedData.currentSkyboxData == null)
            {
                // Immediate OR No current skybox
                RenderSettings.skybox.SetFloat("_Lerp", 0.0f);
                RenderSettings.skybox.SetTexture("_Current", skyboxData.skybox);
                RenderSettings.skybox.SetTexture("_Target", skyboxData.skybox);

                if (currentBackground)
                    currentBackground.SetSunColor(skyboxData.sunColor);

                ReleaseCurrentCachedSkybox();
                ReleaseLastCachedSkybox();
            }
            else
            {
                RenderSettings.skybox.SetFloat("_Lerp", 0.0f);
                RenderSettings.skybox.SetTexture("_Current", currentCachedData.currentSkyboxData.skybox);
                RenderSettings.skybox.SetTexture("_Target", skyboxData.skybox);

                if (currentCachedData.lastSkyboxData)
                    ReleaseLastCachedSkybox();

                currentCachedData.lastSkyboxData = currentCachedData.currentSkyboxData;
                currentCachedData.currentSkyboxData = null;

                if (lerpSkybox == null)
                    lerpSkybox = new LerpInstanceFloat();

                lerpSkybox.StartLerp(0, 1, skipModeEnabled ? 0.1f : transitionDuration);

                if (lerpSunColor == null)
                    lerpSunColor = new LerpInstanceColor();

                lerpSunColor.StartLerp(currentCachedData.lastSkyboxData.sunColor, skyboxData.sunColor, skipModeEnabled ? 0.1f : transitionDuration);
            }

            currentCachedData.currentSkyboxData = skyboxData;
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
                currentOperation = LoadBackground(cachedNextBackgroundID, forceSync);

                loadingBackground = true;
                asyncWaitForNextFrames = loadingBackground ? 2 : 0;

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


            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(currentBackgroundID);
            if (scene != null)
            {
                GameObject[] rootObjs = scene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjs)
                {
                    currentBackground = rootObj.GetComponent<Background>();
                    if (currentBackground)
                    {
                        currentBackground.OnCreate(manager);

                        if (currentCachedData == null)
                        {
                            BackgroundDefaultData defaultData = currentBackground.GetDefaultData();
                            currentCachedData = new BackgroundData()
                            {
                                currentLightDirection = defaultData.currentLightDirection,
                                currentWeatherEffect = defaultData.currentWeatherEffect,
                                currentSkyboxData = null,
                                lastSkyboxData = null
                            };
                            if (!string.IsNullOrEmpty(defaultData.skyboxData))
                            {
                                SetSkybox(defaultData.skyboxData);
                            }
                        }


                        if (currentCachedData.currentSkyboxData == null)
                        {
                            SetSkybox(defaultSkybox);
                        }

                        currentBackground.SetLightDirection(currentCachedData.currentLightDirection);
                        currentBackground.SetWeatherEffect(currentCachedData.currentWeatherEffect);
                        currentBackground.EnableFoliage(enableTerrainFoliage);
                        currentBackground.SetTerrainQuality(terrainQuality);


                        if (lerpSunColor != null && lerpSunColor.lerping)
                            currentBackground.SetSunColor(lerpSunColor.Get());
                        else
                            currentBackground.SetSunColor(currentCachedData.currentSkyboxData.sunColor);
                        break;
                    }
                }
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

            if (!forceSync && asyncLoading)
                SceneManager.LoadScene(ID, LoadSceneMode.Additive);
            else
                operation = SceneManager.LoadSceneAsync(ID, LoadSceneMode.Additive);

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

            operation = SceneManager.UnloadSceneAsync(currentBackgroundID, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            return operation;
        }

        /// <summary>
        /// Callback for changing if the weather effects are enabled in the settings
        /// </summary>
        /// <param name="value">The new value</param>
        private void OnEnableWeatherEffectsChange(object value)
        {
            enableWeatherEffects = (bool)value;

            if (currentBackground != null)
            {
                currentBackground.SetWeatherEffect(enableWeatherEffects ? currentCachedData.currentWeatherEffect : null);
            }
        }

        /// <summary>
        /// Callback for changing if the terrain foliage is enabled in the settings
        /// </summary>
        /// <param name="value">The new value</param>
        private void OnEnableFoliageChange(object value)
        {
            enableTerrainFoliage = (bool)value;

            if (currentBackground != null)
            {
                currentBackground.EnableFoliage(enableTerrainFoliage);
            }
        }

        /// <summary>
        /// Callback for changing the terrain quality in the settings
        /// </summary>
        /// <param name="value">The new value</param>
        private void OnTerrainQualityChange(object value)
        {
            terrainQuality = (TerrainQuality)value;

            if (currentBackground != null)
            {
                currentBackground.SetTerrainQuality(terrainQuality);
            }
        }

        public override void OnInitialize()
        {
            PersistentDataManager.instance.GetPlayerData().GetComponent(out resourceManager);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                enableWeatherEffects = (bool)settings.Register("BackgroundManager_EnableWeatherEffects",
                    SettingsContainer.SettingsDataType.Bool,
                    OnEnableWeatherEffectsChange);

                enableTerrainFoliage = (bool)settings.Register("BackgroundManager_EnableFoliage",
                    SettingsContainer.SettingsDataType.Bool,
                    OnEnableFoliageChange);

                terrainQuality = (TerrainQuality)settings.Register("BackgroundManager_TerrainQuality",
                    SettingsContainer.SettingsDataType.Int,
                    OnTerrainQualityChange);
            }


            RenderSettings.skybox = new Material(skyboxMaterial);
            RenderSettings.skybox.SetFloat("_Lerp", 0.0f);
            if (!string.IsNullOrEmpty(defaultSkybox))
            {
                SetSkybox(defaultSkybox);
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

                if (asyncWaitForNextFrames > 0)
                {
                    asyncWaitForNextFrames--;
                    return;
                }

                EndBackgroundLoading();
            }

            if (lerpSkybox != null && lerpSkybox.lerping)
            {
                RenderSettings.skybox.SetFloat("_Lerp", lerpSkybox.Update());
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

                if (currentCachedData.currentSkyboxData)
                    cacheJson.Add("currentSkybox", currentCachedData.currentSkyboxData.name);

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
                    SetSkybox(cachedData.GetString("currentSkybox"));
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

        public override bool OnChangeScene()
        {
            RenderSettings.skybox.SetTexture("_Current", null);
            RenderSettings.skybox.SetTexture("_Target", null);
            ReleaseCurrentCachedSkybox();
            ReleaseLastCachedSkybox();
            resourceManager = null;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                settings.Unregister("BackgroundManager_EnableWeatherEffects", OnEnableWeatherEffectsChange);
                settings.Unregister("BackgroundManager_EnableFoliage", OnEnableFoliageChange);
                settings.Unregister("BackgroundManager_TerrainQuality", OnTerrainQualityChange);
            }

            if (currentBackground != null)
            {
                currentBackground.OnRemove(manager);
                // Not optimal
                currentOperation = SceneManager.UnloadSceneAsync(currentBackgroundID);

                unloadingBackground = currentOperation != null && !currentOperation.isDone;
            }

            return !unloadingBackground;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return currentOperation != null && !currentOperation.isDone;
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
        public SkyboxData currentSkyboxData = null;
        public SkyboxData lastSkyboxData = null;
    }

    /// <summary>
    /// A background's default data
    /// </summary>
    [System.Serializable]
    public class BackgroundDefaultData
    {
        public string currentWeatherEffect = null;
        public Vector3 currentLightDirection = new Vector3(50, -30, 0);
        public string skyboxData = "Day";
    }
}

