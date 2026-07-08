using ANF.Utils;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{

    /// <summary>
	/// Contains the various maps and maps defs
	/// </summary>
    [System.Serializable]
    public class MapContainer : DataContainer
    {
        private MapsData maps;
        private List<MapDefs> mapDefs;

        public DataContainer CloneContainer()
        {
            return new MapContainer();
        }

        public void Initialize(ANFSettings settings)
        {
            maps = Resources.Load<MapsData>(settings.generalDataPath + "MapsData");
            LoadMapDefs(settings);
        }

        /// <summary>
		/// Gets a specific map
		/// </summary>
		/// <param name="id">The map's id</param>
        /// <param name="map">The out map</param>
		/// <returns>True if the map was found</returns>
        public bool GetMap(string id, out MapData map)
        {
            map = maps.GetMap(id);
            return id != null;
        }

        /// <summary>
		/// Gets a specific map definition
		/// </summary>
		/// <param name="id">The definition's id</param>
        /// <param name="def">The out definition</param>
		/// <returns>True if the definition was found</returns>
        public bool GetDef(string id, out MapDefs def)
        {
            foreach (MapDefs entry in mapDefs)
                if (entry.id.Equals(id))
                {
                    def = entry;
                    return true;
                }

            def = null;
            return false;
        }

        /// <summary>
		/// Loads the known map defs
        /// <paramref name="settings"/>The ANF Settings</param>
		/// </summary>
        private void LoadMapDefs(ANFSettings settings)
        {
            mapDefs = new List<MapDefs>();

            MapDefs currentDef = null;
            List<string> lines = FileManager.ReadTextAsset(
                Resources.Load<TextAsset>(settings.generalDataPath + "mapDefs")
            );

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    if (line.StartsWith('['))
                    {
                        if (currentDef != null)
                        {
                            mapDefs.Add(currentDef);
                            currentDef = null;
                        }

                        string[] split = line.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length == 1)
                        {
                            bool alreadyExists = false;
                            foreach (MapDefs def in mapDefs)
                                if (def.id.Equals(split[0]))
                                    alreadyExists = true;

                            if (!alreadyExists)
                            {
                                currentDef = new MapDefs() { id = split[0], buttons = new List<MapButtonDef>() };
                            }
                        }
                    }
                    else if (currentDef != null)
                    {
                        MapButtonDef buttonDef = new MapButtonDef();
                        string[] split = line.Split(' ');

                        if (split.Length >= 2)
                        {
                            buttonDef.linkedButton = split[0];

                            if (split[1].ToLower().Equals("never") && split.Length == 2)
                            {
                                buttonDef.type = MapDefsType.Never;
                            }
                            else if (split[1].ToLower().Equals("always") && split.Length == 3)
                            {
                                buttonDef.type = MapDefsType.Always;
                                buttonDef.linkedScript = split[2];
                            }
                            else if (split[1].ToLower().Equals("variabletoggle") && split.Length == 4)
                            {
                                buttonDef.type = MapDefsType.VariableToggle;
                                buttonDef.linkedScript = split[2];
                                buttonDef.linkedVariable = split[3];
                            }
                            else if (split[1].ToLower().Equals("variable") && split.Length == 6)
                            {
                                buttonDef.type = MapDefsType.Variable;
                                buttonDef.linkedScript = split[2];
                                buttonDef.linkedVariable = split[3];
                                if (!int.TryParse(split[5], out buttonDef.variableCheckValue))
                                    buttonDef.variableCheckValue = 0;

                                switch (split[5].ToLower())
                                {
                                    case "equals":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.Equals;
                                        break;
                                    case "notequals":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.NotEquals;
                                        break;
                                    case "greaterorequals":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.GreaterOrEquals;
                                        break;
                                    case "greater":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.Greater;
                                        break;
                                    case "less":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.Less;
                                        break;
                                    case "lessorequals":
                                        buttonDef.variableCheckType = MapDefsVariableCheckType.LessOrEquals;
                                        break;
                                }
                            }
                            currentDef.buttons.Add(buttonDef);
                        }
                    }
                }
            }

            if (currentDef != null)
            {
                mapDefs.Add(currentDef);
            }

        }

        public void Reset()
        {
        }

        public void Load(JSON json)
        {
        }

        public void Save(JSON json)
        {
        }
    }
}