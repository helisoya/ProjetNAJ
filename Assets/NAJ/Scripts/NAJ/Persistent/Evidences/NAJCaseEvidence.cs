using System;
using ANF.Persistent;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace NAJ.Persistent
{
    /// <summary>
	/// Represents an evidence
	/// </summary>
    public struct NAJCaseEvidence
    {
        public string id;
        public bool canCheck;
        public uint checkImagesCount;
        public string iconSpriteSheet;
        public string checkImagesSpriteSheet;
        public bool isGlobal;

        /// <summary>
		/// Gets the key to the evidence's name
		/// </summary>
		/// <returns>The evidence's name key</returns>
        public string GetNameKey()
        {
            return $"Evidence_{id}_Name";
        }

        /// <summary>
        /// Gets the key to the evidence's description
        /// </summary>
        /// <returns>The evidence's description key</returns>
        public string GetDescKey()
        {
            return $"Evidence_{id}_Desc";
        }

        /// <summary>
		/// Gets the key to the evidence's icon
		/// </summary>
		/// <returns>The icon's key</returns>
        public string GetIconKey()
        {
            return $"Evidence_{id}";
        }

        /// <summary>
        /// Gets the key to one of the evidence's additinal image
        /// </summary>
        /// <param name="imageIndex">The image's index</param>
        /// <returns>The image's key</returns>
        public string GetImageKey(uint imageIndex)
        {
            return $"Evidence_{id}_{imageIndex}";
        }

        /// <summary>
        /// Loads the Icon from the resources
        /// </summary>
        /// <param name="resourceManager">The resource Manager</param>
        /// <returns>The icon if found</returns>
        public Sprite LoadIcon(ResourceManager resourceManager)
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                return resourceManager.GetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}");

            return resourceManager.GetSpritesheetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}", iconKey);
        }

        /// <summary>
        /// Unloads the icon from memory
        /// </summary>
        /// <param name="resourceManager">The resource manager</param>
        public void UnloadIcon(ResourceManager resourceManager)
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                resourceManager.ReleaseResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}");
            else
                resourceManager.ReleaseSpritesheetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}", iconKey);
        }

        /// <summary>
        /// Loads the Check Image from the resources
        /// </summary>
        /// <param name="imageIndex"></param>
        /// <param name="resourceManager">The resource manager</param>
        /// <returns>The image if found</returns>
        public Sprite LoadCheckImage(uint imageIndex, ResourceManager resourceManager)
        {
            string imageKey = GetImageKey(imageIndex);

            if (string.IsNullOrEmpty(iconSpriteSheet))
                return resourceManager.GetResource<Sprite>($"Evidence/Check/{imageKey}");

            return resourceManager.GetSpritesheetResource<Sprite>($"Evidence/Check/{checkImagesSpriteSheet}", imageKey);
        }

        /// <summary>
        /// Unloads the Check Image from memory
        /// </summary>
        /// <param name="imageIndex"></param>
        /// <param name="resourceManager">The resource manager</param>
        /// <returns>The image if found</returns>
        public void UnloadCheckImage(uint imageIndex, ResourceManager resourceManager)
        {
            string imageKey = GetImageKey(imageIndex);

            if (string.IsNullOrEmpty(iconSpriteSheet))
                resourceManager.ReleaseResource<Sprite>($"Evidence/Check/{imageKey}");
            else
                resourceManager.ReleaseSpritesheetResource<Sprite>($"Evidence/Check/{checkImagesSpriteSheet}", imageKey);
        }
    }

    /// <summary>
    /// Represents a profile
    /// </summary>
    public struct NAJCaseProfile
    {
        public string id;
        public string iconSpriteSheet;
        public bool isGlobal;

        /// <summary>
		/// Gets the key to the profile's name
		/// </summary>
		/// <returns>The profile's name key</returns>
        public string GetNameKey()
        {
            return $"Profile_{id}_Name";
        }

        /// <summary>
        /// Gets the key to the profile's description
        /// </summary>
        /// <returns>The profile's description key</returns>
        public string GetDescKey()
        {
            return $"Profile_{id}_Desc";
        }

        /// <summary>
        /// Gets the key to the profile's icon
        /// </summary>
        /// <returns>The icon's key</returns>
        public string GetIconKey()
        {
            return $"Profile_{id}";
        }

        /// <summary>
        /// Loads the Icon from the resources
        /// </summary>
        /// <param name="resourceManager">The resource manager</param>
        /// <returns>The icon if found</returns>
        public Sprite LoadIcon(ResourceManager resourceManager)
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                return resourceManager.GetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}");

            return resourceManager.GetSpritesheetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}", iconKey);
        }

        /// <summary>
		/// Unloads the icon from memory
		/// </summary>
		/// <param name="resourceManager">The resource manager</param>
        public void UnloadIcon(ResourceManager resourceManager)
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                resourceManager.ReleaseResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}");
            else
                resourceManager.ReleaseSpritesheetResource<Sprite>($"Evidence/Icons/{iconSpriteSheet}", iconKey);
        }
    }
}

