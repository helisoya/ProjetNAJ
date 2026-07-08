using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a entry in the settings menu
    /// </summary>
    public class SettingsEntryUI<T> : MonoBehaviour where T : Selectable
    {
        [SerializeField] protected Locals.LocalizedText label;
        [SerializeField] protected T item;

        /// <summary>
        /// Sets the entry's label
        /// </summary>
        /// <param name="labelKey">The label's key</param>
        public void SetLabel(string labelKey)
        {
            label.SetNewKey(labelKey);
        }

        /// <summary>
        /// Gets the entry's item
        /// </summary>
        /// <returns></returns>
        public T GetItem()
        {
            return item;
        }


    }
}

