using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the game's flow (autoplay, skip)
    /// </summary>
    [System.Serializable]
    public class FlowStateHandler : WorldComponent
    {
        private bool autoPlay;
        private bool skipMode;

        private bool autoplayToggle;
        private bool skipModeToggle;

        /// <summary>
        /// True if auto play is enabled
        /// </summary>
        /// <returns></returns>
        public bool IsAutoPlayEnabled()
        {
            return autoPlay;
        }

        /// <summary>
        /// Gets if skip mode is enabled
        /// </summary>
        /// <returns>True if skip mode is enabled</returns>
        public bool IsSkipModeEnabled()
        {
            return skipMode;
        }

        /// <summary>
        /// Toggles the autoplay on/off
        /// </summary>
        public void ToggleAutoPlay()
        {
            autoPlay = !autoPlay;
            manager.GetWorld().Invoke("OnAutoPlayToggle", autoPlay);
            manager.GetGUIManager().Invoke("OnAutoPlayToggle", autoPlay);
        }

        /// <summary>
        /// Toggles the skip mode on/off
        /// </summary>
        public void ToggleSkipMode()
        {
            skipMode = !skipMode;
            manager.GetWorld().Invoke("OnSkipModeToggle", skipMode);
            manager.GetGUIManager().Invoke("OnSkipModeToggle", skipMode);
        }

        private void OnAutoPlayInput(InputAction.CallbackContext context)
        {
            if(!autoplayToggle || context.ReadValueAsButton())
                ToggleAutoPlay();
        }

        private void OnSkipModeInput(InputAction.CallbackContext context)
        {
            if (!skipModeToggle || context.ReadValueAsButton())
                ToggleSkipMode();
        }

        private void OnAutoPlayToggleChange(object value)
        {
            autoplayToggle = (bool)value;
            if (autoplayToggle)
                ToggleAutoPlay();
        }

        private void OnSkipModeToggleChange(object value)
        {
            skipModeToggle = (bool)value;
            if (autoplayToggle)
                ToggleAutoPlay();
        }

        public override WorldComponent CloneComponent()
        {
            return new FlowStateHandler();
        }

        public override void OnInitialize()
        {
            autoPlay = false;
            skipMode = false;

            if(PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                autoplayToggle = (bool)settings.Register("FlowStateHandler_ToggleAutoplay", SettingsContainer.SettingsDataType.Bool, OnAutoPlayToggleChange);
                skipModeToggle = (bool)settings.Register("FlowStateHandler_ToggleSkipMode", SettingsContainer.SettingsDataType.Bool, OnSkipModeToggleChange);
            }
        }

        public override void OnStart()
        {
            OnRegisterInputs();
        }

        public override void OnUpdate()
        {

        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnDisabled()
        {

        }

        public override void OnSave(JSON json)
        {

        }

        public override void OnLoad(JSON json)
        {

        }

        public override void OnRegisterInputs()
        {
            PlayerInput playerInput = PersistentDataManager.instance.GetANFInput().GetInput();
            playerInput.actions.FindAction("AutoPlay").performed += OnAutoPlayInput;
            playerInput.actions.FindAction("SkipMode").performed += OnSkipModeInput;
        }

        public override void OnUnRegisterInputs()
        {
            PlayerInput playerInput = PersistentDataManager.instance.GetANFInput().GetInput();
            playerInput.actions.FindAction("AutoPlay").performed -= OnAutoPlayInput;
            playerInput.actions.FindAction("SkipMode").performed -= OnSkipModeInput;
        }

        public override void OnChangeScene()
        {
            OnUnRegisterInputs();

            if (PersistentDataManager.instance.GetGlobalData().GetComponent(out SettingsContainer settings))
            {
                settings.Unregister("FlowStateHandler_ToggleAutoplay", OnAutoPlayToggleChange);
                settings.Unregister("FlowStateHandler_ToggleSkipMode", OnSkipModeToggleChange);
            }
        }
    }
}

