using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the logs (in game definitions for the user to read, not the history of dialogs)
    /// </summary>
    [System.Serializable]
    public class LogsContainer : DataContainer
    {
        [Header("Logs")]
        [SerializeField] private string pathToImagesInResources;
        private List<string> allLogs;
        private bool[] knownLogs;

        public DataContainer CloneContainer()
        {
            return new LogsContainer()
            {
                pathToImagesInResources = pathToImagesInResources
            };
        }

        public void Initialize(ANFSettings settings)
        {
            allLogs = new List<string>();
            LoadAllLogs(settings);
            knownLogs = new bool[allLogs.Count];
            Reset();
        }

        /// <summary>
        /// Unlock a log if possible
        /// </summary>
        /// <param name="log">The log to unlock</param>
        public void UnlockLog(string log)
        {
            int idx = allLogs.IndexOf(log);
            if (idx != -1)
                knownLogs[idx] = true;
        }

        /// <summary>
        /// Gets a log's sprite
        /// </summary>
        /// <param name="logID">The log's ID</param>
        /// <returns>The log's sprite if found</returns>
        public Sprite GetLogSprite(string logID)
        {
            return Resources.Load<Sprite>(pathToImagesInResources + logID);
        }

        /// <summary>
        /// Gets the list of all logs (known and unknown)
        /// </summary>
        /// <returns>The list of logs</returns>
        public List<string> GetAllLogs()
        {
            return allLogs;
        }

        /// <summary>
        /// Check if a log is unlocked
        /// </summary>
        /// <param name="logID">The log's ID</param>
        /// <returns>True if unlocked</returns>
        public bool IsUnlocked(string logID)
        {
            int idx = allLogs.IndexOf(logID);
            if (idx != -1)
                return IsUnlocked(idx);

            return false;
        }

        /// <summary>
        /// Checks if a log is unlocked
        /// </summary>
        /// <param name="logIndex">The log's index</param>
        /// <returns>True if unlocked</returns>
        public bool IsUnlocked(int logIndex)
        {
            if (logIndex >= 0 && logIndex < knownLogs.Length)
                return knownLogs[logIndex];
            return false;
        }

        /// <summary>
        /// Loads the known logs
        /// <paramref name="settings"/>The ANF Settings</param>
        /// </summary>
        private void LoadAllLogs(ANFSettings settings)
        {
            allLogs = new List<string>();

            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "logs")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    allLogs.Add(line);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < knownLogs.Length; i++)
                knownLogs[i] = false;
        }

        public void Save(JSON json)
        {
            JSON array = new JSON();
            for (int i = 0; i < allLogs.Count; i++)
                array.Add(allLogs[i], knownLogs[i]);

            json.Add("knownLogs", array);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("knownLogs"))
            {
                JSON array = json.GetJSON("knownLogs");
                Reset();

                for (int i = 0; i < allLogs.Count; i++)
                    if (json.ContainsKey(allLogs[i]))
                        knownLogs[i] = json.GetBool(allLogs[i]);
            }
        }
    }
}

