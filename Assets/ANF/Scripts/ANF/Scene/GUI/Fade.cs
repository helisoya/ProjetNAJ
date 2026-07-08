using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// A fade system that can fade in & out the screen
    /// </summary>
    [System.Serializable]
    public class Fade : GUIComponent
    {
        [Header("Fade")]
        [SerializeField] private Image fadeImg;
        [SerializeField] private CanvasGroup canvasGroup;

        private LerpInstanceFloat lerpAlpha;
        private LerpInstanceColor lerpColor;

        private bool skipModeEnabled;

        public bool fadingAlpha
        {
            get
            {
                return lerpAlpha != null && lerpAlpha.lerping;
            }
        }

        public bool fadingColor
        {
            get
            {
                return lerpColor != null && lerpColor.lerping;
            }
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;

            if (enabled)
            {
                if (lerpAlpha != null && lerpAlpha.lerping)
                    lerpAlpha.ChangeDuration(0.1f);

                if (lerpColor != null && lerpColor.lerping)
                    lerpColor.ChangeDuration(0.1f);
            }
        }

        /// <summary>
		/// Starts an alpha transition
		/// </summary>
		/// <param name="target">The Alpha target</param>
		/// <param name="immediate">True if the transition should be immediate</param>
		/// <param name="transitionDuration">The transition's duration if not immediate</param>
        public void FadeAlphaTo(float target, bool immediate = false, float transitionDuration = 1.0f)
        {
            if (immediate)
            {
                canvasGroup.alpha = target;
            }
            else
            {
                if (lerpAlpha == null)
                    lerpAlpha = new LerpInstanceFloat();

                lerpAlpha.StartLerp(canvasGroup.alpha, target, skipModeEnabled ? 0.1f : transitionDuration);
            }
        }

        /// <summary>
        /// Starts a color transition
        /// </summary>
        /// <param name="target">The color target</param>
        /// <param name="immediate">True if the transition should be immediate</param>
        /// <param name="transitionDuration">The transition's duration if not immediate</param>
        public void FadeColorTo(Color target, bool immediate = false, float transitionDuration = 1.0f)
        {
            if (immediate)
            {
                fadeImg.color = target;
            }
            else
            {
                if (lerpColor == null)
                    lerpColor = new LerpInstanceColor();

                lerpColor.StartLerp(fadeImg.color, target, skipModeEnabled ? 0.1f : transitionDuration);
            }
        }

        public override void OnUpdate()
        {
            if (lerpAlpha != null && lerpAlpha.lerping)
            {
                canvasGroup.alpha = lerpAlpha.Update();
            }

            if (lerpColor != null && lerpColor.lerping)
            {
                fadeImg.color = lerpColor.Update();
            }
        }


        public override void OnInitialize()
        {
            // Unused
        }

        public override void OnStart()
        {
            // Unused
        }

        public override void OnDisabled()
        {
            // Unused
        }

        public override void OnEnabled()
        {
            // Unused
        }

        public override void OnPaused()
        {
            // Unused
        }

        public override void OnUnPaused()
        {
            // Unused
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("lerpAlpha"))
            {
                if (lerpAlpha == null)
                    lerpAlpha = new LerpInstanceFloat();
                lerpAlpha.Load(json.GetJSON("lerpAlpha"));
            }

            if (json.ContainsKey("lerpColor"))
            {
                if (lerpColor == null)
                    lerpColor = new LerpInstanceColor();
                lerpColor.Load(json.GetJSON("lerpColor"));
            }

            if (json.ContainsKey("currentAlpha"))
                canvasGroup.alpha = json.GetFloat("currentAlpha");
            if (json.ContainsKey("currentColor"))
                fadeImg.color = json.GetJArray("currentColor").AsColor();
        }

        public override void OnSave(JSON json)
        {
            json.Add("fadingAlpha", fadingAlpha);
            json.Add("fadingColor", fadingColor);

            if (lerpAlpha != null)
            {
                JSON lerpJSON = new JSON();
                lerpAlpha.Save(lerpJSON);
                json.Add("lerpAlpha", lerpJSON);
            }

            if (lerpColor != null)
            {
                JSON lerpJSON = new JSON();
                lerpColor.Save(lerpJSON);
                json.Add("lerpColor", lerpJSON);
            }

            json.Add("currentAlpha", canvasGroup.alpha);
            json.Add("currentColor", fadeImg.color);
        }
    }
}

