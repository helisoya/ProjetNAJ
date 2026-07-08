using ANF.Persistent;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the graphics tab in the settings
    /// </summary>
    [System.Serializable]
    public class SettingsHandlerUIGraphics : SettingsHandlerUI
    {
        private Toggle fullscreenToggle;
        private TMP_Dropdown resolutionDropdown;
        private TMP_Dropdown antiAliasingDropdown;
        private TMP_Dropdown shadowsDropdown;
        private Toggle vSyncToggle;

        public override void PopulateTab()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out ScreenSettingsContainer screenSettings))
            {
                RectTransform graphicsMenu = menu.GetTab("SettingsMenu_Graphics");

                {
                    fullscreenToggle = menu.CreateToggle("SettingsMenu_Graphics_Fullscreen", graphicsMenu);
                    fullscreenToggle.SetIsOnWithoutNotify(screenSettings.IsFullscreen());
                    fullscreenToggle.onValueChanged.AddListener((bool fullscreen) =>
                    {
                        screenSettings.SetFullscreen(fullscreen);
                    });
                }

                {
                    Resolution[] resolutions = Screen.resolutions;
                    Resolution currentResolution = screenSettings.GetResolution();
                    List<string> resolutionsLabels = new List<string>();
                    int currentIdx = -1;

                    for (int i = 0; i < resolutions.Length; i++)
                    {
                        resolutionsLabels.Add($"{resolutions[i].width}/{resolutions[i].height}({Math.Round(resolutions[i].refreshRateRatio.value, 2)})");
                        if (currentIdx == -1 && resolutions[i].width == currentResolution.width &&
                            resolutions[i].height == currentResolution.height && resolutions[i].refreshRateRatio.value == currentResolution.refreshRateRatio.value)
                        {
                            currentIdx = i;
                        }
                    }

                    if (currentIdx == -1)
                        currentIdx = 0;

                    resolutionDropdown = menu.CreateDropdown("SettingsMenu_Graphics_Resolution", graphicsMenu);
                    resolutionDropdown.ClearOptions();
                    resolutionDropdown.AddOptions(resolutionsLabels);
                    resolutionDropdown.SetValueWithoutNotify(currentIdx);
                    resolutionDropdown.onValueChanged.AddListener((int idx) =>
                    {
                        screenSettings.SetResolution(resolutions[idx]);
                    });
                }

                {
                    vSyncToggle = menu.CreateToggle("SettingsMenu_Graphics_VSync", graphicsMenu);
                    vSyncToggle.SetIsOnWithoutNotify(screenSettings.GetVSyncCount() != 0);
                    vSyncToggle.onValueChanged.AddListener((bool vSync) =>
                    {
                        screenSettings.SetVSyncCount(vSync ? 1 : 0);
                    });
                }

                {
                    int[] antiAliasingValues = { 0, 2, 4, 8 };
                    List<string> antiAliasingValuesLabels = new List<string>(
                        new string[] { "x0", "x2", "x4", "x8" }
                        );
                    int selectedIndex = Array.IndexOf(antiAliasingValues, screenSettings.GetAntiAliasing());
                    if (selectedIndex == -1)
                        selectedIndex = 0;

                    antiAliasingDropdown = menu.CreateDropdown("SettingsMenu_Graphics_AntiAliasing", graphicsMenu);
                    antiAliasingDropdown.ClearOptions();
                    antiAliasingDropdown.AddOptions(antiAliasingValuesLabels);
                    antiAliasingDropdown.SetValueWithoutNotify(selectedIndex);
                    antiAliasingDropdown.onValueChanged.AddListener((int idx) =>
                    {
                        screenSettings.SetAntiAliasing(antiAliasingValues[idx]);
                    });
                }

                {
                    PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals);

                    List<string> labels = new List<string>(
                        new string[]
                        {
                            locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_1") : "SettingsMenu_Graphics_Shadows_1",
                            locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_2") : "SettingsMenu_Graphics_Shadows_2",
                            locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_3") : "SettingsMenu_Graphics_Shadows_3",
                            locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_4") : "SettingsMenu_Graphics_Shadows_4"
                        }
                        );

                    shadowsDropdown = menu.CreateDropdown("SettingsMenu_Graphics_Shadows", graphicsMenu);
                    shadowsDropdown.ClearOptions();
                    shadowsDropdown.AddOptions(labels);
                    shadowsDropdown.SetValueWithoutNotify(screenSettings.GetShadowQuality() - 1);
                    shadowsDropdown.onValueChanged.AddListener((int idx) =>
                    {
                        screenSettings.SetShadowQuality(idx + 1);
                    });
                }
            }
        }

        public override void RedrawLocalizedElements()
        {
            if (!PersistentDataManager.instance.GetGlobalData().GetComponent(out ScreenSettingsContainer screenSettings))
                return;

            PersistentDataManager.instance.GetGlobalData().GetComponent(out Locals.Locals locals);

            List<string> labels = new List<string>(
                new string[]
                {
                    locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_1") : "SettingsMenu_Graphics_Shadows_1",
                    locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_2") : "SettingsMenu_Graphics_Shadows_2",
                    locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_3") : "SettingsMenu_Graphics_Shadows_3",
                    locals != null ? locals.GetLocal("SettingsMenu_Graphics_Shadows_4") : "SettingsMenu_Graphics_Shadows_4"
                });

            shadowsDropdown.ClearOptions();
            shadowsDropdown.AddOptions(labels);
            shadowsDropdown.SetValueWithoutNotify(screenSettings.GetShadowQuality());
        }
    }
}
