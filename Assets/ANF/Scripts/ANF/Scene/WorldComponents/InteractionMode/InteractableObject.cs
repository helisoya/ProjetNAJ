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

        [Header("Renderers")]
        [Tooltip("Represents the renderers that will be highlighted when in interaction mode")]
        [SerializeField] private Renderer[] objectRenderers;
        [SerializeField] private Collider interactionCollider;
        private bool hidden;

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

            foreach (Renderer renderer in objectRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasFloat("_HighlightAlpha"))
                    {
                        DOTween.Kill(material, 0);
                        material.DOFloat(alpha, "_HighlightAlpha", 0.5f).SetEase(Ease.OutQuad).SetId(0);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the renderer's highlight strength
        /// </summary>
        /// <param name="strength">The new strength</param>
        public void SetHighlightStrength(float strength)
        {
            if (hidden) return;

            foreach (Renderer renderer in objectRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasFloat("_HighlightStrength"))
                    {
                        material.SetFloat("_HighlightStrength", strength);
                    }
                }
            }
        }

        /// <summary>
        /// Changes the renderer's highlight color
        /// </summary>
        /// <param name="color">The new color</param>
        public void SetHighlightColor(Color color)
        {
            if (hidden) return;

            foreach (Renderer renderer in objectRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasColor("_HighlightColor"))
                    {
                        DOTween.Kill(material, 1);
                        material.DOColor(color, "_HighlightColor", 0.5f).SetEase(Ease.OutQuad).SetId(1);
                    }

                }
            }
        }

        /// <summary>
        /// Changes if the object is hidden or not
        /// </summary>
        /// <param name="value">true if the object should be hidden</param>
        public void SetHidden(bool value)
        {
            hidden = value;
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
            foreach (Renderer renderer in objectRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    DOTween.Kill(material, 0);
                    DOTween.Kill(material, 1);
                }
            }
        }
    }

}
