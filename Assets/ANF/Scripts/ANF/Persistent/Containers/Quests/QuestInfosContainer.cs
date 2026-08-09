using ANF.Utils;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Contains informations on quests (no the actual state values, just the general infos)
    /// </summary>
    [System.Serializable]
    public class QuestInfosContainer : DataContainer
    {
        private Dictionary<string, List<QuestInfo>> quests;

        public DataContainer CloneContainer()
        {
            return new QuestInfosContainer()
            {

            };
        }

        /// <summary>
        /// Gets all known quests
        /// </summary>
        /// <returns>All the known quests</returns>
        public Dictionary<string, List<QuestInfo>> GetQuests()
        {
            return quests;
        }

        /// <summary>
		/// Creates a user quest
		/// </summary>
		/// <param name="variableId">The linked variable</param>
		/// <param name="categoryId">The category's id</param>
		/// <param name="maxQuestState">The maximum state value</param>
        public void CreateUserQuest(string variableId, string categoryId, int maxQuestState)
        {
            QuestInfo info = new QuestInfo();
            info.isGlobal = false;
            info.variableID = variableId;
            info.categoryID = categoryId;
            info.maxQuestState = maxQuestState;

            if (quests.TryGetValue(categoryId, out List<QuestInfo> data))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].variableID.Equals(variableId))
                        return;
                }
                data.Add(info);
            }
            else
            {
                List<QuestInfo> newList = new List<QuestInfo>();
                newList.Add(info);
                quests.Add(categoryId, newList);
            }
        }

        /// <summary>
		/// Removes a user quest
		/// </summary>
		/// <param name="variableId">The linked variable</param>
		/// <param name="categoryId">The category's id</param>
        public void RemoveUserQuest(string variableId, string categoryId)
        {
            if (quests.TryGetValue(categoryId, out List<QuestInfo> data))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].variableID.Equals(variableId))
                    {
                        data.RemoveAt(i);
                        if (data.Count == 0)
                            quests.Remove(categoryId);
                        return;
                    }
                }
            }
        }


        public void Initialize(ANFSettings settings)
        {
            quests = new Dictionary<string, List<QuestInfo>>();

            string currentCategory = null;
            List<QuestInfo> currentList = null;

            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "quests")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    if (line.StartsWith('['))
                    {
                        string[] split = line.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length == 1)
                        {
                            if (!quests.ContainsKey(split[0]))
                            {
                                currentCategory = split[0];
                                currentList = new List<QuestInfo>();
                                quests.Add(split[0], currentList);
                            }
                        }
                    }
                    else if (currentList != null)
                    {
                        string[] split = line.Split(' ');

                        if (split.Length == 2 && int.TryParse(split[1], out int maxState))
                        {
                            QuestInfo questInfo = new QuestInfo();
                            questInfo.variableID = split[0];
                            questInfo.maxQuestState = maxState;
                            questInfo.categoryID = currentCategory;
                            questInfo.isGlobal = true;

                            currentList.Add(questInfo);
                        }
                    }
                }
            }
        }

        public void Save(JSON json)
        {
            JArray array = new JArray();

            foreach (var data in quests.Values)
            {
                foreach (QuestInfo info in data)
                {
                    if (!info.isGlobal)
                    {
                        JSON obj = new JSON();
                        obj.Add("variableId", info.variableID);
                        obj.Add("categoryId", info.categoryID);
                        obj.Add("maxQuestState", info.maxQuestState);
                        array.Add(obj);
                    }
                }
                json.Add("localQuests", array);
            }
        }

        public void Load(JSON json)
        {
            Reset();

            if (json.ContainsKey("localQuests"))
            {
                JArray array = json.GetJArray("localQuests");
                for (int i = 0; i < array.Length; i++)
                {
                    JSON itemJson = array.GetJSON(i);

                    if (itemJson.ContainsKey("variableId") && itemJson.ContainsKey("categoryId") && itemJson.ContainsKey("maxQuestState"))
                        CreateUserQuest(itemJson.GetString("variableId"), itemJson.GetString("categoryId"), itemJson.GetInt("maxQuestState"));
                }
            }
        }

        public void Reset()
        {
            List<string> keys = new List<string>(quests.Keys);

            foreach (string key in keys)
            {
                List<QuestInfo> list = quests[key];
                int i = list.Count - 1;
                while (i >= 0)
                {
                    if (!list[i].isGlobal)
                        list.RemoveAt(i);
                    i--;
                }

                if (list.Count == 0)
                    quests.Remove(key);
            }
        }
    }
}
