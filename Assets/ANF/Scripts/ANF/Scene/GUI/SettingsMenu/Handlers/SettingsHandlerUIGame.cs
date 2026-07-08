using ANF.Persistent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the game tab in the settings
    /// </summary>
    [System.Serializable]
    public class SettingsHandlerUIGame : SettingsHandlerUI
    {
        private Dictionary<string, List<KeyValuePair<string, Selectable>>> selectables;


        public override void PopulateTab()
        {
            selectables = new Dictionary<string, List<KeyValuePair<string, Selectable>>>();

            PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals);

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                foreach (string key in settings.GetValues().Keys)
                {
                    SettingsContainer.SettingsObjectData data = settings.GetValues()[key];
                    string tabKey = data.drawParameters.tabKey;
                    RectTransform tab = menu.GetTab(tabKey);

                    if (!selectables.ContainsKey(tabKey))
                        selectables.Add(tabKey, new List<KeyValuePair<string, Selectable>>());

                    switch (data.type)
                    {
                        case SettingsContainer.SettingsDataType.Bool:
                            {
                                Toggle toggle = menu.CreateToggle(data.drawParameters.label, tab);
                                toggle.SetIsOnWithoutNotify((bool)data.value);
                                toggle.onValueChanged.AddListener((bool value) =>
                                {
                                    settings.SetValue(key, data.type, value);
                                });
                                selectables[tabKey].Add(new(key, toggle));
                                break;
                            }
                        case SettingsContainer.SettingsDataType.UInt:
                        case SettingsContainer.SettingsDataType.Int:
                            {
                                if (data.drawParameters.dropdownLabels != null)
                                {
                                    TMP_Dropdown dropdown = menu.CreateDropdown(data.drawParameters.label, tab);
                                    dropdown.ClearOptions();

                                    List<string> labels = new List<string>();
                                    foreach (string labelKey in data.drawParameters.dropdownLabels)
                                    {
                                        labels.Add(locals != null ? locals.GetLocal(labelKey) : labelKey);
                                    }

                                    dropdown.AddOptions(labels);
                                    dropdown.SetValueWithoutNotify((int)data.value);

                                    dropdown.onValueChanged.AddListener((int value) =>
                                    {
                                        if (data.type == SettingsContainer.SettingsDataType.Int)
                                            settings.SetValue(key, data.type, value);
                                        else if (data.type == SettingsContainer.SettingsDataType.UInt)
                                            settings.SetValue(key, data.type, (uint)value);
                                    });

                                    selectables[tabKey].Add(new(key, dropdown));
                                    break;
                                }
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Float:
                            {
                                Slider slider = menu.CreateSlider(data.drawParameters.label, tab);
                                slider.maxValue = data.drawParameters.sliderMaxValue;
                                slider.minValue = data.drawParameters.sliderMinValue;
                                slider.SetValueWithoutNotify((float)data.value);
                                slider.onValueChanged.AddListener((float value) =>
                                {
                                    if (data.type == SettingsContainer.SettingsDataType.Float)
                                        settings.SetValue(key, data.type, value);
                                });
                                selectables[tabKey].Add(new(key, slider));
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Color:
                            {
                                Button button = menu.CreateColorPicker(data.drawParameters.label, tab);
                                button.GetComponent<Image>().color = (Color)data.value;
                                button.onClick.AddListener(() =>
                                {
                                    menu.OpenColorPicker((Color)data.value, button, (Color newColor) =>
                                    {
                                        button.GetComponent<Image>().color = newColor;
                                        data.value = newColor;
                                        data.onValueChange.Invoke(newColor);
                                    });
                                });
                                selectables[tabKey].Add(new(key, button));
                                break;
                            }
                        case SettingsContainer.SettingsDataType.String:
                        case SettingsContainer.SettingsDataType.Vector2:
                        case SettingsContainer.SettingsDataType.Vector3:
                        case SettingsContainer.SettingsDataType.Vector4:
                            break;
                    }
                }

                using (var it = selectables.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var value = it.Current;

                        menu.RegisterResetAction(value.Key, () =>
                        {
                            Reset(value.Key);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Resets the parameters to their default values
        /// </summary>
        private void Reset(string tabKey)
        {
            if (!selectables.ContainsKey(tabKey))
                return;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                foreach (var pair in selectables[tabKey])
                {
                    settings.Reset(pair.Key);
                    SettingsContainer.SettingsObjectData data = settings.GetValues()[pair.Key];
                    switch (data.type)
                    {
                        case SettingsContainer.SettingsDataType.Bool:
                            {
                                pair.Value.GetComponent<Toggle>().SetIsOnWithoutNotify((bool)data.value);
                                break;
                            }
                        case SettingsContainer.SettingsDataType.UInt:
                        case SettingsContainer.SettingsDataType.Int:
                            {
                                if (data.drawParameters.dropdownLabels != null)
                                {
                                    pair.Value.GetComponent<TMP_Dropdown>().SetValueWithoutNotify((int)data.value);
                                    break;
                                }
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Float:
                            {
                                pair.Value.GetComponent<Slider>().SetValueWithoutNotify((float)data.value);
                                break;
                            }
                        case SettingsContainer.SettingsDataType.Color:
                            {
                                pair.Value.GetComponent<Image>().color = (Color)data.value;
                                break;
                            }
                        case SettingsContainer.SettingsDataType.String:
                        case SettingsContainer.SettingsDataType.Vector2:
                        case SettingsContainer.SettingsDataType.Vector3:
                        case SettingsContainer.SettingsDataType.Vector4:
                            break;
                    }
                }
            }
        }

        public override void RedrawLocalizedElements()
        {
        }
    }

}
