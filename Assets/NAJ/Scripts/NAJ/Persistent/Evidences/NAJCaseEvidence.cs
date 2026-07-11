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
    }
}

