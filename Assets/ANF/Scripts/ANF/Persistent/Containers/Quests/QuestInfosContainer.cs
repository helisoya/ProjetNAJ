using ANF.Utils;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
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

                            currentList.Add(questInfo);
                        }
                    }
                }
            }
        }

        public void Save(JSON json)
        {
        }

        public void Load(JSON json)
        {
        }

        public void Reset()
        {
        }
    }
}
