using ANF.Utils;
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
        private Transform cameraTransform;
        private LerpInstanceVector3 lerpRotation;
        private LerpInstanceVector3 lerpPosition;
        private bool skipModeEnabled;

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

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (lerpPosition != null && lerpPosition.lerping)
                lerpPosition.ChangeDuration(0.1f);
            if (lerpRotation != null && lerpRotation.lerping)
                lerpRotation.ChangeDuration(0.1f);
        }

        public override WorldComponent CloneComponent()
        {
            return new MainCameraController()
            {
                startPosition = startPosition,
                startRotation = startRotation
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
                cameraTransform.position = lerpPosition.Update();
            }

            if (lerpRotation != null && lerpRotation.lerping)
            {
                cameraTransform.eulerAngles = lerpRotation.Update();
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
                cameraTransform.position = position;
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
                cameraTransform.eulerAngles = eulerAngles;
            }
            else
            {
                if (lerpRotation == null)
                    lerpRotation = new LerpInstanceVector3();

                lerpRotation.StartLerp(cameraTransform.eulerAngles, eulerAngles, skipModeEnabled ? 0.1f : duration);
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("currentPosition"))
                cameraTransform.position = json.GetJArray("currentPosition").AsVector3();
            if (json.ContainsKey("currentRotation"))
                cameraTransform.eulerAngles = json.GetJArray("currentRotation").AsVector3();

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
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentPosition", cameraTransform.position);
            json.Add("currentRotation", cameraTransform.eulerAngles);

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

        public override void OnChangeScene()
        {

        }
    }
}
