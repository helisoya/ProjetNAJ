using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents a settings container.
    /// Settings contained here are usually settings for the world & gui components.
    /// (For instance, this does not contain screen & language settings)
    /// </summary>
    [System.Serializable]
    public class SettingsContainer : DataContainer
    {
        public enum SettingsDataType
        {
            Unknown,
            Bool,
            String,
            Int,
            UInt,
            Float,
            Vector2,
            Vector3,
            Vector4,
            Color
        }

        private Dictionary<string, SettingsObjectData> savedObjects;
        public DataContainer CloneContainer()
        {
            return new SettingsContainer();
        }

        public void Initialize(ANFSettings settings)
        {
            LoadFromFile(settings.generalDataPath + "settings");
        }

        /// <summary>
        /// Registers a callback to a setting's value and gets the current value.
        /// Keep to simple types (bool, int, Vector2, Vector3, ...)
        /// </summary>
        /// <param name="key">The value's key</param>
        /// <param name="type">The value's type</param>
        /// <param name="onValueChange">The callback when a value changed</param>
        /// <returns>The current value</returns>
        public object Register(string key, SettingsDataType type, UnityAction<object> onValueChange)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData value))
            {
                if (!TypeGood(value.value, type))
                {
                    return default;
                }
                value.onValueChange.AddListener(onValueChange);
                return value.value;
            }
            return default;
        }

        /// <summary>
		/// Gets a value from the settings
		/// </summary>
		/// <param name="key">The value's key</param>
		/// <param name="type">The value's type</param>
		/// <returns>The value if found (or valid with the type provided)</returns>
        public object GetValue(string key, SettingsDataType type)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData value) &&
                TypeGood(value.value, type))
            {
                return value.value;
            }

            return default;
        }

        /// <summary>
		/// Changes a setting's value
		/// </summary>
		/// <param name="key">The value's key</param>
		/// <param name="type">The value's type</param>
		/// <param name="value">The new value</param>
        public void SetValue(string key, SettingsDataType type, object value)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData obj) &&
                TypeGood(obj.value, type) && TypeGood(value, type))
            {
                obj.SetValue(value);
            }
        }

        /// <summary>
        /// Unregisters a callback for a settings' value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="onValueChange">The callback to remove</param>
        public void Unregister(string key, UnityAction<object> onValueChange)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData value))
            {
                value.onValueChange.RemoveListener(onValueChange);
            }
        }

        /// <summary>
		/// Checks if a value is of a specific serialized type
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="type">The supposed type</param>
		/// <returns>True if the value is of the correct type</returns>
        private bool TypeGood(object value, SettingsDataType type)
        {
            switch (type)
            {
                case SettingsDataType.Bool:
                    return value is bool;
                case SettingsDataType.String:
                    return value is string;
                case SettingsDataType.Int:
                    return value is int;
                case SettingsDataType.UInt:
                    return value is uint;
                case SettingsDataType.Float:
                    return value is float;
                case SettingsDataType.Color:
                    return value is Color;
                case SettingsDataType.Vector2:
                    return value is Vector2;
                case SettingsDataType.Vector3:
                    return value is Vector3;
                case SettingsDataType.Vector4:
                    return value is Vector4;
            }

            return false;
        }

        /// <summary>
		/// Gets the known values
		/// </summary>
		/// <returns>The known values</returns>
        public Dictionary<string, SettingsObjectData> GetValues()
        {
            return savedObjects;
        }

        public void Reset()
        {
            foreach (SettingsObjectData data in savedObjects.Values)
                data.ResetValue();
        }

        /// <summary>
		/// Resets a specific parameter
		/// </summary>
		/// <param name="key">The parameter's key</param>
        public void Reset(string key)
        {
            if (savedObjects.TryGetValue(key, out SettingsObjectData data))
                data.ResetValue();
        }

        public void Load(JSON json)
        {
            Reset();
            foreach (string key in json.Keys)
            {
                JSON valueJSON = json.GetJSON(key);
                if (valueJSON.ContainsKey("type") && valueJSON.ContainsKey("value") && savedObjects.TryGetValue(key, out SettingsObjectData value))
                {
                    value.Load(valueJSON);
                }

            }
        }

        public void Save(JSON json)
        {
            foreach (string key in savedObjects.Keys)
            {
                JSON valueJSON = new JSON();
                savedObjects[key].Save(valueJSON);
                json.Add(key, valueJSON);
            }
        }

        /// <summary>
        /// Loads the known settings from disk
        /// </summary>
        /// <param name="filePath">The filepath</param>
        private void LoadFromFile(string filePath)
        {
            savedObjects = new Dictionary<string, SettingsObjectData>();
            SettingsObjectData obj;
            string key;
            List<string> lines = FileManager.ReadTextAsset(Resources.Load<TextAsset>(filePath));
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    string[] split = line.Split(' ');
                    int correctIdx = 0;

                    if (split.Length < 3)
                        continue;

                    key = split[0];
                    obj = new SettingsObjectData();
                    obj.type = ParseType(split[1]);

                    if (obj.type == SettingsDataType.Unknown)
                        continue;

                    correctIdx = 2;

                    string fullStr = "";

                    if (obj.type == SettingsDataType.Vector2 || obj.type == SettingsDataType.Vector3 ||
                        obj.type == SettingsDataType.Vector4 || (obj.type == SettingsDataType.Color && !split[correctIdx].StartsWith('#')))
                    {
                        while(!fullStr.EndsWith(')') && correctIdx < split.Length)
                        {
                            fullStr += split[correctIdx];
                            correctIdx++;
                        }

                        if (!fullStr.EndsWith(')'))
                            continue;
                    }
                    else
                    {
                        fullStr = split[correctIdx];
                        correctIdx++;
                    }

                    obj.value = ParseValue(fullStr, obj.type);

                    if (obj.value == null)
                        continue;

                    obj.defaultValue = obj.value;

                    if (correctIdx + 1 >= split.Length)
                        continue;

                    SettingsObjectDrawParameters drawParameters = new SettingsObjectDrawParameters(split[correctIdx + 1], split[correctIdx]);

                    correctIdx += 2;

                    if (correctIdx < split.Length && (obj.type == SettingsDataType.Float || obj.type == SettingsDataType.Int ||
                        obj.type == SettingsDataType.UInt))
                    {
                        fullStr = "";
                        while (!fullStr.EndsWith(')') && correctIdx < split.Length)
                        {
                            fullStr += split[correctIdx];
                            correctIdx++;
                        }

                        if (!fullStr.EndsWith(')') || correctIdx != split.Length)
                            continue;

                        if(obj.type == SettingsDataType.Float)
                        {
                            // Slider
                            string[] splitSlider = fullStr.Replace("(", "").Replace(")", "").Split(',');
                            if (splitSlider.Length != 2 ||
                                !float.TryParse(splitSlider[0], NumberStyles.Float, CultureInfo.InvariantCulture, out drawParameters.sliderMinValue) ||
                                !float.TryParse(splitSlider[1], NumberStyles.Float, CultureInfo.InvariantCulture, out drawParameters.sliderMaxValue))
                                continue;

                        }
                        else if (obj.type == SettingsDataType.Int || obj.type == SettingsDataType.UInt)
                        {
                            // Dropdown
                            drawParameters.dropdownLabels = fullStr.Replace("(", "").Replace(")", "").Split(',');
                            if (drawParameters.dropdownLabels != null && drawParameters.dropdownLabels.Length == 0)
                            {
                                drawParameters.dropdownLabels = null;
                            }
                            else if(drawParameters.dropdownLabels != null)
                            {
                                for (int i = 0; i < drawParameters.dropdownLabels.Length; i++)
                                    drawParameters.dropdownLabels[i] = drawParameters.dropdownLabels[i].Replace(" ", "").Replace("\t", "");
                            }
                        }
                    }

                    obj.drawParameters = drawParameters;

                    savedObjects.Add(key, obj);
                }
            }
        }

        /// <summary>
        /// Parse a string to a SettingsDataType
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The data type. Unknown if the parsing failed</returns>
        private SettingsDataType ParseType(string str)
        {
            str = str.ToLower();
            switch(str)
            {
                case "string":
                    return SettingsDataType.String;
                case "bool":
                    return SettingsDataType.Bool;
                case "float":
                    return SettingsDataType.Float;
                case "int":
                    return SettingsDataType.Int;
                case "uint":
                    return SettingsDataType.UInt;
                case "vector2":
                    return SettingsDataType.Vector2;
                case "vector3":
                    return SettingsDataType.Vector3;
                case "vector4":
                    return SettingsDataType.Vector4;
                case "color":
                    return SettingsDataType.Color;
            }

            return SettingsDataType.Unknown;
        }

        /// <summary>
        /// Parses a string to a specific kind of value
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <param name="type">The target type</param>
        /// <returns>The parsed value</returns>
        private object ParseValue(string str, SettingsDataType type)
        {
            float x = 0, y = 0, z = 0, w = 0;
            string[] vectorSplit = str.Replace("(", "").Replace(")", "").Split(',');
            switch(type)
            {
                case SettingsDataType.String:
                    return str;
                case SettingsDataType.Bool:
                    if (bool.TryParse(str, out bool boolVal))
                        return boolVal;
                    break;
                case SettingsDataType.Int:
                    if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
                        return intVal;
                    break;
                case SettingsDataType.UInt:
                    if (uint.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint uintVal))
                        return uintVal;
                    break;
                case SettingsDataType.Float:
                    if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal))
                        return floatVal;
                    break;
                case SettingsDataType.Color:
                    if (str.StartsWith("#"))
                    {
                        if (ColorUtility.TryParseHtmlString(str, out Color color))
                        {
                            color.a = 1;
                            return color;
                        }
                            
                        break;
                    }
                    else
                        goto case SettingsDataType.Vector4;
                case SettingsDataType.Vector4:
                    if (vectorSplit.Length != 4 || !float.TryParse(vectorSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                        break;
                    goto case SettingsDataType.Vector3;
                case SettingsDataType.Vector3:
                    if (vectorSplit.Length < 3 || !float.TryParse(vectorSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                        break;
                    goto case SettingsDataType.Vector2;
                case SettingsDataType.Vector2:

                    if (vectorSplit.Length < 2 || 
                        !float.TryParse(vectorSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
                        !float.TryParse(vectorSplit[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                        break;

                    if (type == SettingsDataType.Color)
                        return new Color(x, y, z, w);
                    if (type == SettingsDataType.Vector4)
                        return new Vector4(x, y, z, w);
                    if (type == SettingsDataType.Vector3)
                        return new Vector3(x, y, z);
                    if (type == SettingsDataType.Vector2)
                        return new Vector2(x, y);
                    break;
            }

            return null;
        }

        /// <summary>
        /// Represents a settings object's data
        /// </summary>
        public class SettingsObjectData : Jsonable
        {
            public UnityEvent<object> onValueChange;
            public SettingsDataType type;
            public object value;
            public object defaultValue;
            public SettingsObjectDrawParameters drawParameters;

            public SettingsObjectData()
            {
                onValueChange = new UnityEvent<object>();
            }

            public void SetValue(object newValue)
            {
                value = newValue;
                onValueChange.Invoke(newValue);
            }

            /// <summary>
            /// Resets the object's value
            /// </summary>
            public void ResetValue()
            {
                value = defaultValue;
                onValueChange.Invoke(value);
            }

            public void Save(JSON json)
            {
                json.Add("value", value);
                json.Add("type", (int)type);
                JSON drawJSON = new JSON();
            }

            public void Load(JSON json)
            {
                if (!json.ContainsKey("type") || !json.ContainsKey("value"))
                    return;

                onValueChange = new UnityEvent<object>();
                type = (SettingsDataType)json.GetInt("type");
                switch (type)
                {
                    case SettingsDataType.Bool:
                        {
                            value = json.GetBool("value");
                            break;
                        }
                    case SettingsDataType.String:
                        {
                            value = json.GetString("value");
                            break;
                        }
                    case SettingsDataType.Int:
                        {
                            value = json.GetInt("value");
                            break;
                        }
                    case SettingsDataType.UInt:
                        {
                            value = json.GetJNumber("value").AsUInt();
                            break;
                        }
                    case SettingsDataType.Float:
                        {
                            value = json.GetFloat("value");
                            break;
                        }
                    case SettingsDataType.Color:
                        {
                            value = json.GetJArray("value").AsColor();
                            break;
                        }
                    case SettingsDataType.Vector2:
                        {
                            value = json.GetJArray("value").AsVector2();
                            break;
                        }
                    case SettingsDataType.Vector3:
                        {
                            value = json.GetJArray("value").AsVector3();
                            break;
                        }
                    case SettingsDataType.Vector4:
                        {
                            value = json.GetJArray("value").AsVector4();
                            break;
                        }
                }
            }
        }

        /// <summary>
		/// Represents a setting's object draw parameters
		/// </summary>
        public struct SettingsObjectDrawParameters
        {
            public string label;
            public string tabKey;
            public string[] dropdownLabels;
            public float sliderMinValue;
            public float sliderMaxValue;
            public SettingsObjectDrawParameters(string label, string tabKey, string[] dropdownLabels = null,
                float sliderMinValue = 0, float sliderMaxValue = 1)
            {
                this.label = label;
                this.tabKey = tabKey;
                this.dropdownLabels = dropdownLabels;
                this.sliderMaxValue = sliderMaxValue;
                this.sliderMinValue = sliderMinValue;
            }
        }
    }
}
