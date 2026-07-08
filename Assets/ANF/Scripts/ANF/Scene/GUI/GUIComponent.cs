using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a GUI Component
    /// A component can be opened or closed, which will be saved.
    /// It can also be enabled and disabled, which will restrict some of its functions, but will not be saved.
    /// You may use isEnabled to disable components when in a pause menu, without compromising a potential save
    /// </summary>
    [System.Serializable]
    public abstract class GUIComponent : MonoBehaviour, ANFComponent
    {
        [Header("General")]
        [SerializeField] protected bool canBeSaved = true;
        [SerializeField] protected bool enabledByDefault = true;
        [SerializeField] protected GameObject root;
        [SerializeField] protected bool hideRootWhenClosed = true;
        [Tooltip("A component may be opened only if all its lock components are closed. For instance, the load menu cannot be opened if the map menu is open")]
        [SerializeField] protected string[] lockComponents;

        public bool isEnabled { get; protected set; } = true;
        public bool isPaused { get; protected set; } = false;

        protected ANFManager manager;
        protected GUIManager gui;

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(ANFManager manager, GUIManager gui)
        {
            this.manager = manager;
            this.gui = gui;
            isEnabled = false;
            isPaused = false;
            OnInitialize();

            root.SetActive(!hideRootWhenClosed);

            if (enabledByDefault)
                SetEnabled(true);
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled && gui.AnyComponentsActive(lockComponents))
                return;

            if (enabled != isEnabled)
            {
                isPaused = false;
                isEnabled = enabled;
                root.SetActive(!hideRootWhenClosed || isEnabled);

                if (isEnabled)
                {
                    OnRegisterInputs();
                    OnEnabled();
                }
                else
                {
                    OnUnRegisterInputs();
                    OnDisabled();
                }
            }
        }

        public void SetPaused(bool paused)
        {
            bool newValue = isEnabled && paused;

            if (newValue != isPaused)
            {
                isPaused = newValue;
                if (newValue)
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
            {
                bool open = json.GetBool("isEnabled");
                enabled = !open;
                SetEnabled(open);
            }
        }

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
