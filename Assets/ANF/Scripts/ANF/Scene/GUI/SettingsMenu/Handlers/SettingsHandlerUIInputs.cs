using ANF.Persistent;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents the inputs tab in the settings
    /// </summary>
    [System.Serializable]
    public class SettingsHandlerUIInputs : SettingsHandlerUI
    {
        [SerializeField] private UIInputBindingTab[] bindingTabs;
        private Dictionary<Guid, Image> registeredButtons;


        public override void PopulateTab()
        {
            registeredButtons = new Dictionary<Guid, Image>();
            ANFInput anfInput = PersistentDataManager.instance.GetANFInput();
            var tabsEnumerator = bindingTabs.GetEnumerator();
            while (tabsEnumerator.MoveNext())
            {
                UIInputBindingTab tab = (UIInputBindingTab)tabsEnumerator.Current;
                RectTransform tabRoot = menu.GetTab(tab.tabName);

                var bindingEnumerator = tab.bindings.GetEnumerator();
                while (bindingEnumerator.MoveNext())
                {
                    UIInputBinding binding = (UIInputBinding)bindingEnumerator.Current;

                    if (binding.bindingIndex >= binding.inputAction.action.bindings.Count)
                        continue;

                    Button button = menu.CreateInputBinder(binding.labelKey, tabRoot);
                    InputBinding inputbinding = binding.inputAction.action.bindings[binding.bindingIndex];

                    string groups = inputbinding.groups;
                    if (groups.StartsWith(';'))
                        groups = groups.Substring(1);

                    button.image.sprite = anfInput.GetIcon(groups.Split(";")[0], inputbinding.effectivePath.Split("/", 2)[1]);

                    button.onClick.AddListener(() =>
                    {
                        menu.StartRebindingProcess(binding.inputAction, binding.bindingIndex, binding.labelKey, button.image);
                    });

                    registeredButtons.Add(inputbinding.id, button.image);
                }

                menu.RegisterResetAction(tab.tabName, () => { Reset(tab.tabName); });
            }
        }

        /// <summary>
        /// Resets the parameters to their default values
        /// </summary>
        private void Reset(string tabKey)
        {
            ANFInput anfInput = PersistentDataManager.instance.GetANFInput();
            foreach (UIInputBindingTab tab in bindingTabs)
            {
                if (tab.tabName.Equals(tabKey))
                {
                    foreach (UIInputBinding binding in tab.bindings)
                    {
                        if (binding.bindingIndex < binding.inputAction.action.bindings.Count)
                        {
                            binding.inputAction.action.RemoveBindingOverride(binding.bindingIndex);
                            InputBinding inputBinding = binding.inputAction.action.bindings[binding.bindingIndex];
                            if (registeredButtons.TryGetValue(inputBinding.id, out Image image))
                            {
                                string groups = inputBinding.groups;
                                if (groups.StartsWith(';'))
                                    groups = groups.Substring(1);

                                image.sprite = anfInput.GetIcon(groups.Split(";")[0], inputBinding.effectivePath.Split("/", 2)[1]);
                            }

                        }
                    }
                    break;
                }
            }
        }

        public override void RedrawLocalizedElements()
        {
        }
    }

    /// <summary>
	/// A tab containing input bindings
	/// </summary>
    [System.Serializable]
    public struct UIInputBindingTab
    {
        public string tabName;
        public UIInputBinding[] bindings;
    }

    /// <summary>
    /// A UI Input Binding inside a tab
    /// </summary>
    [System.Serializable]
    public struct UIInputBinding
    {
        public InputActionReference inputAction;
        public string labelKey;
        public int bindingIndex;
    }
}
