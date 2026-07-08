using System;
using TMPro;
using UnityEngine;

namespace ANF.Locals
{
    /// <summary>
    /// Represents the base settings of the local system
    /// </summary>
    [System.Serializable]
    public class LocalsData
    {
        [Header("General")]
        public string[] languages = new string[] { "eng" };
        public TMP_FontAsset[] fonts;
        [Tooltip("A size entry should contains one size for each type of text (Title, Standard, ...)")]
        public SizeData[] sizes;

        [Header("Default Settings")]
        public string defaultLanguage = "eng";
        public LocalChannelData[] defaultData = new LocalChannelData[Enum.GetValues(typeof(Locals.Channel)).Length];
    }

    /// <summary>
    /// Represents the size data for a size entry
    /// </summary>
    [System.Serializable]
    public struct SizeData
    {
        public int[] sizes;
    }
}
