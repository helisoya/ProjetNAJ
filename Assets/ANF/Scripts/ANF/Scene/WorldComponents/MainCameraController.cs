using ANF.GUI;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the main camera movements
    /// </summary>
    [System.Serializable]
    public class MainCameraController : WorldComponent
    {
        [Header("Infos")]
        [SerializeField] private Vector3 startPosition = new Vector3(0, 0, -10);
        [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);

        [Header("Screenshake")]
        [SerializeField] private string[] uiToShake;
        [SerializeField] private float shakeWorldToUIRation = 100.0f;
        private float shakeStrength;
        private float shakeTime;
        public bool Shaking { get; private set; }

        private Transform cameraTransform;
        private LerpInstanceVector3 lerpRotation;
        private LerpInstanceVector3 lerpPosition;
        private bool skipModeEnabled;
        private Vector3 currentRotation;
        private Vector3 currentPosition;

        public bool Rotating
        {
            get
            {
                return lerpRotation != null && lerpRotation.lerping;
            }
        }

        public bool Moving
        {
            get
            {
                return lerpPosition != null && lerpPosition.lerping;
            }
        }

        /// <summary>
		/// Shakes the screen
		/// </summary>
		/// <param name="strength">The screenshake's strength</param>
		/// <param name="duration">The screenshake's duration</param>
		/// <param name="shakeCamera">True if the camera should be shaken</param>
		/// <param name="shakeUI">True if the UI should be shaken</param>
        public void ShakeScreen(float strength, float duration, bool shakeCamera = true, bool shakeUI = true)
        {
            if (shakeCamera)
            {
                Shaking = true;
                shakeStrength = strength;
                shakeTime = duration;
            }

            if (shakeUI)
            {
                foreach (string component in uiToShake)
                {
                    if (manager.GetGUIManager().GetComponent(component, out GUIComponent guiComponent))
                    {
                        RectTransform rectTransform = guiComponent.GetComponent<RectTransform>();
                        Vector2 lastAnchoredPosition = rectTransform.anchoredPosition;
                        rectTransform.DOShakeAnchorPos(duration, strength * shakeWorldToUIRation).OnComplete(() => { rectTransform.anchoredPosition = lastAnchoredPosition; });
                    }
                }
            }
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (lerpPosition != null && lerpPosition.lerping)
                lerpPosition.ChangeDuration(0.1f);
            if (lerpRotation != null && lerpRotation.lerping)
                lerpRotation.ChangeDuration(0.1f);
            if (Shaking && shakeTime > 0.1f)
                shakeTime = 0.1f;
        }

        public override WorldComponent CloneComponent()
        {
            return new MainCameraController()
            {
                startPosition = startPosition,
                startRotation = startRotation,
                uiToShake = uiToShake
            };
        }

        public override void OnInitialize()
        {
            cameraTransform = Camera.main.transform;
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            if (lerpPosition != null && lerpPosition.lerping)
            {
                currentPosition = lerpPosition.Update();
                cameraTransform.position = currentPosition;
            }

            if (lerpRotation != null && lerpRotation.lerping)
            {
                currentRotation = lerpRotation.Update();
                cameraTransform.eulerAngles = currentRotation;
            }

            if (Shaking)
            {
                shakeTime -= Time.deltaTime;
                if (shakeTime <= 0)
                {
                    Shaking = false;
                    cameraTransform.position = currentPosition;
                }
                else
                {
                    Vector2 randCircle = Random.insideUnitCircle * shakeStrength;

                    cameraTransform.position = new Vector3(
                        currentPosition.x + randCircle.x,
                        currentPosition.y + randCircle.y,
                        currentPosition.z);
                }
            }
        }

        /// <summary>
		/// Gets the camera's default position
		/// </summary>
		/// <returns>The default position</returns>
        public Vector3 GetDefaultPosition()
        {
            return startPosition;
        }

        /// <summary>
		/// Gets the camera's default rotation
		/// </summary>
		/// <returns>The default rotation</returns>
        public Vector3 GetDefaultRotation()
        {
            return startRotation;
        }

        /// <summary>
        /// Sets the camera's position. Can be immediate or over time
        /// </summary>
        /// <param name="position">The new position</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The movement's duration if not immediate</param>
        public void SetPosition(Vector3 position, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                currentPosition = position;
                cameraTransform.position = position;

                if (lerpPosition != null)
                    lerpPosition.StopLerp();
            }
            else
            {
                if (lerpPosition == null)
                    lerpPosition = new LerpInstanceVector3();

                lerpPosition.StartLerp(cameraTransform.position, position, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
        /// Sets the camera's rotation. Can be immediate or over time
        /// </summary>
        /// <param name="position">The new euler angles</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The movement's duration if not immediate</param>
        public void SetRotation(Vector3 eulerAngles, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                currentRotation = eulerAngles;
                cameraTransform.eulerAngles = eulerAngles;

                if (lerpRotation != null)
                    lerpRotation.StopLerp();
            }
            else
            {
                if (lerpRotation == null)
                    lerpRotation = new LerpInstanceVector3();

                lerpRotation.StartLerp(currentRotation, eulerAngles, skipModeEnabled ? 0.1f : duration);
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
            if (json.ContainsKey("currentPosition"))
                cameraTransform.position = json.GetJArray("currentPosition").AsVector3();

            currentPosition = cameraTransform.position;

            if (json.ContainsKey("currentRotation"))
            {
                currentRotation = json.GetJArray("currentRotation").AsVector3();
                cameraTransform.eulerAngles = currentRotation;
            }

            if (json.ContainsKey("lerpPosition"))
            {
                if (lerpPosition == null)
                    lerpPosition = new LerpInstanceVector3();

                lerpPosition.Load(json.GetJSON("lerpPosition"));
            }

            if (json.ContainsKey("lerpRotation"))
            {
                if (lerpRotation == null)
                    lerpRotation = new LerpInstanceVector3();

                lerpRotation.Load(json.GetJSON("lerpRotation"));
            }

            return true;
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentPosition", currentPosition);
            json.Add("currentRotation", currentRotation);

            if (lerpPosition != null)
            {
                JSON jsonLerp = new JSON();
                lerpPosition.Save(jsonLerp);
                json.Add("lerpPosition", jsonLerp);
            }

            if (lerpRotation != null)
            {
                JSON jsonLerp = new JSON();
                lerpRotation.Save(jsonLerp);
                json.Add("lerpRotation", jsonLerp);
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

        public override bool IsLoadingOrCleaningUp()
        {
            return false;
        }
    }
}
