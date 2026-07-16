using System;
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
            return $"Evidence_{id}_Name";
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
        /// Loads the Icon from the resources folder
        /// </summary>
        /// <returns>The icon if found</returns>
        public Sprite LoadIcon()
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                return Resources.Load<Sprite>("Evidence/Icons/" + iconKey);

            Sprite[] sprites = Resources.LoadAll<Sprite>("Evidence/Icons/" + iconSpriteSheet);

            if (sprites == null || sprites.Length == 0)
                return null;

            foreach (Sprite sprite in sprites)
            {
                if (sprite.name.Equals(iconKey))
                    return sprite;
            }

            return null;
        }

        /// <summary>
        /// Loads the Check Image from the resources folder
        /// </summary>
        /// <returns>The image if found</returns>
        public Sprite LoadCheckImage(uint imageIndex)
        {
            string imageKey = GetImageKey(imageIndex);

            if (string.IsNullOrEmpty(checkImagesSpriteSheet))
                return Resources.Load<Sprite>("Evidence/Check/" + imageKey);

            Sprite[] sprites = Resources.LoadAll<Sprite>("Evidence/Check/" + checkImagesSpriteSheet);

            if (sprites == null || sprites.Length == 0)
                return null;

            foreach (Sprite sprite in sprites)
            {
                if (sprite.name.Equals(imageKey))
                    return sprite;
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a profile
    /// </summary>
    public struct NAJCaseProfile
    {
        public string id;
        public string iconSpriteSheet;

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
            return $"Profile_{id}_Name";
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
        /// Loads the Icon from the resources folder
        /// </summary>
        /// <returns>The icon if found</returns>
        public Sprite LoadIcon()
        {
            string iconKey = GetIconKey();

            if (string.IsNullOrEmpty(iconSpriteSheet))
                return Resources.Load<Sprite>("Evidence/Icons/" + iconKey);

            Sprite[] sprites = Resources.LoadAll<Sprite>("Evidence/Icons/" + iconSpriteSheet);

            if (sprites == null || sprites.Length == 0)
                return null;

            foreach(Sprite sprite in sprites)
            {
                if (sprite.name.Equals(iconKey))
                    return sprite;
            }

            return null;
        }
    }
}

