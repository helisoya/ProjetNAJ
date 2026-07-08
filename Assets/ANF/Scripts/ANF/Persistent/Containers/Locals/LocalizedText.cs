using ANF.Persistent;
using TMPro;
using UnityEngine;

namespace ANF.Locals
{
    /// <summary>
    /// Represents a text that is localized
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] protected string localKey;
        [SerializeField] protected Locals.Channel channel;
        [SerializeField] protected Locals.TextType type;
        [SerializeField] protected TMP_Text text;

        [Header("Checks")]
        [Tooltip("True if the text is affected by the size system")]
        [SerializeField] protected bool canResize = true;
        [Tooltip("True if the text is affected by the color system")]
        [SerializeField] protected bool canRecolor = true;
        [Tooltip("True if the text should not be reloaded in any way (no localization AND no injection)")]
        [SerializeField] protected bool noReload = false;
        [Tooltip("True if the text should not be localized (It will still be subject to injection)")]
        [SerializeField] protected bool disableLocalization = false;
        protected object[] injectors;
        protected Locals locals;

        private bool alreadyRegistered = false;

        void Start()
        {
            RegisterText();
        }

        protected void OnDestroy()
        {
            if (locals != null)
                locals.UnregisterText(this, channel);
        }

        /// <summary>
        /// Registers the text. Called automatically in a Start() function. But can also be called manually
        /// </summary>
        public void RegisterText()
        {
            if (alreadyRegistered)
                return;

            alreadyRegistered = true;

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals>(out locals))
                locals.RegisterText(this, channel);

            ReloadText();
        }

        /// <summary>
        /// Changes the ID of the localized text
        /// </summary>
        /// <param name="key">The new ID</param>
        public void SetNewKey(string key)
        {
            localKey = key;
            ReloadText();
        }

        /// <summary>
        /// Reloads the localized text
        /// </summary>
        public virtual void ReloadText()
        {
            if (noReload)
                return;

            string txt = localKey;
            if (locals != null && !disableLocalization)
                txt = locals.GetLocal(localKey);

            if (PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer playerVariableContainer))
                txt = txt.Replace("{MC}", playerVariableContainer.GetPlayerName());

            if (injectors != null && injectors.Length > 0)
            {
                for (int i = 0; i < injectors.Length; i++)
                {
                    txt = txt.Replace(string.Concat("{", i, "}"), injectors[i].ToString());
                }
            }
            text.text = txt;
        }

        /// <summary>
        /// Sets the injectors for this string
        /// </summary>
        /// <param name="newInjectors">The new injectors</param>
        /// <param name="reloadText">True if the text should be immediatly reloaded</param>
        public void SetInjectors(object[] newInjectors, bool reloadText = true)
        {
            injectors = newInjectors;
            if (reloadText) ReloadText();
        }

        /// <summary>
        /// Sets the current font for the text
        /// </summary>
        /// <param name="font">The new font</param>
        public void SetFont(TMP_FontAsset font)
        {
            text.font = font;
        }

        /// <summary>
        /// Sets if the text can be reloaded using the key system or not
        /// </summary>
        /// <param name="canReload">True if the text is shown</param>
        /// <param name="reloadText">True if the visual should be reloaded</param>
        public void SetCanReload(bool canReload, bool reloadText = true)
        {
            noReload = !canReload;
            if (reloadText) ReloadText();
        }

        /// <summary>
        /// Sets if the text should display localized text or the raw key
        /// </summary>
        /// <param name="enabled">True for localized text</param>
        /// <param name="reloadText">True if the visual should be reloaded/param>
        public void SetLocalizationEnabled(bool enabled, bool reloadText = true)
        {
            disableLocalization = !enabled;
            if (reloadText) ReloadText();
        }

        /// <summary>
        /// Returns the text field
        /// </summary>
        /// <returns>The text field</returns>
        public TMP_Text GetText()
        {
            return text;
        }

        /// <summary>
        /// Changes the text's color
        /// </summary>
        /// <param name="color">The new color</param>
        public void SetColor(Color color)
        {
            if (!canRecolor) return;

            text.color = color;
        }

        /// <summary>
        /// Sets the font size
        /// </summary>
        /// <param name="size">The new size</param>
        public void SetSize(int size)
        {
            if (!canResize) return;

            text.fontSize = size;
            text.fontSizeMax = size;
        }

        public string Key { get { return localKey; } }
        public Locals.Channel Channel { get { return channel; } }
        public Locals.TextType Type { get { return type; } }
    }
}
