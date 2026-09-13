using ANF.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.Persistent
{
    /// <summary>
	/// Handles persistent data (Sounds, User data, ...)
	/// </summary>
    public class PersistentDataManager : MonoBehaviour
    {
        public static PersistentDataManager instance { get; private set; }
        public bool hasLoaded { get; private set; }

        [Header("Data")]
        [SerializeField] private ANFSettings anfSettings;
        [SerializeField] private ANFInput anfInput;
        [SerializeField] private ContainerManager playerData;
        [SerializeField] private ContainerManager globalData;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                hasLoaded = false;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (instance == this && !hasLoaded)
            {
                Load();
            }
        }

        /// <summary>
		/// Loads the data manager
		/// </summary>
        public void Load()
        {
            anfInput.Initialize();

            playerData = new ContainerManager(anfSettings.registeredPlayerDataContainers);
            globalData = new ContainerManager(anfSettings.registeredGlobalDataContainers);
            playerData.Initialize(anfSettings);
            globalData.Initialize(anfSettings);

            string globalDataSaveFile = FileManager.savPath + anfSettings.saveFolder + "global.json";
            if (SaveUtils.FileExists(globalDataSaveFile))
                SaveUtils.LoadGlobalData(globalData, anfInput, globalDataSaveFile);
            else
                SaveUtils.SaveGlobalData(globalData, anfInput, globalDataSaveFile);

            hasLoaded = true;
        }

        /// <summary>
		/// Gets the player input
		/// </summary>
		/// <returns>The player input</returns>
        public ANFInput GetANFInput()
        {
            return anfInput;
        }

        /// <summary>
        /// Gets the internal ANF settings
        /// </summary>
        /// <returns>The ANF Settings</returns>
        public ANFSettings GetANFSettings()
        {
            return anfSettings;
        }

        /// <summary>
		/// Gets the player's data (data that is local to a save)
		/// </summary>
		/// <returns>The player's data</returns>
        public ContainerManager GetPlayerData()
        {
            return playerData;
        }

        /// <summary>
        /// Gets the global data (data that is not local to a save, ex: Settings)
        /// </summary>
        /// <returns>The global data</returns>
        public ContainerManager GetGlobalData()
        {
            return globalData;
        }
    }
}
