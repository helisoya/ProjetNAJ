using ANF.Utils;
using Leguar.TotalJSON;

namespace ANF.Scene
{
    /// <summary>
	/// ANF Components contains a variety of utility functions and are Jsonable
	/// </summary>
    public interface ANFComponent : Jsonable
    {
        /// <summary>
		/// On Initialize callback.
        /// Called when the component is created
		/// </summary>
        public abstract void OnInitialize();

        /// <summary>
        /// On Update callback.
        /// Called once per frame
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
		/// On Start callback.
        /// Called when all components are created
		/// </summary>
        public abstract void OnStart();

        /// <summary>
        /// On Paused callback.
        /// Called when the component is paused (paused state is not saved)
        /// </summary>
        public abstract void OnPaused();

        /// <summary>
        /// On UnPaused callback.
        /// Called when the component is unpaused (paused state is not saved)
        /// </summary>
        public abstract void OnUnPaused();

        /// <summary>
        /// On Enabled callback.
        /// Called when the component is enabled (enabled state is saved)
        /// </summary>
        public abstract void OnEnabled();

        /// <summary>
        /// On Disabled callback.
        /// Called when the component is disabled (enabled state is saved)
        /// </summary>
        public abstract void OnDisabled();

        /// <summary>
        /// Called when inputs should be registered
        /// </summary>
        public abstract void OnRegisterInputs();

        /// <summary>
        /// Called when inputs should be unregistered (component is disabled, scene change, ...)
        /// </summary>
        public abstract void OnUnRegisterInputs();

        /// <summary>
		/// Called before changing scenes
		/// </summary>
        public abstract void OnChangeScene();

        /// <summary>
        /// Changes if the component is enabled or not
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        public void SetEnabled(bool enabled);

        /// <summary>
        /// Changes if the component is paused or not (not saved)
        /// </summary>
        /// <param name="paused">True if paused</param>
        public void SetPaused(bool paused);

        public abstract void OnSave(JSON json);
        public abstract void OnLoad(JSON json);
    }
}

