using ANF.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a visual tab in the settings menu
    /// </summary>
    public class SettingsTabUI : MonoBehaviour
    {
        [SerializeField] protected Locals.LocalizedText tabNameText;
        [SerializeField] protected RectTransform root;
        [SerializeField] protected GameObject resetRoot;
        [SerializeField] protected Button resetButton;
        protected List<Selectable> selectables = new List<Selectable>();

        /// <summary>
		/// Adds a new selectable to the owned selectables
		/// </summary>
		/// <param name="selectable">The selectable</param>
        public void AddSelectable(Selectable selectable)
        {
            selectables.Add(selectable);
        }

        /// <summary>
		/// Gets the owned selectables
		/// </summary>
		/// <returns>The owned selectables</returns>
        public List<Selectable> GetSelectables()
        {
            return selectables;
        }

        /// <summary>
        /// Sets the tab's label
        /// </summary>
        /// <returns>The label's key</returns>
        public void SetLabelKey(string labelKey)
        {
            tabNameText.SetNewKey(labelKey);
        }


        /// <summary>
        /// Gets the tab's root
        /// </summary>
        /// <returns>The tab's root</returns>
        public RectTransform GetRoot()
        {
            return root;
        }

        /// <summary>
        /// Gets the tab's reset button
        /// </summary>
        /// <returns>The reset button</returns>
        public Button GetResetButton()
        {
            return resetButton;
        }

        /// <summary>
        /// Registers a new reset action for the tab's reset button
        /// </summary>
        /// <param name="resetAction">The reset action</param>
        public void RegisterResetAction(UnityAction resetAction)
        {
            resetButton.onClick.AddListener(resetAction);
            resetRoot.SetActive(true);
        }

        /// <summary>
        /// Rebuild the component's layout
        /// </summary>
        public void Rebuild()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }

}
