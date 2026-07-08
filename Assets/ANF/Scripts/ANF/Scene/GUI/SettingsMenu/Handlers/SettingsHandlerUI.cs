using ANF.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a visual tab in the settings menu
    /// </summary>
    [System.Serializable]
    public abstract class SettingsHandlerUI
    {
        protected SettingsMenuUI menu;
        protected ANFManager manager;


        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="menu">The settings menu</param>
        /// <param name="manager">The ANF Manager</param>
        /// <param name="tab">The linked settings tab</param>
        public void Initialize(SettingsMenuUI menu, ANFManager manager)
        {
            this.menu = menu;
            this.manager = manager;

            PopulateTab();
        }

        /// <summary>
        /// Populates the tab
        /// </summary>
        public abstract void PopulateTab();

        /// <summary>
        /// Redraws the tab's content
        /// </summary>
        public abstract void RedrawLocalizedElements();
    }

}
