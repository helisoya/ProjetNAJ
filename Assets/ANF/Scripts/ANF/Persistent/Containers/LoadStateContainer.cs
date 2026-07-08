using Leguar.TotalJSON;

namespace ANF.Persistent
{
    /// <summary>
	/// Contains the current loading orders for the game.
	/// </summary>
    [System.Serializable]
    public class LoadStateContainer : DataContainer
    {
        private string defaultStartingScript = null;
        private string scriptToLoad = null;
        private string saveFileToLoad = null;

        public bool loadingASaveFile { get { return saveFileToLoad != null; } }

        public DataContainer CloneContainer()
        {
            return new LoadStateContainer();
        }

        public void Initialize(ANFSettings settings)
        {
            defaultStartingScript = settings.startingScript;
            SetToLoadDefaultScript();
        }

        /// <summary>
		/// Gets the script to load
		/// </summary>
		/// <returns>The script to load</returns>
        public string GetScriptToLoad()
        {
            return scriptToLoad;
        }

        /// <summary>
		/// Gets the savefile to load
		/// </summary>
		/// <returns>The savefile</returns>
        public string GetSaveFileToLoad()
        {
            return saveFileToLoad;
        }

        /// <summary>
		/// Sets the loader to load a save file
		/// </summary>
		/// <param name="saveFileName">The save file's name</param>
        public void SetToLoadSaveFile(string saveFileName)
        {
            saveFileToLoad = saveFileName;
            scriptToLoad = null;
        }

        /// <summary>
		/// Sets the loader to load a script file
		/// </summary>
		/// <param name="scriptPath">The script's path</param>
        public void SetToLoadScript(string scriptPath)
        {
            saveFileToLoad = null;
            scriptToLoad = scriptPath;
        }

        /// <summary>
		/// Sets the loader to load the default script file
		/// </summary>
        public void SetToLoadDefaultScript()
        {
            SetToLoadScript(defaultStartingScript);
        }

        public void Reset()
        {
            scriptToLoad = null;
            saveFileToLoad = null;
        }

        public void Load(JSON json)
        {

        }

        public void Save(JSON json)
        {
        }
    }
}

