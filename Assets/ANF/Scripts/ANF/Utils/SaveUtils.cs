using ANF.Persistent;
using ANF.Scene;
using Leguar.TotalJSON;
using System.Collections.Generic;
using System.IO;

namespace ANF.Utils
{
    /// <summary>
    /// Contains various functions for saving and loading game states
    /// </summary>
    public class SaveUtils
    {

        /// <summary>
        /// Checks if a file exists on disk.
        /// The filepath should only be the part inside the save folder (Ex: Saves/global.json, which will translate to Assets/Saves/global.json in the editor)
        /// </summary>
        /// <param name="filepath">The filepath inside the save</param>
        /// <returns>True if the save file exists</returns>
        public static bool FileExists(string filepath)
        {
            return File.Exists(filepath);
        }

        /// <summary>
		/// Computes a save path
		/// </summary>
		/// <param name="saveName">The save's name</param>
		/// <param name="saveFolder">The save folder</param>
		/// <returns></returns>
        public static string GetSavePath(string saveName, string saveFolder)
        {
            return FileManager.savPath + saveFolder + saveName + ".json";
        }

        /// <summary>
        /// Loads a JSON
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static JSON LoadJSON(string filepath)
        {
            if (!File.Exists(filepath))
                return null;

            List<string> lines = FileManager.LoadFile(filepath);
            string result = "";
            foreach (string line in lines)
                result += line;

            try
            {
                return JSON.ParseString(result);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the global data to disk
        /// </summary>
        /// <param name="globalData">The global data manager</param>
        /// <param name="anfInput">The ANF Input System</parama>
        /// <param name="savePath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool SaveGlobalData(ContainerManager globalData, ANFInput anfInput, string savePath)
        {
            JSON json = new JSON();

            JSON globalDataJson = new JSON();
            globalData.Save(globalDataJson);
            json.Add("globalData", globalDataJson);

            JSON inputDataJson = new JSON();
            anfInput.Save(inputDataJson);
            json.Add("inputData", inputDataJson);

            FileManager.SaveFile(savePath, json.CreateString());

            return true;
        }

        /// <summary>
        /// Loads the global data from disk
        /// </summary>
        /// <param name="globalData">The global data manager</param>
        /// <param name="anfInput">The ANF Input System</parama>
        /// <param name="loadPath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool LoadGlobalData(ContainerManager globalData, ANFInput anfInput, string loadPath)
        {
            JSON loadedJSON = LoadJSON(loadPath);
            if (loadedJSON == null)
                return false;

            if (loadedJSON.ContainsKey("globalData"))
                globalData.Load(loadedJSON.GetJSON("globalData"));

            if (loadedJSON.ContainsKey("inputData"))
                anfInput.Load(loadedJSON.GetJSON("inputData"));

            return true;
        }


        /// <summary>
        /// Saves the player & world data to disk
        /// </summary>
        /// <param name="playerData">The player data</param>
        /// <param name="anfManager">The ANF Manager</param>
        /// <param name="currentScene">The current scene</param>
        /// <param name="savePath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool SavePlayerData(ContainerManager playerData, ANFManager anfManager, string currentScene, string savePath)
        {
            JSON json = new JSON();

            json.Add("currentScene", currentScene);

            JSON partJSON = new JSON();
            playerData.Save(partJSON);
            json.Add("playerData", partJSON);

            partJSON = new JSON();
            anfManager.Save(partJSON);
            json.Add("worldData", partJSON);

            FileManager.SaveFile(savePath, json.CreateString());

            return true;
        }

        /// <summary>
        /// Loads the player & world data from disk
        /// </summary>
        /// <param name="playerData">The player data</param>
        /// <param name="anfManager">The ANF Manager</param>
        /// <param name="loadPath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool LoadPlayerData(ContainerManager playerData, ANFManager anfManager, string loadPath)
        {
            return LoadPlayerData(playerData, anfManager, LoadJSON(loadPath));
        }


        /// <summary>
        /// Loads the player & world data from disk
        /// </summary>
        /// <param name="playerData">The player data</param>
        /// <param name="anfManager">The ANF Manager</param>
        /// <param name="loadPath">The save's filepath</param>
        /// <returns>True if the operation was a success</returns>
        public static bool LoadPlayerData(ContainerManager playerData, ANFManager anfManager, JSON json)
        {
            if (json == null)
                return false;

            if (json.ContainsKey("playerData"))
                playerData.Load(json.GetJSON("playerData"));

            if (json.ContainsKey("worldData"))
                anfManager.Load(json.GetJSON("worldData"));

            return true;
        }
    }
}

