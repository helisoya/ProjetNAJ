using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Represents a skybox's data
    /// </summary>
    [CreateAssetMenu(fileName = "SkyboxData", menuName = "ANF/SkyboxData")]
    public class SkyboxData : ScriptableObject
    {
        public Cubemap skybox;
        public Color sunColor;
    }
}

