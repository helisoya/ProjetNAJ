using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ANF.Locals
{
    /// <summary>
    /// Handles the languages
    /// </summary>
    [System.Serializable]
    public class Locals : DataContainer
    {
        public enum Channel
        {
            CHANNEL0,
            CHANNEL1,
            CHANNEL2,
            CHANNEL3,
            CHANNEL4,
            CHANNEL5,
            CHANNEL6,
            CHANNEL7,
            CHANNEL8,
            CHANNEL9
        }

        public enum TextType
        {
            TITLE,
            STANDARD,
            SMALL
        }

        [SerializeField] private LocalsData staticData;

        private string currentLanguage = null;
        private string[] currentAdditionalFiles = null;

        private Dictionary<string, string> locals;
        private UnityEvent onChangeLocal;
        private LocalChannel[] channels;



        /// <summary>
        /// Registers a localized text
        /// </summary>
        /// <param name="text">The localized text</param>
        /// <param name="channel">The text's channel</param>
        public void RegisterText(LocalizedText text, Locals.Channel channel)
        {
            onChangeLocal.AddListener(text.ReloadText);

            LocalChannel linkedChannel = channels[(int)channel];
            channels[(int)channel].onChangeSize[(int)text.Type].AddListener(text.SetSize);
            channels[(int)channel].onChangeFont.AddListener(text.SetFont);
            channels[(int)channel].onChangeColor.AddListener(text.SetColor);
            text.SetColor(linkedChannel.data.color);
            text.SetFont(staticData.fonts[linkedChannel.data.fontIndex]);
            text.SetSize(staticData.sizes[linkedChannel.data.sizeIndex].sizes[(int)text.Type]);
        }

        /// <summary>
        /// Unregisters a localized text
        /// </summary>
        /// <param name="text">The localized text</param>
        /// <param name="channel">The text's channel</param>
        public void UnregisterText(LocalizedText text, Locals.Channel channel)
        {
            onChangeLocal.RemoveListener(text.ReloadText);

            channels[(int)channel].onChangeSize[(int)text.Type].RemoveListener(text.SetSize);
            channels[(int)channel].onChangeFont.RemoveListener(text.SetFont);
            channels[(int)channel].onChangeColor.RemoveListener(text.SetColor);
        }

        /// <summary>
        /// Gets the font size for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <returns>Its font size</returns>
        public SizeData GetFontSize(Locals.Channel channel)
        {
            return staticData.sizes[channels[(int)channel].data.sizeIndex];
        }

        /// <summary>
        /// Gets the font size index for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <returns>Its font size index</returns>
        public int GetFontSizeIndex(Locals.Channel channel)
        {
            return channels[(int)channel].data.sizeIndex;
        }

        /// <summary>
        /// Gets the font for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <returns>Its font</returns>
        public TMP_FontAsset GetFont(Locals.Channel channel)
        {
            return staticData.fonts[channels[(int)channel].data.fontIndex];
        }

        /// <summary>
        /// Gets the font index for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <returns>Its font index</returns>
        public int GetFontIndex(Locals.Channel channel)
        {
            return channels[(int)channel].data.fontIndex;
        }

        /// <summary>
        /// Gets the font color for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <returns>Its font color</returns>
        public Color GetColor(Locals.Channel channel)
        {
            return channels[(int)channel].data.color;
        }

        /// <summary>
        /// Changes the current language
        /// </summary>
        /// <param name="newOne">The new language's code</param>
        /// <param name="force">True if the reload should be forced</param>
        public void ChangeLanguage(string newOne, bool force = false)
        {
            if (newOne.Equals(currentLanguage) && !force) return;

            currentLanguage = newOne;
            locals.Clear();
            LoadContent(newOne + "_system");
            LoadContent(newOne + "_common");

            if (currentAdditionalFiles != null)
                foreach (string file in currentAdditionalFiles)
                    if (file != null)
                        LoadContent(newOne + "_" + file);

            onChangeLocal.Invoke();
        }

        /// <summary>
        /// Changes the current additional files to load
        /// </summary>
        /// <param name="newFiles">The new files to load</param>
        public void ChangeAdditionalFiles(string[] newFiles)
        {
            bool reload = false;
            if ((newFiles == null || currentAdditionalFiles == null) && newFiles != currentAdditionalFiles)
                reload = true;

            if (!reload && newFiles != null && currentAdditionalFiles != null)
            {
                foreach (string newFile in newFiles)
                {
                    if (newFile == null)
                        continue;

                    bool found = false;
                    foreach (string existing in currentAdditionalFiles)
                    {
                        if (existing == null)
                            continue;

                        if (newFile.Equals(existing))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        reload = true;
                        break;
                    }
                }
            }


            currentAdditionalFiles = newFiles;

            if (reload)
                ChangeLanguage(currentLanguage, true);
        }

        /// <summary>
        /// Changes the current font for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <param name="fontIndex">The new font</param>
        public void ChangeFont(Locals.Channel channel, int fontIndex)
        {
            channels[(int)channel].data.fontIndex = fontIndex;
            channels[(int)channel].onChangeFont.Invoke(staticData.fonts[fontIndex]);
        }

        /// <summary>
        /// Changes the current font size for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <param name="sizeIndex">The new size index</param>
        public void ChangeSize(Locals.Channel channel, int sizeIndex)
        {
            channels[(int)channel].data.sizeIndex = sizeIndex;

            SizeData sizes = staticData.sizes[sizeIndex];
            UnityEvent<int>[] events = channels[(int)channel].onChangeSize;

            for (int i = 0; i < events.Length; i++)
                if (i < sizes.sizes.Length)
                    events[i].Invoke(sizes.sizes[i]);
        }

        /// <summary>
        /// Changes the current font color for a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <param name="color">The new font color</param>
        public void ChangeColor(Locals.Channel channel, Color color)
        {
            channels[(int)channel].data.color = color;
            channels[(int)channel].onChangeColor.Invoke(color);
        }

        /// <summary>
        /// Gets a localized string
        /// </summary>
        /// <param name="key">The string's ID</param>
        /// <returns>The localized string</returns>
        public string GetLocal(string key)
        {
            if (key != null && locals.ContainsKey(key)) return locals[key];
            return key;
        }

        /// <summary>
        /// Gets a font from the static database
        /// </summary>
        /// <param name="idx">The font's index</param>
        /// <returns>The font</returns>
        public TMP_FontAsset GetFont(int idx)
        {
            if (idx >= 0 && idx < staticData.fonts.Length) return staticData.fonts[idx];
            return null;
        }

        /// <summary>
        /// Gets the current language
        /// </summary>
        /// <returns>Its key</returns>
        public string GetCurrentLanguage()
        {
            return currentLanguage;
        }

        /// <summary>
        /// Gets the current language's index in the list of languages
        /// </summary>
        /// <returns>Its index</returns>
        public int GetCurrentLanguageIndex()
        {
            for (int i = 0; i < staticData.languages.Length; i++)
                if (staticData.languages[i].Equals(currentLanguage))
                    return i;
            return 0;
        }

        /// <summary>
        /// Gets all available fonts
        /// </summary>
        /// <returns>The available fonts</returns>
        public TMP_FontAsset[] GetFonts()
        {
            return staticData.fonts;
        }

        /// <summary>
        /// Gets all available text sizes
        /// </summary>
        /// <returns>The available text sizes</returns>
        public SizeData[] GetSizes()
        {
            return staticData.sizes;
        }

        /// <summary>
        /// Gets a text size
        /// </summary>
        /// <returns>The text size's index</returns>
        public SizeData GetSize(int index)
        {
            return staticData.sizes[index];
        }


        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>The available languages</returns>
        public string[] GetLanguages()
        {
            return staticData.languages;
        }

        /// <summary>
        /// Loads the content of a file
        /// </summary>
        /// <param name="fileName">The filename</param>
        void LoadContent(string fileName)
        {
            List<string> fileContent = FileManager.ReadTextAsset(Resources.Load<TextAsset>("Locals/" + fileName));
            string line;
            string[] split;

            for (int i = 0; i < fileContent.Count; i++)
            {
                line = fileContent[i];
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;

                split = line.Split("=", 2, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length != 2)
                {
                    Debug.LogWarning("Error on line " + line + ". No = found .");
                    continue;
                }

                while (split[0].EndsWith(" "))
                    split[0] = split[0].Substring(0, split[0].Length - 1);

                while (split[1].StartsWith(" "))
                    split[1] = split[1].Substring(1);

                locals.TryAdd(split[0], split[1]);
            }
        }

        public void Initialize(ANFSettings settings)
        {
            onChangeLocal = new UnityEvent();

            Array values = Enum.GetValues(typeof(Channel));
            channels = new LocalChannel[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                channels[i] = new LocalChannel();
                channels[i].data = new LocalChannelData();
                channels[i].onChangeColor = new UnityEvent<Color>();
                channels[i].onChangeFont = new UnityEvent<TMP_FontAsset>();

                channels[i].onChangeSize = new UnityEvent<int>[Enum.GetValues(typeof(TextType)).Length];
                for (int j = 0; j < channels[i].onChangeSize.Length; j++)
                {
                    channels[i].onChangeSize[j] = new UnityEvent<int>();
                }
            }

            currentLanguage = null;
            locals = new Dictionary<string, string>();

            Reset();
        }

        public void Reset()
        {
            Array values = Enum.GetValues(typeof(Channel));

            for (int i = 0; i < values.Length; i++)
            {
                ChangeColor((Channel)i, staticData.defaultData.Length > i ? staticData.defaultData[i].color : Color.black);
                ChangeFont((Channel)i, staticData.defaultData.Length > i ? staticData.defaultData[i].fontIndex : 0);
                ChangeSize((Channel)i, staticData.defaultData.Length > i ? staticData.defaultData[i].sizeIndex : 0);
            }
            ChangeLanguage(staticData.defaultLanguage);
        }

        public DataContainer CloneContainer()
        {
            return new Locals() { staticData = staticData };
        }

        public void Save(JSON json)
        {
            json.Add("currentLanguage", currentLanguage);

            JArray channelArray = new JArray();
            JSON channelData;

            foreach (LocalChannel channel in channels)
            {
                channelData = new JSON();
                channelData.Add("fontIndex", channel.data.fontIndex);
                channelData.Add("sizeIndex", channel.data.sizeIndex);
                channelData.Add("color", channel.data.color);
                channelArray.Add(channelData);
            }

            json.Add("channels", channelArray);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("channels"))
            {
                JArray channelArray = json.GetJArray("channels");
                JSON channelJson;
                for (int i = 0; i < channels.Length && i < channelArray.Length; i++)
                {
                    channelJson = channelArray.GetJSON(i);
                    if (channelJson.ContainsKey("fontIndex"))
                        ChangeFont((Channel)i, channelJson.GetInt("fontIndex"));
                    if (channelJson.ContainsKey("sizeIndex"))
                        ChangeSize((Channel)i, channelJson.GetInt("sizeIndex"));
                    if (channelJson.ContainsKey("color"))
                        ChangeColor((Channel)i, channelJson.GetJArray("color").AsColor());
                }
            }

            if (json.ContainsKey("currentLanguage"))
                ChangeLanguage(json.GetString("currentLanguage"));
        }
    }
}
