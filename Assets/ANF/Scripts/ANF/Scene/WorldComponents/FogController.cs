using ANF.GUI;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the fog system
    /// </summary>
    [System.Serializable]
    public class SceneRenderController : WorldComponent
    {
        [Header("Infos")]
        [SerializeField] private bool defaultFogEnabled = false;
        [SerializeField] private Color defaultFogColor = Color.white;
        [SerializeField] private float defaultFogDensity = 0.05f;
        [SerializeField] private float defaultAmbientIntensity = 1.0f;

        private bool currentEnabled;
        private Color currentColor;
        private float currentDensity;
        private float currentAmbientIntensity;
        private LerpInstanceColor lerpColor;
        private LerpInstanceFloat lerpDensity;
        private LerpInstanceFloat lerpAmbientIntensity;
        private bool skipModeEnabled;

        public bool LerpingColor
        {
            get
            {
                return lerpColor != null && lerpColor.lerping;
            }
        }

        public bool LerpingDensity
        {
            get
            {
                return lerpDensity != null && lerpDensity.lerping;
            }
        }

        public bool LerpingAmbientIntensity
        {
            get
            {
                return lerpAmbientIntensity != null && lerpAmbientIntensity.lerping;
            }
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (lerpDensity != null && lerpDensity.lerping)
                lerpDensity.ChangeDuration(0.1f);
            if (lerpColor != null && lerpColor.lerping)
                lerpColor.ChangeDuration(0.1f);
            if (lerpAmbientIntensity != null && lerpAmbientIntensity.lerping)
                lerpAmbientIntensity.ChangeDuration(0.1f);
        }

        public override WorldComponent CloneComponent()
        {
            return new SceneRenderController()
            {
                defaultFogColor = defaultFogColor,
                defaultFogDensity = defaultFogDensity,
                defaultFogEnabled = defaultFogEnabled,
                defaultAmbientIntensity = defaultAmbientIntensity
            };
        }

        public override void OnInitialize()
        {
            SetFogEnabled(defaultFogEnabled);
            SetFogColor(defaultFogColor);
            SetFogDensity(defaultFogDensity);
            SetAmbientIntensity(defaultAmbientIntensity);
        }

        /// <summary>
		/// Enables / Disables fog
		/// </summary>
		/// <param name="enabled">True if the fog should be visible</param>
        public void SetFogEnabled(bool enabled)
        {
            currentEnabled = enabled;
            RenderSettings.fog = enabled;
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            if (lerpColor != null && lerpColor.lerping)
            {
                currentColor = lerpColor.Update();
                RenderSettings.fogColor = currentColor;
            }

            if (lerpDensity != null && lerpDensity.lerping)
            {
                currentDensity = lerpDensity.Update();
                RenderSettings.fogDensity = currentDensity;
            }

            if (lerpAmbientIntensity != null && lerpAmbientIntensity.lerping)
            {
                defaultAmbientIntensity = lerpAmbientIntensity.Update();
                RenderSettings.ambientIntensity = defaultAmbientIntensity;
            }
        }

        /// <summary>
        /// Sets the fog's color. Can be immediate or over time
        /// </summary>
        /// <param name="Color">The new color</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The lerp's duration if not immediate</param>
        public void SetFogColor(Color color, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                currentColor = color;
                RenderSettings.fogColor = color;

                if (lerpColor != null)
                    lerpColor.StopLerp();
            }
            else
            {
                if (lerpColor == null)
                    lerpColor = new LerpInstanceColor();

                lerpColor.StartLerp(currentColor, color, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
        /// Sets the fog's density. Can be immediate or over time
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The lerp's duration if not immediate</param>
        public void SetFogDensity(float density, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                currentDensity = density;
                RenderSettings.fogDensity = density;

                if (lerpDensity != null)
                    lerpDensity.StopLerp();
            }
            else
            {
                if (lerpDensity == null)
                    lerpDensity = new LerpInstanceFloat();

                lerpDensity.StartLerp(currentDensity, density, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
        /// Sets the ambient lighting intensity. Can be immediate or over time
        /// </summary>
        /// <param name="intensity">The new intensity</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The lerp's duration if not immediate</param>
        public void SetAmbientIntensity(float intensity, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                currentAmbientIntensity = intensity;
                RenderSettings.ambientIntensity = currentAmbientIntensity;

                if (lerpAmbientIntensity != null)
                    lerpAmbientIntensity.StopLerp();
            }
            else
            {
                if (lerpAmbientIntensity == null)
                    lerpAmbientIntensity = new LerpInstanceFloat();

                lerpAmbientIntensity.StartLerp(currentAmbientIntensity, intensity, skipModeEnabled ? 0.1f : duration);
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override bool OnLoad(JSON json)
        {
            if (json.ContainsKey("currentEnabled"))
                SetFogEnabled(json.GetBool("currentEnabled"));

            if (json.ContainsKey("currentColor"))
            {
                currentColor = json.GetJArray("currentColor").AsColor();
                RenderSettings.fogColor = currentColor;
            }

            if (json.ContainsKey("currentDensity"))
            {
                currentDensity = json.GetFloat("currentDensity");
                RenderSettings.fogDensity = currentDensity;
            }

            if (json.ContainsKey("currentAmbientIntensity"))
            {
                currentAmbientIntensity = json.GetFloat("currentAmbientIntensity");
                RenderSettings.ambientIntensity = currentAmbientIntensity;
            }

            if (json.ContainsKey("lerpColor"))
            {
                if (lerpColor == null)
                    lerpColor = new LerpInstanceColor();

                lerpColor.Load(json.GetJSON("lerpColor"));
            }

            if (json.ContainsKey("lerpDensity"))
            {
                if (lerpDensity == null)
                    lerpDensity = new LerpInstanceFloat();

                lerpDensity.Load(json.GetJSON("lerpDensity"));
            }

            if (json.ContainsKey("lerpAmbientIntensity"))
            {
                if (lerpAmbientIntensity == null)
                    lerpAmbientIntensity = new LerpInstanceFloat();

                lerpAmbientIntensity.Load(json.GetJSON("lerpAmbientIntensity"));
            }

            return true;
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentEnabled", currentEnabled);
            json.Add("currentColor", currentColor);
            json.Add("currentDensity", currentDensity);
            json.Add("currentAmbientIntensity", currentAmbientIntensity);

            if (lerpColor != null)
            {
                JSON jsonLerp = new JSON();
                lerpColor.Save(jsonLerp);
                json.Add("lerpColor", jsonLerp);
            }

            if (lerpDensity != null)
            {
                JSON jsonLerp = new JSON();
                lerpDensity.Save(jsonLerp);
                json.Add("lerpDensity", jsonLerp);
            }

            if (lerpAmbientIntensity != null)
            {
                JSON jsonLerp = new JSON();
                lerpAmbientIntensity.Save(jsonLerp);
                json.Add("lerpAmbientIntensity", jsonLerp);
            }
        }


        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnRegisterInputs()
        {

        }

        public override void OnUnRegisterInputs()
        {

        }

        public override bool OnChangeScene()
        {
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }
    }
}
