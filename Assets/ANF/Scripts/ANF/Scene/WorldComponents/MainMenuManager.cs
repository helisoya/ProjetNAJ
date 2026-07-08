using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
        [SerializeField] private string fadeAllName = "fadeAll";
        [SerializeField] private string defaultBackground = "";
        [SerializeField] private string defaultWeather = "";
        [SerializeField] private string defaultSkybox = "";
        [SerializeField] private MainMenuBackgroundType backgroundType = MainMenuBackgroundType.UseAutosave;


        public override WorldComponent CloneComponent()
        {
            return new MainMenuManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                fadeAllName = fadeAllName,
                defaultBackground = defaultBackground,
                backgroundType = backgroundType
            };
        }



        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            if (manager.GetWorld().GetComponent(out BackgroundManager backgroundManager))
            {
                string selectedBackground = defaultBackground;
                string selectedSkybox = defaultSkybox;
                string selectedWeather = defaultWeather;

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
                    backgroundManager.SetSkybox(selectedSkybox);
                    backgroundManager.SetWeatherEffect(selectedWeather);
                }
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnPaused()
        {
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

        public override void OnLoad(JSON json)
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
        }
    }

}
