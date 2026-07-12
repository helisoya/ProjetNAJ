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
        private Dictionary<string, NAJCaseProfile> knownProfiles;
        private List<string> inventoryEvidence;
        private List<string> inventoryProfiles;

        public DataContainer CloneContainer()
        {
            return new NAJCaseInventoryContainer()
            {

            };
        }

        public void Initialize(ANFSettings settings)
        {
            inventoryProfiles = new List<string>();
            inventoryEvidence = new List<string>();
            knownProfiles = new Dictionary<string, NAJCaseProfile>();
            knownEvidence = new Dictionary<string, NAJCaseEvidence>();

            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "evidence")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    string[] split = line.Split(' ');

                    if (split.Length != 2 && split.Length != 3)
                        continue;

                    switch (split[0].ToLower())
                    {
                        case "evidence":
                            NAJCaseEvidence evidence = new NAJCaseEvidence();
                            evidence.id = split[1];
                            evidence.canCheck = split.Length == 3;

                            if (evidence.canCheck &&
                            !uint.TryParse(split[2], out evidence.checkImagesCount))
                                continue;

                            knownEvidence.Add(evidence.id, evidence);

                            break;
                        case "profile":
                            if (split.Length == 2)
                                knownProfiles.Add(split[1], new NAJCaseProfile() { id = split[1] });
                            break;
                    }
                }
            }
        }

        /// <summary>
		/// Removes all evidence from the inventory
		/// </summary>
        public void RemoveAllEvidence()
        {
            inventoryEvidence.Clear();
        }

        /// <summary>
        /// Removes all profiles from the inventory
        /// </summary>
        public void RemoveAllProfiles()
        {
            inventoryProfiles.Clear();
        }

        /// <summary>
		/// Removes a evidence from the inventory
		/// </summary>
		/// <param name="evidenceID">The evidence's ID</param>
        public void RemoveEvidence(string evidenceID)
        {
            int index = inventoryEvidence.IndexOf(evidenceID);
            if (index != -1)
                inventoryEvidence.RemoveAt(index);
        }

        /// <summary>
		/// Adds a new evidence to the inventory
		/// </summary>
		/// <param name="evidenceID">The evidence's ID</param>
        public void AddEvidence(string evidenceID)
        {
            if (knownEvidence.ContainsKey(evidenceID) && !inventoryEvidence.Contains(evidenceID))
                inventoryEvidence.Add(evidenceID);
        }

        /// <summary>
		/// Gets the unlocked evidence
		/// </summary>
		/// <returns>The unlocked evidence</returns>
        public List<string> GetEvidenceInventory()
        {
            return inventoryEvidence;
        }

        /// <summary>
        /// Removes a profile from the inventory
        /// </summary>
        /// <param name="profileID">The profile's ID</param>
        public void RemoveProfile(string profileID)
        {
            int index = inventoryProfiles.IndexOf(profileID);
            if (index != -1)
                inventoryProfiles.RemoveAt(index);
        }

        /// <summary>
		/// Adds a new profile to the inventory
		/// </summary>
		/// <param name="evidenceID">The profile's ID</param>
        public void AddProfile(string profileID)
        {
            if (knownProfiles.ContainsKey(profileID) && !inventoryProfiles.Contains(profileID))
                inventoryProfiles.Add(profileID);
        }

        /// <summary>
		/// Gets the unlocked profiles
		/// </summary>
		/// <returns>The unlocked profiles</returns>
        public List<string> GetProfilesInventory()
        {
            return inventoryProfiles;
        }

        /// <summary>
		/// Gets the list of all evidence in the game
		/// </summary>
		/// <returns>Every evidence in the game</returns>
        public Dictionary<string, NAJCaseEvidence> GetAllEvidence()
        {
            return knownEvidence;
        }

        /// <summary>
        /// Gets the list of all profile in the game
        /// </summary>
        /// <returns>Every profile in the game</returns>
        public Dictionary<string, NAJCaseProfile> GetAllProfiles()
        {
            return knownProfiles;
        }

        public void Reset()
        {
            inventoryProfiles.Clear();
            inventoryEvidence.Clear();
        }

        public void Save(JSON json)
        {
            JArray array;

            if (inventoryEvidence.Count > 0)
            {
                array = new JArray();
                foreach (string evidence in inventoryEvidence)
                {
                    array.Add(evidence);
                }
                json.Add("inventoryEvidence", array);
            }

            if (inventoryProfiles.Count > 0)
            {
                array = new JArray();
                foreach (string profile in inventoryProfiles)
                {
                    array.Add(profile);
                }
                json.Add("inventoryProfiles", array);
            }
        }

        public void Load(JSON json)
        {
            Reset();

            if (json.ContainsKey("inventoryEvidence"))
            {
                JArray array = json.GetJArray("inventoryEvidence");
                for (int i = 0; i < array.Length; i++)
                {
                    inventoryEvidence.Add(array.GetString(i));
                }
            }

            if (json.ContainsKey("inventoryProfiles"))
            {
                JArray array = json.GetJArray("inventoryProfiles");
                for (int i = 0; i < array.Length; i++)
                {
                    inventoryProfiles.Add(array.GetString(i));
                }
            }
        }
    }
}

