using ANF.GUI;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the main camera movements
    /// </summary>
    [System.Serializable]
    public class MainCameraController : WorldComponent
    {
        private readonly int SCREENSHAKE_TWEENING_ID = -4242;

        [Header("Infos")]
        [SerializeField] private Vector3 startPosition = new Vector3(0, 0, -10);
        [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);

        [Header("Manual Rotation")]
        [SerializeField] private float manualRotationSpeed = 5.0f;
        [SerializeField] private float screenEdgePercentage = 20.0f;
        private Vector2 allowedManualRotation = new Vector2(0, 0);
        private bool canManualRotate = false;
        private float currentManualRotationLerp = 0.5f;
        private Vector2 currentMousePosition = Vector2.zero;
        private Vector3 manualRotationFocalPoint = Vector3.zero;

        [Header("Screenshake")]
        [SerializeField] private string[] uiToShake;
        [SerializeField] private float shakeWorldToUIRation = 100.0f;
        private float shakeStrength;
        private float shakeTime;
        public bool Shaking { get; private set; }

        private Transform cameraTransform;
        private LerpInstanceVector3 lerpRotation;
        private LerpInstanceVector3 lerpPosition;
        private LerpInstanceFloat lerpManualRotate;
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

        public bool IsManualyRotatingViaLerp
        {
            get
            {
                return lerpManualRotate != null && lerpManualRotate.lerping;
            }
        }

        public void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (Moving)
                lerpPosition.ChangeDuration(0.1f);
            if (Rotating)
                lerpRotation.ChangeDuration(0.1f);
            if (IsManualyRotatingViaLerp)
                lerpManualRotate.ChangeDuration(0.1f);
            if (Shaking && shakeTime > 0.1f)
                shakeTime = 0.1f;
        }

        /// <summary>
		/// Gets the current manual pan value
		/// </summary>
		/// <returns>The current value (0-1)</returns>
        public float GetManualPanValue()
        {
            return currentManualRotationLerp;
        }

        public override WorldComponent CloneComponent()
        {
            return new MainCameraController()
            {
                startPosition = startPosition,
                startRotation = startRotation,
                uiToShake = uiToShake,
                allowedManualRotation = allowedManualRotation,
                canManualRotate = canManualRotate,
                manualRotationSpeed = manualRotationSpeed,
                currentManualRotationLerp = currentManualRotationLerp,
                screenEdgePercentage = screenEdgePercentage
            };
        }

        public override void OnInitialize()
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform)
            {
                currentPosition = cameraTransform.position;
                currentRotation = cameraTransform.eulerAngles;
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MousePosition").performed += OnMousePosition;
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                manualRotationSpeed = (float)settings.Register("MainCameraController_ScrollSpeed", SettingsContainer.SettingsDataType.Float, OnManualPanSpeedChange);
        }

        private void OnManualPanSpeedChange(object value)
        {
            manualRotationSpeed = (float)value;
        }

        public override void OnUpdate()
        {
            if (Moving)
            {
                currentPosition = lerpPosition.Update();
                cameraTransform.position = currentPosition;
            }

            if (Rotating)
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


            if (canManualRotate)
            {
                if (IsManualyRotatingViaLerp)
                {
                    currentManualRotationLerp = lerpManualRotate.Update();
                    cameraTransform.rotation = ComputeCameraManualRotation();
                }
                else
                {
                    float edgeSize = Screen.width * (screenEdgePercentage / 100.0f);
                    float side = 0;
                    bool canLoop = allowedManualRotation.x <= -180.0f && allowedManualRotation.y >= 180.0f;
                    if (currentMousePosition.x < edgeSize)
                    {
                        // Go Left
                        side = -1;
                    }
                    else if (currentMousePosition.x > Screen.width - edgeSize)
                    {
                        // Go Right
                        side = 1;
                    }

                    currentManualRotationLerp += Time.deltaTime * side * (manualRotationSpeed / (allowedManualRotation.y - allowedManualRotation.x));

                    if (canLoop)
                    {
                        if (currentManualRotationLerp < 0)
                            currentManualRotationLerp += 1.0f;
                        else if (currentManualRotationLerp > 1)
                            currentManualRotationLerp -= 1.0f;
                    }
                    else
                    {
                        currentManualRotationLerp = Mathf.Clamp(currentManualRotationLerp, 0.0f, 1.0f);
                    }
                }

                cameraTransform.rotation = ComputeCameraManualRotation();
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

        /// <summary>
        /// Changes if the camera can be manually rotated
        /// </summary>
        /// <param name="canRotate">True if the camera can be manually rotated</param>
        /// <param name="skipCurrentLerps">True to end current rotation & movement lerps</param>
        /// <param name="setFocalPoint">True if the focal point should be updated</param>
        public void SetCanManualRotate(bool canRotate, bool skipCurrentLerps = true, bool setFocalPoint = true)
        {
            if (canManualRotate == canRotate)
                return;

            this.canManualRotate = canRotate;

            if (IsManualyRotatingViaLerp)
                lerpManualRotate.StopLerp();

            if (canRotate)
            {
                if (skipCurrentLerps && Rotating)
                    SetRotation(lerpRotation.target);

                if (skipCurrentLerps && Moving)
                    SetPosition(lerpPosition.target);

                if (setFocalPoint)
                {
                    if (Rotating)
                        manualRotationFocalPoint = lerpRotation.target;
                    else
                        manualRotationFocalPoint = currentRotation;

                    currentManualRotationLerp = Mathf.Abs(allowedManualRotation.x) / (allowedManualRotation.y - allowedManualRotation.x);
                }

            }
            else
            {
                Vector3 target = ComputeCameraManualRotation().eulerAngles;

                if (Rotating)
                    lerpRotation.StartLerp(target, lerpRotation.target, lerpRotation.transitionDuration * (1.0f - lerpRotation.t));
                else
                    SetRotation(target);
            }
        }

        /// <summary>
		/// Sets the camera's allowed rotation angles
		/// </summary>
		/// <param name="minDelta">The minimum angle (left)</param>
		/// <param name="maxDelta">The minimum angle (right)</param>
        public void SetAllowedCameraRotation(float minDelta, float maxDelta)
        {
            if (minDelta == allowedManualRotation.x && maxDelta == allowedManualRotation.y)
                return;

            float baseLerp = Mathf.Lerp(allowedManualRotation.x, allowedManualRotation.y, currentManualRotationLerp);

            minDelta = Mathf.Clamp(minDelta, -180f, 0f);
            maxDelta = Mathf.Clamp(maxDelta, 0f, 180f);

            allowedManualRotation = new Vector2(minDelta, maxDelta);

            baseLerp = Mathf.Clamp(baseLerp, minDelta, maxDelta);

            currentManualRotationLerp = (baseLerp - allowedManualRotation.x) / (allowedManualRotation.y - allowedManualRotation.x);
            if (IsManualyRotatingViaLerp)
                lerpManualRotate.StopLerp();
        }

        /// <summary>
		/// Computes the current camera rotation based on manual rotation
		/// </summary>
        private Quaternion ComputeCameraManualRotation()
        {
            float minValue = manualRotationFocalPoint.y + allowedManualRotation.x;
            float maxValue = manualRotationFocalPoint.y + allowedManualRotation.y;

            float baseLerp = Mathf.Lerp(minValue, maxValue, currentManualRotationLerp);

            return Quaternion.Euler(cameraTransform.eulerAngles.x, baseLerp, cameraTransform.eulerAngles.z);
        }

        /// <summary>
		/// Makes the camera look at an object
		/// </summary>
		/// <param name="position">The object's position</param>
		/// <param name="immediate">True if the change should be immediate</param>
		/// <param name="duration">The transition's duration if needed</param>
        public void LookAt(Vector3 position, bool immediate = true, float duration = 1.0f)
        {
            Vector3 directionVector = (position - cameraTransform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionVector);

            if (canManualRotate)
            {
                float sign = Vector3.Dot(directionVector, Quaternion.Euler(manualRotationFocalPoint) * Vector3.forward) < 0.0f ? -1.0f : 1.0f;
                Quaternion baseQuat = Quaternion.Euler(manualRotationFocalPoint);
                float angleDistance = Mathf.Clamp(
                    Quaternion.Angle(baseQuat, targetRotation) * sign,
                    allowedManualRotation.x,
                    allowedManualRotation.y);

                float targetValue = (angleDistance - allowedManualRotation.x) / (allowedManualRotation.y - allowedManualRotation.x);

                if (immediate)
                {
                    currentManualRotationLerp = targetValue;
                }
                else
                {
                    if (lerpManualRotate == null)
                        lerpManualRotate = new LerpInstanceFloat();

                    lerpManualRotate.StartLerp(currentManualRotationLerp, targetValue, duration);
                }
            }
            else
            {
                SetRotation(targetRotation.eulerAngles, immediate, duration);
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
                        DOTween.Kill(rectTransform, SCREENSHAKE_TWEENING_ID, true);
                        Vector2 lastAnchoredPosition = rectTransform.anchoredPosition;
                        rectTransform.DOShakeAnchorPos(duration, strength * shakeWorldToUIRation)
                            .OnComplete(() => { rectTransform.anchoredPosition = lastAnchoredPosition; })
                            .SetId(SCREENSHAKE_TWEENING_ID);
                    }
                }
            }
        }

        public override void OnDisabled()
        {

        }

        public override void OnEnabled()
        {

        }

        private void OnMousePosition(InputAction.CallbackContext context)
        {
            currentMousePosition = context.ReadValue<Vector2>();
            if (IsManualyRotatingViaLerp)
                lerpManualRotate.StopLerp();
        }

        public override bool OnLoad(JSON json)
        {
            if (json.ContainsKey("currentPosition"))
                cameraTransform.position = json.GetJArray("currentPosition").AsVector3();

            if (json.ContainsKey("manualRotationFocalPoint"))
                manualRotationFocalPoint = json.GetJArray("manualRotationFocalPoint").AsVector3();

            currentPosition = cameraTransform.position;

            if (json.ContainsKey("currentRotation"))
            {
                currentRotation = json.GetJArray("currentRotation").AsVector3();
                cameraTransform.eulerAngles = currentRotation;
            }

            if (json.ContainsKey("allowedManualRotation"))
                allowedManualRotation = json.GetJArray("allowedManualRotation").AsVector2();

            if (json.ContainsKey("canManualRotate"))
                canManualRotate = json.GetBool("canManualRotate");

            if (json.ContainsKey("currentManualRotationLerp"))
                currentManualRotationLerp = json.GetFloat("currentManualRotationLerp");

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

            if (json.ContainsKey("lerpManualRotate"))
            {
                if (lerpManualRotate == null)
                    lerpManualRotate = new LerpInstanceFloat();

                lerpManualRotate.Load(json.GetJSON("lerpManualRotate"));
            }

            if (canManualRotate)
                cameraTransform.rotation = ComputeCameraManualRotation();

            return true;
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentPosition", currentPosition);
            json.Add("currentRotation", currentRotation);
            json.Add("allowedManualRotation", allowedManualRotation);
            json.Add("canManualRotate", canManualRotate);
            json.Add("currentManualRotationLerp", currentManualRotationLerp);
            json.Add("manualRotationFocalPoint", manualRotationFocalPoint);

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

            if (lerpManualRotate != null)
            {
                JSON jsonLerp = new JSON();
                lerpManualRotate.Save(jsonLerp);
                json.Add("lerpManualRotate", jsonLerp);
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
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("MousePosition").performed -= OnMousePosition;
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<SettingsContainer>(out SettingsContainer settings))
                settings.Unregister("MainCameraController_ScrollSpeed", OnManualPanSpeedChange);
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }
    }
}
