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
        private List<KeyValuePair<bool, string>> allLogs;
        private List<bool> knownLogs;

        public DataContainer CloneContainer()
        {
            return new LogsContainer()
            {
            };
        }

        public void Initialize(ANFSettings settings)
        {
            allLogs = new List<KeyValuePair<bool, string>>();
            knownLogs = new List<bool>();
            LoadAllLogs(settings);
            Reset();
        }

        /// <summary>
		/// Creates a new User Log
		/// </summary>
		/// <param name="logId">The new log's Id</param>
        public void CreateUserLog(string logId)
        {
            for (int i = 0; i < allLogs.Count; i++)
            {
                if (allLogs[i].Value.Equals(logId))
                {
                    return;
                }
            }

            allLogs.Add(new KeyValuePair<bool, string>(false, logId));
            knownLogs.Add(false);
        }

        public void RemoveUserLog(string logId)
        {
            for (int i = 0; i < allLogs.Count; i++)
            {
                if (allLogs[i].Value.Equals(logId) && !allLogs[i].Key)
                {
                    allLogs.RemoveAt(i);
                    knownLogs.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Unlock a log if possible
        /// </summary>
        /// <param name="log">The log to unlock</param>
        public void UnlockLog(string log)
        {
            for (int i = 0; i < allLogs.Count; i++)
            {
                if (allLogs[i].Value.Equals(log))
                {
                    knownLogs[i] = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the list of all logs (known and unknown)
        /// </summary>
        /// <returns>The list of logs</returns>
        public List<KeyValuePair<bool, string>> GetAllLogs()
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
            for (int i = 0; i < allLogs.Count; i++)
            {
                if (allLogs[i].Value.Equals(logID))
                {
                    return IsUnlocked(i);
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a log is unlocked
        /// </summary>
        /// <param name="logIndex">The log's index</param>
        /// <returns>True if unlocked</returns>
        public bool IsUnlocked(int logIndex)
        {
            if (logIndex >= 0 && logIndex < knownLogs.Count)
                return knownLogs[logIndex];
            return false;
        }

        /// <summary>
        /// Loads the known logs
        /// <paramref name="settings"/>The ANF Settings</param>
        /// </summary>
        private void LoadAllLogs(ANFSettings settings)
        {
            allLogs.Clear();
            knownLogs.Clear();

            if (!PersistentDataManager.instance.GetPlayerData().GetComponent(out ResourceManager resourceManager))
                return;

            string fileName = settings.generalDataPath + "logs";

            TextAsset asset = resourceManager.GetResource<TextAsset>(fileName);

            if (!asset)
                return;

            List<string> lines = FileManager.ReadTextAsset(asset);

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    allLogs.Add(new KeyValuePair<bool, string>(true, line));
                    knownLogs.Add(false);
                }
            }

            asset = null;
            resourceManager.ReleaseResource<TextAsset>(fileName);
        }

        public void Reset()
        {
            int i = allLogs.Count - 1;
            while (i >= 0)
            {
                if (!allLogs[i].Key)
                {
                    allLogs.RemoveAt(i);
                    knownLogs.RemoveAt(i);
                }
                else
                {
                    knownLogs[i] = false;
                }
                i--;
            }
        }

        public void Save(JSON json)
        {
            JSON array = new JSON();
            JArray localLogs = new JArray();
            for (int i = 0; i < allLogs.Count; i++)
            {
                array.Add(allLogs[i].Value, knownLogs[i]);
                if (!allLogs[i].Key)
                    localLogs.Add(allLogs[i].Value);
            }

            json.Add("knownLogs", array);
            if (localLogs.Length > 0)
                json.Add("localLogs", localLogs);
        }

        public bool Load(JSON json)
        {
            Reset();

            if (json.ContainsKey("localLogs"))
            {
                JArray array = json.GetJArray("localLogs");
                for (int i = 0; i < array.Length; i++)
                {
                    CreateUserLog(array.GetString(i));
                }
            }

            if (json.ContainsKey("knownLogs"))
            {
                JSON array = json.GetJSON("knownLogs");

                for (int i = 0; i < allLogs.Count; i++)
                    if (array.ContainsKey(allLogs[i].Value))
                        knownLogs[i] = array.GetBool(allLogs[i].Value);
            }

            return true;
        }
    }
}

