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
        public string anslSourceFolder = "Assets/ANSL/Source/";
        [Tooltip("ANSL destination file location")]
        public string anslDestinationFolder = "Assets/ANSL/Compiled";
        [Tooltip("Path to the ANSL .code-snippets file (auto complete for VS code)")]
        public string anslResourcePath = "ANSL/";
        [Tooltip("Path to the ANSL scripts for the resource manager")]
        public string anslVSCodeSnippetsPath = ".vscode/";
        [Tooltip("Should scripts be automatically registered as Addressables ?")]
        public bool linkScriptsToAddressables = true;
        [Tooltip("If Addressables are used, all scripts will have this label enabled")]
        public string addressablesLabel = "Scripts";


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

