using System.Collections.Generic;
using ANF.GUI;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.Rendering;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the post process's volumes
    /// </summary>
    [System.Serializable]
    public class PostProcessManager : WorldComponent
    {
        /// <summary>
		/// Represents a volume's default data
		/// </summary>
        [System.Serializable]
        public struct VolumeDefaultData
        {
            public string id;
            public VolumeProfile volume;

            [Range(0.0f, 1.0f)]
            public float defaultValue;
        }

        /// <summary>
		/// A volume's data
		/// </summary>
        public class VolumeData
        {
            public LerpInstanceFloat lerp;
            public float currentValue;
            public Volume currentVolume;
        }


        [Header("Infos")]
        [SerializeField] private VolumeDefaultData[] defaultData;
        private Dictionary<string, VolumeData> volumeData;
        private bool skipModeEnabled;

        /// <summary>
		/// Checks if a volume is currently lerping
		/// </summary>
		/// <param name="volumeName">The volume's name</param>
		/// <returns>True if it is lerping</returns>
        public bool IsLerpingVolume(string volumeName)
        {
            if (volumeData.TryGetValue(volumeName, out VolumeData data))
                return data.lerp.lerping;
            return false;
        }


        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            foreach (VolumeData data in volumeData.Values)
            {
                data.lerp.ChangeDuration(0.1f);
            }
        }

        public override WorldComponent CloneComponent()
        {
            return new PostProcessManager()
            {
                defaultData = defaultData
            };
        }

        public override void OnInitialize()
        {
            volumeData = new Dictionary<string, VolumeData>();
            GameObject root = new GameObject("PostProcess");
            root.transform.SetParent(manager.transform);

            for (int i = 0; i < defaultData.Length; i++)
            {
                Volume linkedVolume = root.AddComponent<Volume>();
                linkedVolume.weight = defaultData[i].defaultValue;
                linkedVolume.profile = defaultData[i].volume;
                linkedVolume.isGlobal = true;
                linkedVolume.priority = i;

                volumeData.Add(defaultData[i].id, new VolumeData()
                {
                    lerp = new LerpInstanceFloat(),
                    currentValue = linkedVolume.weight,
                    currentVolume = linkedVolume
                });
            }
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            foreach (VolumeData data in volumeData.Values)
            {
                if (data.lerp.lerping)
                {
                    data.currentValue = data.lerp.Update();
                    data.currentVolume.weight = data.currentValue;
                }
            }
        }

        /// <summary>
        /// Sets a volume's weight. Can be immediate or over time
        /// </summary>
        /// <param name="volumeName">The volume</param>
        /// <param name="weight">The new weight</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The lerp's duration if not immediate</param>
        public void SetVolume(string volumeName, float weight, bool immediate = true, float duration = 1.0f)
        {
            if (!volumeData.TryGetValue(volumeName, out VolumeData data))
                return;

            if (immediate)
            {
                data.currentValue = weight;
                data.lerp.StopLerp();
                data.currentVolume.weight = weight;
            }
            else
            {
                data.lerp.StartLerp(data.currentValue, weight, skipModeEnabled ? 0.1f : duration);
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override bool OnLoad(JSON json)
        {
            if (json.ContainsKey("volumes"))
            {
                JSON keys = json.GetJSON("volumes");
                foreach (string key in keys.Keys)
                {
                    if (volumeData.TryGetValue(key, out VolumeData data))
                    {
                        JSON volumeData = keys.GetJSON(key);
                        if (volumeData.ContainsKey("currentValue"))
                            data.currentValue = volumeData.GetFloat("currentValue");
                        if (volumeData.ContainsKey("lerp"))
                            data.lerp.Load(volumeData.GetJSON("lerp"));
                        data.currentVolume.weight = data.currentValue;
                    }
                }
            }

            return true;
        }

        public override void OnSave(JSON json)
        {
            JSON volumesJson = new JSON();

            foreach (string key in volumeData.Keys)
            {
                JSON dataJson = new JSON();
                dataJson.Add("currentValue", volumeData[key].currentValue);

                JSON jsonLerp = new JSON();
                volumeData[key].lerp.Save(jsonLerp);
                dataJson.Add("lerp", jsonLerp);

                volumesJson.Add(key, dataJson);
            }

            json.Add("volumes", volumesJson);
        }


        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnRegisterInputs()
        {

        }

        public override void OnUnRegisterInputs()
        {

        }

        public override bool OnChangeScene()
        {
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }
    }
}
