using System.Collections.Generic;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANF.Persistent
{
    /// <summary>
	/// Handles the ANF Save System, load/save files from here
	/// </summary>
    public class ANFSaveSystem
    {
        private string saveFolder;
        private int saveSlotsAmount;
        private string savePath;

        private SaveSlot[] slots;

        /// <summary>
		/// Initialize the save system
		/// </summary>
		/// <param name="settings">The save system's settings</param>
        public void Initialize(ANFSettings settings)
        {
            saveFolder = settings.saveFolder;
            saveSlotsAmount = settings.saveSlotsAmount;

            savePath = Utils.FileManager.savPath + saveFolder;

            slots = new SaveSlot[saveSlotsAmount + 1];
            PersistentDataManager.instance.GetPlayerData().GetComponent(out ResourceManager resourceManager);

            GenerateSaveSlotMetadata("autosave", 0, true, resourceManager);

            for (int i = 0; i < saveSlotsAmount; i++)
            {
                GenerateSaveSlotMetadata(i.ToString(), i + 1, false, resourceManager);
            }
        }

        /// <summary>
		/// Generates a save slot's metadata
		/// </summary>
		/// <param name="slotName">The slot's name</param>
		/// <param name="slotIndex">The slot's index</param>
		/// <param name="isAutoSave">True if the slot is an autosave slot</param>
		/// <param name="resourceManager">The resource manager</param>
        private void GenerateSaveSlotMetadata(string slotName, int slotIndex, bool isAutoSave, ResourceManager resourceManager)
        {
            SaveSlot slot = new SaveSlot();
            slot.slotName = slotName;
            slot.fullPath = $"{savePath}{slotName}.json";
            slot.isAutosave = isAutoSave;

            JSON json = SaveUtils.LoadJSON(slot.fullPath);
            if (json == null)
            {
                slot.existsOnDisk = false;
                slot.isCorrupted = false;
            }
            else
            {
                slot.existsOnDisk = true;
                slot.isCorrupted = false;

                try
                {
                    if (resourceManager != null)
                    {
                        JArray array = json.GetJSON("worldData").GetJSON("world").GetJSON("ANF.ANSL.ANSLManager").GetJArray("contexts");
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array.GetJSON(i).GetBool("currentFileHasChecksum"))
                            {
                                string path = array.GetJSON(i).GetString("currentFilePath");

                                if (!array.GetJSON(i).ContainsKey("currentChecksum"))
                                {
                                    slot.isCorrupted = true;
                                    break;
                                }

                                string checksum = array.GetJSON(i).GetString("currentChecksum");

                                TextAsset asset = resourceManager.GetResource<TextAsset>(path);


                                if (asset)
                                {
                                    List<string> lines = FileManager.ReadTextAsset(asset);
                                    if (lines.Count > 0 && lines[0].StartsWith("#") &&
                                        !lines[0].Equals(checksum))
                                        slot.isCorrupted = true;

                                    asset = null;
                                    resourceManager.ReleaseResource<TextAsset>(path);
                                }
                                else
                                {
                                    // Missing script
                                    slot.isCorrupted = true;
                                }

                                if (slot.isCorrupted)
                                    break;
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            json = null;

            slots[slotIndex] = slot;

#if UNITY_EDITOR
            Debug.Log($"Slot {slotIndex} : [Name: {slot.slotName}, Path: {slot.fullPath}," +
                $"OnDisk: {slot.existsOnDisk}, Autosave: {slot.isAutosave}, Corrupted: {slot.isCorrupted}]");
#endif
        }

        /// <summary>
		/// Gets the available save slots
		/// </summary>
		/// <returns>The save slots</returns>
        public SaveSlot[] GetSlots()
        {
            return slots;
        }


        /// <summary>
        /// Save to a valid save slot
        /// </summary>
        /// <param name="manager">The ANF Scene Manager</param>
        /// <param name="saveSlot">The Save slot</param>
        /// <returns>True if the operation succeded</returns>
        public bool Save(ANFManager manager, string saveSlot)
        {
            foreach (SaveSlot slot in slots)
            {
                if (slot.slotName.Equals(saveSlot))
                {
                    bool result = SaveUtils.SavePlayerData(PersistentDataManager.instance.GetPlayerData(),
                        manager,
                        SceneManager.GetActiveScene().name,
                        slot.fullPath);

                    if (result)
                    {
                        slot.isCorrupted = false;
                        slot.existsOnDisk = true;
                    }

                    return result;
                }
            }

            return false;
        }

        /// <summary>
		/// Loads a save file from a save slot
		/// </summary>
		/// <param name="manager">The ANF Scen Manager</param>
		/// <param name="saveSlot">The save slot</param>
        public bool Load(ANFManager manager, string saveSlot)
        {
            foreach (SaveSlot slot in slots)
            {
                if (slot.slotName.Equals(saveSlot))
                {
                    bool result = SaveUtils.LoadPlayerData(PersistentDataManager.instance.GetPlayerData(),
                        manager,
                        slot.fullPath);

                    return result;
                }
            }

            return false;
        }

        /// <summary>
		/// Represents a save slot
		/// </summary>
        public class SaveSlot
        {
            public string slotName;
            public bool isAutosave;
            public bool isCorrupted;
            public bool existsOnDisk;
            public string fullPath;
        }
    }
}
