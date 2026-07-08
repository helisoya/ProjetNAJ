using ANF.Utils;
using AYellowpaper.SerializedCollections;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ANF.Persistent
{
    /// <summary>
	/// Handles ANF Inputs and visual icons
	/// </summary>
    public class ANFInput : MonoBehaviour, Jsonable
    {
        [SerializeField] private InputActionAsset inputMap;

        [Header("Icons")]
        [SerializeField] private GamepadIcons xboxIcons;
        [SerializeField] private SerializedDictionary<string, Sprite> keyboardIcons;
        public Sprite defaultIcon;
        private PlayerInput playerInput;
        private UnityEvent<string> onDeviceChange;

        /// <summary>
		/// Initialize the input system
		/// </summary>
        public void Initialize()
        {
            onDeviceChange = new UnityEvent<string>();

            playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = inputMap;
            playerInput.neverAutoSwitchControlSchemes = false;
            playerInput.defaultActionMap = "Player";
            playerInput.ActivateInput();

            OnControlsChanged(playerInput);
        }

        /// <summary>
		/// Triggers a fake scheme change (will use the player input's current scheme)
		/// </summary>
        public void TriggerSchemeChange()
        {
            OnControlsChanged(playerInput);
        }

        /// <summary>
        /// On Controls changed callback
        /// </summary>
        /// <param name="input">The player input</param>
        void OnControlsChanged(PlayerInput input)
        {
            if (playerInput == null)
                return;

            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);

            onDeviceChange.Invoke(input.currentControlScheme);
        }

        /// <summary>
        /// Registers an action when the control scheme changes
        /// </summary>
        /// <param name="action">The action</param>
        public void RegisterOnControlSchemeChange(UnityAction<string> action)
        {
            onDeviceChange.AddListener(action);
            action.Invoke(playerInput.currentControlScheme);
        }

        /// <summary>
		/// Unregisters an anction when the control scheme changes
		/// </summary>
		/// <param name="action">The action</param>
        public void UnregisterOnControlSchemeChange(UnityAction<string> action)
        {
            onDeviceChange.RemoveListener(action);
        }

        /// <summary>
        /// Gets the player input
        /// </summary>
        /// <returns>The player input</returns>
        public PlayerInput GetInput()
        {
            return playerInput;
        }

        /// <summary>
        /// Gets an input's icon
        /// </summary>
        /// <param name="deviceLayoutName">The device name</param>
        /// <param name="controlPath">The input's path</param>
        /// <returns>The sprite if it exists</returns>
        public Sprite GetIcon(string deviceLayoutName, string controlPath)
        {
            //if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
            //    icon = ps4.GetSprite(controlPath);
            if (deviceLayoutName.Equals("Gamepad"))
            {
                Sprite sprite = xboxIcons.GetSprite(controlPath);
                if (sprite != null)
                    return sprite;
            }
            else
            {
                if (keyboardIcons.TryGetValue(controlPath, out Sprite icon))
                    return icon;
            }

            return defaultIcon;
        }

        public void Save(JSON json)
        {
            json.Add("bindings", playerInput.actions.SaveBindingOverridesAsJson());
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("bindings"))
                playerInput.actions.LoadBindingOverridesFromJson(json.GetString("bindings"));
        }
    }
}
