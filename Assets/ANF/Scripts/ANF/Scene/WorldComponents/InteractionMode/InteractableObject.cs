using DG.Tweening;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Represents an interactable object in the interaction mode
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        [Header("General Informations")]
        [SerializeField] private string ID;
        [Tooltip("The icon display when interacting with the mouse")]
        [SerializeField] private Texture2D icon;
        private string nextScript;
        private float currentAlpha = 0.0f;
        private Color currentColor = Color.blue;
        private Tweener alphaTweener;
        private Tweener colorTweener;
        private MaterialPropertyBlock propertyBlock;

        [Header("Renderers")]
        [Tooltip("Represents the renderers that will be highlighted when in interaction mode")]
        [SerializeField] private Renderer[] objectRenderers;
        [SerializeField] private Collider interactionCollider;
        private bool hidden;

        void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Initialize the component (Editor Script)
        /// </summary>
        public void EditorInit(string charID, Texture2D icon, Renderer[] renderers, Collider interactionCollider)
        {
            ID = charID;
            this.icon = icon;
            this.interactionCollider = interactionCollider;
            objectRenderers = renderers;
        }

        void OnDrawGizmosSelected()
        {
            if (interactionCollider)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetApproximateVisualPosition(), 0.2f);
            }
        }

        /// <summary>
		/// Gets the object's appromixate visual position
		/// </summary>
		/// <returns>Its approximate visual position</returns>
        public Vector3 GetApproximateVisualPosition()
        {
            return interactionCollider.bounds.center;
        }

        /// <summary>
        /// Changes the renderer's highlight alpha
        /// </summary>
        /// <param name="alpha">The new alpha</param>
        public void SetHighlightAlpha(float alpha)
        {
            if (hidden) return;

            if (alphaTweener != null)
                alphaTweener.Kill();

            alphaTweener = DOVirtual.Float(currentAlpha, alpha, 0.5f, (float value) =>
            {
                currentAlpha = value;
                propertyBlock.SetFloat("_HighlightAlpha", value);
                foreach (Renderer renderer in objectRenderers)
                    if (renderer)
                        renderer.SetPropertyBlock(propertyBlock);
            }).SetEase(Ease.OutQuad).OnComplete(() => { alphaTweener = null; }).OnKill(() => { alphaTweener = null; });
        }

        /// <summary>
        /// Sets the renderer's highlight strength
        /// </summary>
        /// <param name="strength">The new strength</param>
        public void SetHighlightStrength(float strength)
        {
            if (hidden) return;

            MaterialPropertyBlock newBlock = new MaterialPropertyBlock();
            newBlock.SetFloat("_HighlightStrength", strength);

            foreach (Renderer renderer in objectRenderers)
                renderer.SetPropertyBlock(newBlock);
        }

        /// <summary>
        /// Changes the renderer's highlight color
        /// </summary>
        /// <param name="color">The new color</param>
        public void SetHighlightColor(Color color)
        {
            if (hidden) return;

            if (colorTweener != null)
                colorTweener.Kill();

            alphaTweener = DOVirtual.Color(currentColor, color, 0.5f, (Color value) =>
            {
                currentColor = value;
                propertyBlock.SetColor("_HighlightColor", value);
                foreach (Renderer renderer in objectRenderers)
                    if (renderer)
                        renderer.SetPropertyBlock(propertyBlock);
            }).SetEase(Ease.OutQuad).OnComplete(() => { alphaTweener = null; }).OnKill(() => { alphaTweener = null; }); ;
        }

        /// <summary>
        /// Changes if the object is hidden or not
        /// </summary>
        /// <param name="value">true if the object should be hidden</param>
        public void SetHidden(bool value)
        {
            hidden = value;
            interactionCollider.enabled = !hidden;
        }

        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public bool GetIsHidden()
        {
            return hidden;
        }

        /// <summary>
        /// Changes the next script to be loaded when interacted with
        /// </summary>
        /// <param name="script">The new script</param>
        public void SetNextScript(string script)
        {
            nextScript = script;
        }

        /// <summary>
        /// Gets the interactable's icon
        /// </summary>
        /// <returns>The icon</returns>
        public Texture2D GetIcon()
        {
            return icon;
        }

        /// <summary>
        /// Gets the interactable's ID
        /// </summary>
        /// <returns>The ID</returns>
        public string GetID()
        {
            return ID;
        }

        /// <summary>
        /// Gets the interactable's next script
        /// </summary>
        /// <returns>The next script</returns>
        public string GetNextScript()
        {
            return nextScript;
        }

        /// <summary>
        /// Stops all tweens on this object
        /// </summary>
        public void StopAllTween()
        {
            if (alphaTweener != null && !alphaTweener.IsComplete())
                alphaTweener.Kill();

            if (colorTweener != null && !colorTweener.IsComplete())
                colorTweener.Kill();
        }
    }
}
