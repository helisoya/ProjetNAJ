using ANF.Persistent;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the sound tab in the settings
    /// </summary>
    [System.Serializable]
    public class SettingsHandlerUISound : SettingsHandlerUI
    {
        [SerializeField] private float sliderMaxValue = 20;
        [SerializeField] private float sliderMinValue = -80;

        private Slider musicSlider;
        private Slider ambientSlider;
        private Slider sfxSlider;
        private Slider voiceSlider;

        public override void PopulateTab()
        {
            RectTransform soundMenu = menu.GetTab("SettingsMenu_Sound");

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
            {
                musicSlider = menu.CreateSlider("SettingsMenu_Sound_Music", soundMenu);
                musicSlider.minValue = sliderMinValue;
                musicSlider.maxValue = sliderMaxValue;
                musicSlider.SetValueWithoutNotify(audioManager.GetMusicVolume());
                musicSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetMusicVolume(newVolume);
                });

                ambientSlider = menu.CreateSlider("SettingsMenu_Sound_Ambient", soundMenu);
                ambientSlider.minValue = sliderMinValue;
                ambientSlider.maxValue = sliderMaxValue;
                ambientSlider.SetValueWithoutNotify(audioManager.GetAmbientVolume());
                ambientSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetAmbientVolume(newVolume);
                });

                sfxSlider = menu.CreateSlider("SettingsMenu_Sound_Sfx", soundMenu);
                sfxSlider.minValue = sliderMinValue;
                sfxSlider.maxValue = sliderMaxValue;
                sfxSlider.SetValueWithoutNotify(audioManager.GetSfxVolume());
                sfxSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetSfxVolume(newVolume);
                });

                voiceSlider = menu.CreateSlider("SettingsMenu_Sound_Voice", soundMenu);
                voiceSlider.minValue = sliderMinValue;
                voiceSlider.maxValue = sliderMaxValue;
                voiceSlider.SetValueWithoutNotify(audioManager.GetVoiceVolume());
                voiceSlider.onValueChanged.AddListener((float newVolume) =>
                {
                    audioManager.SetVoiceVolume(newVolume);
                });
            }

            menu.RegisterResetAction("SettingsMenu_Sound", Reset);
        }

        /// <summary>
        /// Resets the values
        /// </summary>
        private void Reset()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out Persistent.AudioManager audioManager))
            {
                audioManager.Reset();

                musicSlider.SetValueWithoutNotify(audioManager.GetMusicVolume());
                ambientSlider.SetValueWithoutNotify(audioManager.GetAmbientVolume());
                sfxSlider.SetValueWithoutNotify(audioManager.GetSfxVolume());
                voiceSlider.SetValueWithoutNotify(audioManager.GetVoiceVolume());
            }
        }

        public override void RedrawLocalizedElements()
        {

        }
    }

}
