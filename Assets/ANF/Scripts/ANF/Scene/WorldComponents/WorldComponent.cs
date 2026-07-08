using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
	/// Represent a world component
    /// Ex : The ANSL manager, the Background, the Characters, ...
    /// By default, isEnabled is used as a temporary way to halt a component. It won't be saved and should be used to pause components when in menus, etc...
	/// </summary>
    [System.Serializable]
    public abstract class WorldComponent : ANFComponent
    {
        [Header("General")]
        [SerializeField] protected bool canBeSaved = true;
        [SerializeField] protected bool enabledByDefault = true;
        public bool isEnabled { get; protected set; } = true;
        public bool isPaused { get; protected set; } = false;

        protected ANFManager manager;

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager)
        {
            this.manager = manager;
            isEnabled = enabledByDefault;
            isPaused = false;
            OnInitialize();
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled != isEnabled)
            {
                isEnabled = enabled;
                isPaused = false;
                if (isEnabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        public void SetPaused(bool paused)
        {
            bool newValue = isEnabled && paused;

            if (newValue != isPaused)
            {
                isPaused = newValue;
                if (isPaused)
                    OnPaused();
                else
                    OnUnPaused();
            }
        }

        public void Save(JSON json)
        {
            if (!canBeSaved)
                return;

            json.Add("isEnabled", isEnabled);
            OnSave(json);
        }

        public void Load(JSON json)
        {
            if (!canBeSaved)
                return;

            OnLoad(json);

            if (json.ContainsKey("isEnabled"))
                isEnabled = json.GetBool("isEnabled");
        }

        /// <summary>
        /// Clones the component
        /// </summary>
        /// <returns>The cloned component</returns>
        public abstract WorldComponent CloneComponent();

        public abstract void OnInitialize();
        public abstract void OnUpdate();
        public abstract void OnStart();
        public abstract void OnPaused();
        public abstract void OnUnPaused();
        public abstract void OnEnabled();
        public abstract void OnDisabled();
        public abstract void OnSave(JSON json);
        public abstract void OnLoad(JSON json);
        public abstract void OnRegisterInputs();
        public abstract void OnUnRegisterInputs();
        public abstract void OnChangeScene();
    }
}
