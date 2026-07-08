using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Contains the history of past dialogs
    /// </summary>
    [System.Serializable]
    public class HistoryContainer : DataContainer
    {
        [SerializeField] private int maxDialogs = 50;
        private List<HistoryData> data;

        public DataContainer CloneContainer()
        {
            return new HistoryContainer()
            {
                maxDialogs = maxDialogs
            };
        }

        public void Initialize(ANFSettings settings)
        {
            data = new List<HistoryData>();
        }

        public void Reset()
        {
            data.Clear();
        }

        /// <summary>
        /// Adds a dialog to the history
        /// </summary>
        /// <param name="dialogKey">The dialog's key</param>
        /// <param name="speakerKey">The speaker's key</param>
        public void AddDialog(string dialogKey, string speakerKey)
        {
            data.Insert(0, new HistoryData
            {
                dialogKey = dialogKey,
                speakerKey = speakerKey
            });

            if (data.Count > maxDialogs)
                data.RemoveRange(maxDialogs - 1, data.Count - maxDialogs);
        }

        /// <summary>
        /// Gets the history data
        /// </summary>
        /// <returns>The history</returns>
        public List<HistoryData> GetHistory()
        {
            return data;
        }

        public void Save(JSON json)
        {
            JArray array = new JArray();
            JSON tmpJson;
            foreach (HistoryData dialog in data)
            {
                tmpJson = new JSON();
                tmpJson.Add("speakerKey", dialog.speakerKey);
                tmpJson.Add("dialogKey", dialog.dialogKey);
                array.Add(tmpJson);
            }
            json.Add("data", array);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("data"))
            {
                data.Clear();
                JArray array = json.GetJArray("data");
                for (int i = 0; i < array.Length; i++)
                {
                    JSON tmpJSON = array.GetJSON(i);
                    string speakerKey = null;
                    string dialogKey = null;

                    if (tmpJSON.ContainsKey("speakerKey"))
                        speakerKey = tmpJSON.GetString("speakerKey");
                    if (tmpJSON.ContainsKey("dialogKey"))
                        dialogKey = tmpJSON.GetString("dialogKey");

                    data.Add(new HistoryData()
                    {
                        dialogKey = dialogKey,
                        speakerKey = speakerKey,
                    });
                }
            }
        }
    }

    /// <summary>
    /// Represents a dialog saved in the history container
    /// </summary>
    public struct HistoryData
    {
        public string dialogKey;
        public string speakerKey;
    }
}

