using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
	/// Represents an entry in a component register (World Components, Global data containers, ...).
    /// Used in ANF Settings to designate the components in use
	/// </summary>
    [System.Serializable]
    public struct ComponentRegisterEntry<T>
    {
        public string id;
        [SerializeReference, SubclassSelector(AllowNull = false)] public T data;
    }

    /// <summary>
    /// Represents an entry in a component register (World Components, Global data containers, ...).
    /// Used in ANF Settings to designate the components in use
    /// </summary>
    [System.Serializable]
    public struct GUIRegisterEntry<T>
    {
        public string id;
        [SerializeReference] public T data;
    }
}

