using ANF.Utils;

namespace ANF.Scene
{
    /// <summary>
	/// Handles multiple ANFComponents
	/// </summary>
    public abstract class ANFComponentManager<T> : DataManager<T> where T : ANFComponent
    {
        public void OnInitialize()
        {
            foreach (T component in components.Values)
                component.OnInitialize();
        }

        public virtual void OnUpdate()
        {
            foreach (T component in components.Values)
                component.OnUpdate();
        }

        public void OnStart()
        {
            foreach (T component in components.Values)
                component.OnStart();
        }

        /// <summary>
        /// Changes if all components are enabled or not
        /// </summary>
        /// <param name="enabled">True if all components should be enabled</param>
        public void SetEnabledAll(bool enabled)
        {
            foreach (T component in components.Values)
                component.SetEnabled(enabled);
        }

        /// <summary>
        /// Changes if all components are paused or not
        /// </summary>
        /// <param name="paused">True if all components should be paused</param>
        public void SetPausedAll(bool paused)
        {
            foreach (T component in components.Values)
                component.SetPaused(paused);
        }

        /// <summary>
        /// Unregisters all component inputs. Use this when changing scenes.
        /// </summary>
        public void UnRegisterAllInputs()
        {
            foreach (T component in components.Values)
                component.OnUnRegisterInputs();
        }

        /// <summary>
        /// Calls the On Change Scene callback on all components
        /// </summary>
        public void OnChangeScene()
        {
            foreach (T component in components.Values)
                component.OnChangeScene();
        }
    }
}
