using ANF.Persistent;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.Utils
{
	/// <summary>
	/// Represents an icon that automatically updates itself on device change
	/// </summary>
	public class AutomaticIcon : MonoBehaviour
	{
		[SerializeField] private Image iconImg;
		[SerializeField] private InputActionReference action;
		[Tooltip("The index of the binding within the action")]
		[SerializeField] private int indexKeyboard;
		[Tooltip("The index of the binding within the action")]
		[SerializeField] private int indexGamepad;

		void Start()
		{
			iconImg.preserveAspect = true;

			PersistentDataManager.instance.GetANFInput().RegisterOnControlSchemeChange(OnDeviceChange);
		}

		void OnDestroy()
		{
			PersistentDataManager.instance.GetANFInput().UnregisterOnControlSchemeChange(OnDeviceChange);
		}

		/// <summary>
		/// Sets the data for this icon
		/// </summary>
		/// <param name="action">The referenced action</param>
		/// <param name="indexKeyboard">The binding index for the keyboard action</param>
		/// <param name="indexGamepad">The binding index for the gamepad action</param>
		public void SetData(InputActionReference action, int indexKeyboard, int indexGamepad)
		{
			this.action = action;
			this.indexKeyboard = indexKeyboard;
			this.indexGamepad = indexGamepad;

			OnDeviceChange(PersistentDataManager.instance.GetANFInput().GetInput().currentControlScheme);
		}

		/// <summary>
		/// Callback for changing the current device
		/// </summary>
		/// <param name="newDevice">The new device</param>
		private void OnDeviceChange(string newDevice)
		{
			if (action == null)
				return;

			int correctIdx = newDevice.Equals("Gamepad") ? indexGamepad : indexKeyboard;
			string controlPath = action.action.bindings[correctIdx].overridePath;
			if (string.IsNullOrEmpty(controlPath)) controlPath = action.action.bindings[correctIdx].path;
			controlPath = controlPath.Split('/', 2)[1];
			iconImg.sprite = PersistentDataManager.instance.GetANFInput().GetIcon(newDevice, controlPath);
		}
	}
}