using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Contains all known maps
    /// </summary>
    [CreateAssetMenu(fileName = "MapsData", menuName = "ANF/Maps Data")]
    public class MapsData : ScriptableObject
    {
        public List<MapData> maps = new List<MapData>();

        /// <summary>
        /// Gets a map from the data ppol
        /// </summary>
        /// <param name="id">The map's id</param>
        /// <returns>The map, if found</returns>
        public MapData GetMap(string id)
        {
            foreach (MapData map in maps)
                if (map.id.Equals(id))
                    return map;

            return null;
        }
    }

    /// <summary>
    /// Represents a map's data
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        public string id;
        public Sprite backgroundSprite;
        public List<MapButton> buttons = new List<MapButton>();
    }

    /// <summary>
    /// Represents a button in a map
    /// </summary>
    [System.Serializable]
    public class MapButton
    {
        public string id;
        public float rotation;
        public Sprite sprite;
        public Vector2 normalizedPosition;
    }
}
