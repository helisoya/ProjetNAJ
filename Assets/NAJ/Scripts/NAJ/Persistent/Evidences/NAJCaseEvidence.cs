using System;
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
    }

    /// <summary>
    /// Represents a profile
    /// </summary>
    public struct NAJCaseProfile
    {
        public string id;

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
    }
}

