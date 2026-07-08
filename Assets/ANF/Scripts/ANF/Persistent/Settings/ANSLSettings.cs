using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents additional settings linked to ANSL
    /// </summary>
    [System.Serializable]
    public class ANSLSettings : ANFSettingsAdditionalPart
    {
        [Tooltip("ANSL source files location")]
        public string anslSourceFolder = "Assets/ANSL/";
        [Tooltip("ANSL destination file location (Is inside Resources/)")]
        public string anslDestinationFolder = "Story/";
        [Tooltip("Path to the ANSL .code-snippets file (auto complete for VS code)")]
        public string anslVSCodeSnippetsPath = ".vscode/";

        [HideInInspector] // Use the ANSL Functions Window to edit this 
        public List<ANSLFunctionSettingsData> registeredFunctions;
        
        //public SerializedDictionary<Type, ANSLFunctionSettingsData> registeredFunctions = new SerializedDictionary<Type, ANSLFunctionSettingsData>();

        /// <summary>
        /// Represents the internal settings data for an ANSL function
        /// </summary>
        [System.Serializable]
        public struct ANSLFunctionSettingsData
        {
            public string typeName;
            public uint id;
            public bool enabled;
        }
    }
}

