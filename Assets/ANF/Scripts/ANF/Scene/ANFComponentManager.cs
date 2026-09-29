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
                component.UpdateComponent();
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
        /// <returns>True if the cleanup is done</returns>
        public bool OnChangeScene()
        {
            bool finished = true;
            foreach (T component in components.Values)
                if (!component.OnChangeScene())
                    finished = false;

            return finished;
        }

        /// <summary>
        /// Checks if some components are still cleaning up for scene change
        /// </summary>
        /// <returns>True if the cleanup is still ongoing</returns>
        public bool IsLoadingOrCleaningUp()
        {
            bool cleaningUp = false;
            foreach (T component in components.Values)
                if (component.IsLoadingOrCleaningUp())
                    cleaningUp = true;

            return cleaningUp;
        }
    }
}
