using ANF.Scene;
using ANF.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Handles the ANF GUI
    /// </summary>
    public class GUIManager : ANFComponentManager<GUIComponent>
    {
        private RectTransform uiRoot;
        private ANFManager manager;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="manager">The ANFManager</param>
        /// <param name="uiRoot">The UI's root</param>
        /// <param name="componentsToCopy">The registered UI components</param>
        public GUIManager(ANFManager manager, RectTransform uiRoot, GUIRegisterEntry<GUIComponent>[] componentsToCopy)
        {
            this.uiRoot = uiRoot;
            this.manager = manager;

            components = new Dictionary<string, GUIComponent>();
            foreach (GUIRegisterEntry<GUIComponent> entry in componentsToCopy)
            {
                if (!entry.data)
                    continue;

                string finalId = string.IsNullOrEmpty(entry.id) ? entry.data.GetType().FullName : entry.id;

                GUIComponent instance = Object.Instantiate(entry.data, uiRoot);
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                instance.name = finalId;
                instance.Initialize(manager, this);
                components.Add(finalId, instance);
            }
        }

        /// <summary>
        /// Checks if any of the specified components are open
        /// </summary>
        /// <param name="components">The components to check</param>
        /// <returns>True if any of the specified components is open</returns>
        public bool AnyComponentsActive(string[] componentIds)
        {
            GUIComponent tmp;
            foreach (string componentId in componentIds)
            {
                if (components.TryGetValue(componentId, out tmp))
                    if (tmp.isEnabled)
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Changes if all components are paused or not
        /// </summary>
        /// <param name="paused">True if paused</param>
        public void SetComponentsPaused(string[] componentIds, bool paused)
        {
            GUIComponent tmp;
            foreach (string componentId in componentIds)
            {
                if (components.TryGetValue(componentId, out tmp))
                    tmp.SetPaused(paused);
            }
        }

        public override void OnUpdate()
        {
            foreach (GUIComponent component in components.Values)
                if (component.isEnabled && !component.isPaused)
                    component.OnUpdate();
        }

        /// <summary>
        /// Gets the UI's Root
        /// </summary>
        /// <returns>The UI's root</returns>
        public RectTransform GetRoot()
        {
            return uiRoot;
        }
    }
}

