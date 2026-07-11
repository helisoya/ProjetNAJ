using System.Collections.Generic;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;

namespace NAJ.Persistent
{
    /// <summary>
	/// Handles the game's evidence inventory system.
    /// Technically, you can unlock evidence from all cases, so be careful with it.
	/// </summary>
    [System.Serializable]
    public class NAJCaseInventoryContainer : DataContainer
    {
        private Dictionary<string, NAJCaseEvidence> knownEvidence;
        private List<string> inventory;

        public DataContainer CloneContainer()
        {
            return new NAJCaseInventoryContainer()
            {

            };
        }

        public void Initialize(ANFSettings settings)
        {
            inventory = new List<string>();
            knownEvidence = new Dictionary<string, NAJCaseEvidence>();

            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "evidence")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    string[] split = line.Split(' ');

                    if (split.Length == 0 || split.Length > 2)
                        continue;

                    NAJCaseEvidence evidence = new NAJCaseEvidence();
                    evidence.id = split[0];
                    evidence.canCheck = split.Length == 2;

                    if (evidence.canCheck &&
                    !uint.TryParse(split[1], out evidence.checkImagesCount))
                        continue;

                    knownEvidence.Add(evidence.id, evidence);
                }
            }
        }

        /// <summary>
		/// Removes all evidence from the inventory
		/// </summary>
        public void RemoveAllEvidence()
        {
            inventory.Clear();
        }

        /// <summary>
		/// Removes a evidence from the inventory
		/// </summary>
		/// <param name="evidenceID">The evidence's ID</param>
        public void RemoveEvidence(string evidenceID)
        {
            int index = inventory.IndexOf(evidenceID);
            if (index != -1)
                inventory.RemoveAt(index);
        }

        /// <summary>
		/// Adds a new evidence to the inventory
		/// </summary>
		/// <param name="evidenceID">The evidence's ID</param>
        public void AddEvidence(string evidenceID)
        {
            if (knownEvidence.ContainsKey(evidenceID) && !inventory.Contains(evidenceID))
                inventory.Add(evidenceID);
        }

        /// <summary>
		/// Gets the current player inventory
		/// </summary>
		/// <returns>The unlocked evidence</returns>
        public List<string> GetInventory()
        {
            return inventory;
        }

        /// <summary>
		/// Gets the list of all evidence in the game
		/// </summary>
		/// <returns>Every evidence in the game</returns>
        public Dictionary<string, NAJCaseEvidence> GetKnownEvidence()
        {
            return knownEvidence;
        }

        public void Reset()
        {
            inventory.Clear();
        }

        public void Save(JSON json)
        {
            JArray array = new JArray();
            foreach (string evidence in inventory)
            {
                array.Add(evidence);
            }
            json.Add("inventory", array);
        }

        public void Load(JSON json)
        {
            Reset();

            if (json.ContainsKey("inventory"))
            {
                JArray array = json.GetJArray("inventory");
                for (int i = 0; i < array.Length; i++)
                {
                    inventory.Add(array.GetString(i));
                }
            }
        }
    }
}

