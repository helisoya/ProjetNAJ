using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;
using ANF.GUI;

namespace ANF.Scene
{
    /// <summary>
	/// Handles the world linked to the main menu
	/// </summary>
    [System.Serializable]
    public class MainMenuManager : WorldComponent
    {
        /// <summary>
		/// Stipulate how the default background should be chosen
		/// </summary>
        public enum MainMenuBackgroundType
        {
            UseDefault,
            UseAutosave
        }
        [SerializeField] private string defaultBackground = "";
        [SerializeField] private string defaultWeather = "";
        [SerializeField] private string defaultSkybox = "";
        [SerializeField] private MainMenuBackgroundType backgroundType = MainMenuBackgroundType.UseAutosave;

        private bool waitingForBackgroundLoad = false;
        private BackgroundManager backgroundManager;
        private Fade fade;

        string selectedSkybox;
        string selectedWeather;

        public override WorldComponent CloneComponent()
        {
            return new MainMenuManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                defaultBackground = defaultBackground,
                backgroundType = backgroundType
            };
        }



        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            if (manager.GetGUIManager().GetComponent(out fade))
            {
                fade.FadeAlphaTo(1, true);
            }

            if (manager.GetWorld().GetComponent(out backgroundManager))
            {
                waitingForBackgroundLoad = false;
                string selectedBackground = defaultBackground;
                selectedSkybox = defaultSkybox;
                selectedWeather = defaultWeather;

                if (backgroundType == MainMenuBackgroundType.UseAutosave)
                {
                    string filePath = SaveUtils.GetSavePath("autosave", PersistentDataManager.instance.GetANFSettings().saveFolder);

                    JSON loadedJSON = SaveUtils.LoadJSON(filePath);
                    if (loadedJSON != null)
                    {
                        try
                        {
                            JSON backgroundJSON = loadedJSON.GetJSON("worldData").GetJSON("world").GetJSON(typeof(BackgroundManager).FullName);
                            selectedBackground = backgroundJSON.GetString("backgroundID");
                            selectedSkybox = backgroundJSON.GetString("currentSkybox");
                            selectedWeather = backgroundJSON.GetJSON("cachedData").GetString("currentWeather");
                        }
                        catch
                        {
                            selectedBackground = defaultBackground;
                            selectedSkybox = defaultSkybox;
                            selectedWeather = defaultWeather;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(selectedBackground))
                {
                    backgroundManager.SetBackground(selectedBackground, true);
                    waitingForBackgroundLoad = true;
                }
            }
        }

        public override void OnUpdate()
        {
            if (waitingForBackgroundLoad && backgroundManager != null)
            {
                if (!backgroundManager.loadingBackground)
                {
                    waitingForBackgroundLoad = false;
                    backgroundManager.SetWeatherEffect(selectedWeather);
                    backgroundManager.SetSkybox(selectedSkybox);
                    fade.FadeAlphaTo(0);
                }
                else
                {
                    if (fade.fadingAlpha)
                    {
                        fade.FadeAlphaTo(1, true);
                    }
                    if (backgroundManager.isPaused)
                    {
                        backgroundManager.SetPaused(false);
                    }
                }

            }
        }

        public override void OnPaused()
        {
            SetPaused(false);
        }

        public override void OnUnPaused()
        {
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }

        public override void OnSave(JSON json)
        {
        }

        public override bool OnLoad(JSON json)
        {
            return true;
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override bool OnChangeScene()
        {
            backgroundManager = null;
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }
    }

}
