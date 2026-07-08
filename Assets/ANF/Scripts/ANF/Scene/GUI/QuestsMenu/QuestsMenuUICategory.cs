using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a category in the Quests menu
    /// </summary>
    public class QuestsMenuUICategory : MonoBehaviour
    {
        [SerializeField] private Locals.LocalizedText label;

        /// <summary>
        /// Sets the label's key
        /// </summary>
        /// <param name="key">The new key</param>
        public void SetLabelKey(string key)
        {
            label.SetNewKey(key);
        }
    }
}
