using ANF.Utils;
using UnityEngine;

namespace ANF.Persistent
{

    /// <summary>
    /// Represents the internal ANF Settings.
    /// For user settings such as the resolution and fonts, check UserSettings
    /// </summary>
    [CreateAssetMenu(fileName = "ANFSettings", menuName = "ANF/ANFSettings")]
    public class ANFSettings : ScriptableObject
    {
        [Header("General")]
        [Tooltip("The default starting script for the game")]
        public string startingScript;
        [Tooltip("The name of the main menu scene")]
        public string mainMenuScene;
        [Tooltip("The name of the game scene")]
        public string gameScene;


        [Header("Data")]
        [Tooltip("Filepath to the save files (Inside Assets/ in the editor, and inside the game's persistent data folder in build)")]
        public string saveFolder = "Saves/";
        [Tooltip("How many save slot (minus the auto save) should be available ?")]
        public int saveSlotsAmount = 10;
        [Tooltip("Path to the general data files (in Resources/) Ex: variables.txt, maps.txt")]
        public string generalDataPath = "General/";
        [Tooltip("Player datas containers are responsible for datas that are local to a save file (Ex: variables)")]
        public ComponentRegisterEntry<DataContainer>[] registeredPlayerDataContainers;
        [Tooltip("Global datas containers are responsible for datas that are not local to a save file (Ex: settings)")]
        public ComponentRegisterEntry<DataContainer>[] registeredGlobalDataContainers;

        [Header("Additional")]
        [Tooltip("You can add additional settings here. Don't add more than one additional part of each type.  (Ex: Only one ANSLSettings)")]
        [SerializeReference, SubclassSelector(AllowNull = false)] public ANFSettingsAdditionalPart[] additionalParts;

        /// <summary>
        /// Search for a specific additional part in the settings
        /// </summary>
        /// <typeparam name="T">The searched setting's type</typeparam>
        /// <param name="result">The settings if found</param>
        /// <returns>True if found</returns>
        public bool FindAdditionalPart<T>(out T result)
        {
            foreach(ANFSettingsAdditionalPart part in additionalParts)
            {
                if(part.GetType() == typeof(T) || part.GetType().IsSubclassOf(typeof(T)))
                {
                    result = (T)part;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
