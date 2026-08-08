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

                    if (split.Length < 2 || split.Length > 5)
                        continue;

                    switch (split[0].ToLower())
                    {
                        case "evidence":
                            NAJCaseEvidence evidence = new NAJCaseEvidence();
                            evidence.id = split[1];
                            evidence.canCheck = false;
                            evidence.isGlobal = true;

                            if (split.Length >= 3)
                            {
                                // Check if n°3 is a number of addional images, or a spritesheet

                                if (!uint.TryParse(split[2], out evidence.checkImagesCount))
                                {
                                    evidence.iconSpriteSheet = split[2];
                                }
                                else
                                {
                                    evidence.canCheck = true;
                                    evidence.iconSpriteSheet = null;
                                }
                            }

                            if (split.Length >= 4)
                            {
                                if (evidence.canCheck)
                                {
                                    if (split.Length == 5)
                                    {
                                        // One too many parameters
                                        continue;
                                    }

                                    // n°3 was a number of images
                                    evidence.checkImagesSpriteSheet = split[3];
                                }
                                else
                                {
                                    // Could be Number of images

                                    if (!uint.TryParse(split[3], out evidence.checkImagesCount))
                                    {
                                        // Should not happen, this means a sprite sheet was linked to no images
                                        continue;
                                    }
                                    else
                                    {
                                        evidence.canCheck = true;
                                    }
                                }
                            }

                            if (split.Length == 5)
                            {
                                // n°5 can only be a spritesheet
                                evidence.checkImagesSpriteSheet = split[4];
                            }

                            knownEvidence.Add(evidence.id, evidence);

                            break;
                        case "profile":
                            if (split.Length > 3)
                                continue;

                            NAJCaseProfile profile = new NAJCaseProfile();
                            profile.id = split[1];
                            profile.isGlobal = true;

                            if (split.Length == 3)
                                profile.iconSpriteSheet = split[2];
                            else
                                profile.iconSpriteSheet = null;

                            knownProfiles.Add(profile.id, profile);
                            break;
                    }
                }
            }
        }

        /// <summary>
		/// Create a user evidence (local to a save file)
		/// </summary>
		/// <param name="evidenceId">The evidence's id</param>
		/// <param name="checkImageCount">The check image count</param>
		/// <param name="iconSpritesheet">The icon's sprite sheet</param>
		/// <param name="checkImageSpritesheet">The check image's sprite sheet</param>
        public void CreateUserEvidence(string evidenceId, uint checkImageCount, string iconSpritesheet, string checkImageSpritesheet)
        {
            if (!knownEvidence.ContainsKey(evidenceId))
            {
                NAJCaseEvidence evidence = new NAJCaseEvidence();
                evidence.id = evidenceId;
                evidence.isGlobal = false;
                evidence.iconSpriteSheet = iconSpritesheet;
                evidence.checkImagesSpriteSheet = checkImageSpritesheet;
                evidence.checkImagesCount = checkImageCount;
                evidence.canCheck = checkImageCount != 0;
                knownEvidence.Add(evidenceId, evidence);
            }
        }

        /// <summary>
		/// Deletes a user evidence
		/// </summary>
		/// <param name="evidenceId">The evidence's id</param>
        public void DeleteUserEvidence(string evidenceId)
        {
            if (knownEvidence.TryGetValue(evidenceId, out NAJCaseEvidence evidence) && !evidence.isGlobal)
                knownEvidence.Remove(evidenceId);
        }

        /// <summary>
        /// Create a user profile (local to a save file)
        /// </summary>
        /// <param name="profileId">The profile's id</param>
        /// <param name="iconSpritesheet">The icon's sprite sheet</param>
        public void CreateUserProfile(string profileId, string iconSpritesheet)
        {
            if (!knownProfiles.ContainsKey(profileId))
            {
                NAJCaseProfile profile = new NAJCaseProfile();
                profile.id = profileId;
                profile.isGlobal = false;
                profile.iconSpriteSheet = iconSpritesheet;
                knownProfiles.Add(profileId, profile);
            }
        }

        /// <summary>
		/// Deletes a user profile
		/// </summary>
		/// <param name="profileId">The profile's id</param>
        public void DeleteUserProfile(string profileId)
        {
            if (knownProfiles.TryGetValue(profileId, out NAJCaseProfile profile) && !profile.isGlobal)
                knownProfiles.Remove(profileId);
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

            List<string> toDelete = new List<string>();
            foreach (string key in knownEvidence.Keys)
            {
                if (!knownEvidence[key].isGlobal)
                    toDelete.Add(key);
            }
            foreach (string key in toDelete)
                knownEvidence.Remove(key);

            toDelete.Clear();
            foreach (string key in knownProfiles.Keys)
            {
                if (!knownProfiles[key].isGlobal)
                    toDelete.Add(key);
            }
            foreach (string key in toDelete)
                knownProfiles.Remove(key);
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

            JArray localItems = new JArray();
            foreach (NAJCaseEvidence evidence in knownEvidence.Values)
            {
                if (!evidence.isGlobal)
                {
                    JSON itemData = new JSON();
                    itemData.Add("id", evidence.id);
                    itemData.Add("canCheck", evidence.canCheck);
                    itemData.Add("checkImagesCount", evidence.checkImagesCount);

                    if (!string.IsNullOrEmpty(evidence.iconSpriteSheet))
                        itemData.Add("iconSpriteSheet", evidence.iconSpriteSheet);

                    if (!string.IsNullOrEmpty(evidence.checkImagesSpriteSheet))
                        itemData.Add("checkImagesSpriteSheet", evidence.checkImagesSpriteSheet);

                    localItems.Add(itemData);
                }
            }
            if (localItems.Length > 0)
                json.Add("localEvidence", localItems);

            localItems = new JArray();
            foreach (NAJCaseProfile profile in knownProfiles.Values)
            {
                if (!profile.isGlobal)
                {
                    JSON itemData = new JSON();
                    itemData.Add("id", profile.id);

                    if (!string.IsNullOrEmpty(profile.iconSpriteSheet))
                        itemData.Add("iconSpriteSheet", profile.iconSpriteSheet);

                    localItems.Add(itemData);
                }
            }
            if (localItems.Length > 0)
                json.Add("localProfile", localItems);
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

            if (json.ContainsKey("localEvidence"))
            {
                JArray array = json.GetJArray("localEvidence");
                for (int i = 0; i < array.Length; i++)
                {
                    JSON itemData = array.GetJSON(i);
                    NAJCaseEvidence evidence = new NAJCaseEvidence();
                    evidence.isGlobal = false;

                    if (itemData.ContainsKey("id"))
                        evidence.id = itemData.GetString("id");
                    if (itemData.ContainsKey("canCheck"))
                        evidence.canCheck = itemData.GetBool("canCheck");
                    if (itemData.ContainsKey("checkImagesCount"))
                        evidence.checkImagesCount = itemData.GetJNumber("checkImagesCount").AsUInt();
                    if (itemData.ContainsKey("iconSpriteSheet"))
                        evidence.iconSpriteSheet = itemData.GetString("iconSpriteSheet");
                    if (itemData.ContainsKey("checkImagesSpriteSheet"))
                        evidence.checkImagesSpriteSheet = itemData.GetString("checkImagesSpriteSheet");

                    if (!string.IsNullOrEmpty(evidence.id) && !knownEvidence.ContainsKey(evidence.id))
                        knownEvidence.Add(evidence.id, evidence);
                }
            }

            if (json.ContainsKey("localProfile"))
            {
                JArray array = json.GetJArray("localProfile");
                for (int i = 0; i < array.Length; i++)
                {
                    JSON itemData = array.GetJSON(i);
                    NAJCaseProfile profile = new NAJCaseProfile();
                    profile.isGlobal = false;

                    if (itemData.ContainsKey("id"))
                        profile.id = itemData.GetString("id");
                    if (itemData.ContainsKey("iconSpriteSheet"))
                        profile.iconSpriteSheet = itemData.GetString("iconSpriteSheet");

                    if (!string.IsNullOrEmpty(profile.id) && !knownProfiles.ContainsKey(profile.id))
                        knownProfiles.Add(profile.id, profile);
                }
            }
        }
    }
}

