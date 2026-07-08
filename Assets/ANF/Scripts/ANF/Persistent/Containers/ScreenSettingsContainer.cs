
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANF.Persistent
{
    /// <summary>
    /// Handles the Screen Settings
    /// </summary>
    [System.Serializable]
    public class ScreenSettingsContainer : DataContainer
    {
        [Tooltip("If true, current Screen/QualitySettings data will be used as a default for ALL parameters")]
        [SerializeField] private bool useScreenSettings = true;
        [SerializeField] private bool defaultFullscreen = true;
        [SerializeField] private int defaultWidth = 800;
        [SerializeField] private int defaultHeight = 600;
        [SerializeField] private int defaultVSyncCount = 1;
        [SerializeField] private int defaultAntiAliasing = 2;
        [SerializeField] private int defaultShadowCascadeCount = 4;
        private Resolution resolution;
        private bool fullscreen;
        private int vSyncCount;
        private int antiAliasing;
        private int shadowCascadeCount;

        public DataContainer CloneContainer()
        {
            return new ScreenSettingsContainer()
            {
                defaultFullscreen = defaultFullscreen,
                useScreenSettings = useScreenSettings,
                defaultWidth = defaultWidth,
                defaultHeight = defaultHeight,
                defaultVSyncCount = defaultVSyncCount,
                defaultAntiAliasing = defaultAntiAliasing,
                defaultShadowCascadeCount = defaultShadowCascadeCount
            };
        }

        public void Initialize(ANFSettings settings)
        {
            Reset();
        }

        public void Reset()
        {
            if (useScreenSettings)
            {
                resolution = Screen.currentResolution;
                fullscreen = Screen.fullScreen;
                vSyncCount = QualitySettings.vSyncCount = vSyncCount;

                UniversalRenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                antiAliasing = pipelineAsset.msaaSampleCount;
                shadowCascadeCount = pipelineAsset.shadowCascadeCount;
            }
            else
            {
                resolution = new Resolution() { height = defaultHeight, width = defaultWidth, refreshRateRatio = Screen.currentResolution.refreshRateRatio };

                SetFullscreen(defaultFullscreen);
                SetResolution(resolution);
                SetVSyncCount(defaultVSyncCount);
                SetAntiAliasing(antiAliasing);
                SetShadowQuality(shadowCascadeCount);
            }
        }

        public void Load(JSON json)
        {
            int width = resolution.width;
            int height = resolution.height;
            uint denominator = resolution.refreshRateRatio.denominator;
            uint numerator = resolution.refreshRateRatio.numerator;

            if (json.ContainsKey("fullscreen"))
                fullscreen = json.GetBool("fullscreen");

            if (json.ContainsKey("vSyncCount"))
                vSyncCount = json.GetInt("vSyncCount");

            if (json.ContainsKey("antiAliasing"))
                antiAliasing = json.GetInt("antiAliasing");

            if (json.ContainsKey("shadows"))
                shadowCascadeCount = json.GetInt("shadows");

            if (json.ContainsKey("width"))
                width = json.GetInt("width");
            if (json.ContainsKey("height"))
                height = json.GetInt("height");
            if (json.ContainsKey("refreshRateDenomiator"))
                denominator = json.GetJNumber("refreshRateDenomiator").AsUInt();
            if (json.ContainsKey("refreshRateNumerator"))
                numerator = json.GetJNumber("refreshRateNumerator").AsUInt();

            SetResolution(new Resolution()
            {
                height = height,
                width = width,
                refreshRateRatio = new() { denominator = denominator, numerator = numerator }
            });
            SetFullscreen(fullscreen);
            SetVSyncCount(vSyncCount);
            SetAntiAliasing(antiAliasing);
            SetShadowQuality(shadowCascadeCount);
        }

        public void Save(JSON json)
        {
            json.Add("fullscreen", fullscreen);
            json.Add("width", resolution.width);
            json.Add("height", resolution.height);
            json.Add("refreshRateDenomiator", resolution.refreshRateRatio.denominator);
            json.Add("refreshRateNumerator", resolution.refreshRateRatio.numerator);
            json.Add("vSyncCount", vSyncCount);
            json.Add("antiAliasing", antiAliasing);
            json.Add("shadows", shadowCascadeCount);
        }

        /// <summary>
        /// Sets the game's resolution
        /// </summary>
        /// <param name="resolution">The new resolution</param>
        public void SetResolution(Resolution resolution)
        {
            this.resolution = resolution;
            Screen.SetResolution(resolution.width, resolution.height,
                fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
                resolution.refreshRateRatio);
        }

        /// <summary>
        /// Sets if the game is in fullscreen or not
        /// </summary>
        /// <param name="fullscreen">True if fullscreen</param>
        public void SetFullscreen(bool fullscreen)
        {
            this.fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
		/// Sets the game's vSyncCount (0 to disable, 1 to enable)
		/// </summary>
		/// <param name="vSyncCount">The VSync Count</param>
        public void SetVSyncCount(int vSyncCount)
        {
            this.vSyncCount = vSyncCount;
            QualitySettings.vSyncCount = vSyncCount;
        }

        /// <summary>
		/// Sets the game's antialiasing (0, 2 4 and 8)
		/// </summary>
		/// <param name="antiAliasing">The antialiasing value</param>
        public void SetAntiAliasing(int antiAliasing)
        {
            this.antiAliasing = antiAliasing;
            UniversalRenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            pipelineAsset.msaaSampleCount = antiAliasing;
        }

        /// <summary>
		/// Sets the game's shadow quality
		/// </summary>
		/// <param name="cascadeCount">The shadow's cascade count</param>
        public void SetShadowQuality(int cascadeCount)
        {
            this.shadowCascadeCount = cascadeCount;
            UniversalRenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            pipelineAsset.shadowCascadeCount = cascadeCount;
        }

        /// <summary>
        /// Gets if the game is in fullscreen or not
        /// </summary>
        /// <returns>True if in fullscreen</returns>
        public bool IsFullscreen()
        {
            return fullscreen;
        }

        /// <summary>
        /// Gets the current game's resolution
        /// </summary>
        /// <returns>The current resolution</returns>
        public Resolution GetResolution()
        {
            return resolution;
        }

        /// <summary>
		/// Gets the current vsync count
		/// </summary>
		/// <returns>The Vsync count</returns>
        public int GetVSyncCount()
        {
            return vSyncCount;
        }

        /// <summary>
		/// Gets the game's antialiasing value
		/// </summary>
		/// <returns>The antialiasing value</returns>
        public int GetAntiAliasing()
        {
            return antiAliasing;
        }

        /// <summary>
		/// Gets the shadow's quality
		/// </summary>
		/// <returns>The shadow's quality</returns>
        public int GetShadowQuality()
        {
            return shadowCascadeCount;
        }
    }
}
