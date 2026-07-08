using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
	/// Represents a scene object (Static object, character, ...)
	/// </summary>
    public abstract class SceneObject : MonoBehaviour, Jsonable
    {
        [Header("Base")]
        [SerializeField] protected Renderer[] renderers;
        [SerializeField] protected InteractableObject linkedInteraction;

        protected LerpInstanceVector3 lerpPosition;
        protected LerpInstanceVector3 lerpRotation;
        protected LerpInstanceFloat lerpAlpha;
        protected bool skipModeEnabled;

        public bool Moving
        {
            get { return lerpPosition != null && lerpPosition.lerping; }
        }

        public bool Rotating
        {
            get { return lerpRotation != null && lerpRotation.lerping; }
        }

        public bool Fading
        {
            get { return lerpAlpha != null && lerpAlpha.lerping; }
        }

        public virtual void OnSkipModeToggle(bool enabled)
        {
            skipModeEnabled = enabled;
            if (lerpPosition != null && lerpPosition.lerping)
                lerpPosition.ChangeDuration(0.1f);
            if (lerpRotation != null && lerpRotation.lerping)
                lerpRotation.ChangeDuration(0.1f);
            if (lerpAlpha != null && lerpAlpha.lerping)
                lerpAlpha.ChangeDuration(0.1f);
        }

        /// <summary>
		/// Called when the object is created. Setups the object
		/// </summary>
		/// <param name="manager">The ANFManager</param>
        public void Create(ANFManager manager)
        {
            if (linkedInteraction != null && manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode))
            {
                interactionMode.Register(linkedInteraction);
            }

            OnCreate(manager);
        }

        /// <summary>
		/// Called when the object is removed. Cleans the object.
		/// </summary>
		/// <param name="manager">The ANF Manager</param>
        public void Remove(ANFManager manager)
        {
            if (linkedInteraction != null && manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode))
            {
                interactionMode.UnRegister(linkedInteraction);
            }

            OnRemove(manager);
        }

        /// <summary>
        /// Sets the object's alpha. Can be immediate or over time
        /// </summary>
        /// <param name="alpha">The new alpha value</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The movement's duration if not immediate</param>
        public void SetAlpha(float alpha, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                InternalSetAlpha(alpha);
            }
            else
            {
                if (lerpAlpha == null)
                    lerpAlpha = new LerpInstanceFloat();

                lerpAlpha.StartLerp(InternalFindAlpha(), alpha, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
		/// Sets the objects position. Can be immediate or over time
		/// </summary>
		/// <param name="position">The new position</param>
		/// <param name="immediate">True if the change must be immediate</param>
		/// <param name="duration">The movement's duration if not immediate</param>
        public void SetPosition(Vector3 position, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                transform.position = position;
            }
            else
            {
                if (lerpPosition == null)
                    lerpPosition = new LerpInstanceVector3();

                lerpPosition.StartLerp(transform.position, position, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
        /// Sets the object's rotation. Can be immediate or over time
        /// </summary>
        /// <param name="position">The new euler angles</param>
        /// <param name="immediate">True if the change must be immediate</param>
        /// <param name="duration">The movement's duration if not immediate</param>
        public void SetRotation(Vector3 eulerAngles, bool immediate = true, float duration = 1.0f)
        {
            if (immediate)
            {
                transform.eulerAngles = eulerAngles;
            }
            else
            {
                if (lerpRotation == null)
                    lerpRotation = new LerpInstanceVector3();

                lerpRotation.StartLerp(transform.eulerAngles, eulerAngles, skipModeEnabled ? 0.1f : duration);
            }
        }

        /// <summary>
        /// Internal function to set the alpha of every material on this object
        /// </summary>
        /// <param name="alpha">The new alpha</param>
        protected void InternalSetAlpha(float alpha)
        {
            foreach (Renderer renderer in renderers)
                foreach (Material material in renderer.materials)
                    material.SetFloat("_Alpha", alpha);
        }

        /// <summary>
		/// Returns the internal Alpha value
		/// </summary>
		/// <returns>The internal alpha value</returns>
        protected float InternalFindAlpha()
        {
            if (renderers.Length > 0 && renderers[0].material && renderers[0].material.HasFloat("_Alpha"))
                return renderers[0].material.GetFloat("_Alpha");
            return 1.0f;
        }

        /// <summary>
		/// Updates the object
		/// </summary>
		/// <param name="manager">The ANF Manager</param>
        public void UpdateObject(ANFManager manager)
        {
            if (lerpPosition != null && lerpPosition.lerping)
            {
                transform.position = lerpPosition.Update();
            }

            if (lerpRotation != null && lerpRotation.lerping)
            {
                transform.eulerAngles = lerpRotation.Update();
            }

            if (lerpAlpha != null && lerpAlpha.lerping)
            {
                InternalSetAlpha(lerpAlpha.Update());
            }

            OnUpdate(manager);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("currentPosition"))
                transform.position = json.GetJArray("currentPosition").AsVector3();

            if (json.ContainsKey("currentRotation"))
                transform.eulerAngles = json.GetJArray("currentRotation").AsVector3();

            if (json.ContainsKey("currentAlpha"))
                InternalSetAlpha(json.GetFloat("currentAlpha"));

            if (json.ContainsKey("positionLerp"))
            {
                if (lerpPosition == null)
                    lerpPosition = new LerpInstanceVector3();

                lerpPosition.Load(json.GetJSON("positionLerp"));
            }

            if (json.ContainsKey("rotationLerp"))
            {
                if (lerpRotation == null)
                    lerpRotation = new LerpInstanceVector3();

                lerpRotation.Load(json.GetJSON("rotationLerp"));
            }

            if (json.ContainsKey("alphaLerp"))
            {
                if (lerpAlpha == null)
                    lerpAlpha = new LerpInstanceFloat();

                lerpAlpha.Load(json.GetJSON("alphaLerp"));
            }

            OnLoad(json);
        }

        public void Save(JSON json)
        {
            json.Add("currentPosition", transform.position);
            json.Add("currentRotation", transform.eulerAngles);
            json.Add("currentAlpha", InternalFindAlpha());

            if (lerpPosition != null)
            {
                JSON lerpJSON = new JSON();
                lerpPosition.Save(lerpJSON);
                json.Add("positionLerp", lerpJSON);
            }

            if (lerpRotation != null)
            {
                JSON lerpJSON = new JSON();
                lerpRotation.Save(lerpJSON);
                json.Add("rotationLerp", lerpJSON);
            }

            if (lerpAlpha != null)
            {
                JSON lerpJSON = new JSON();
                lerpAlpha.Save(lerpJSON);
                json.Add("alphaLerp", lerpJSON);
            }

            OnSave(json);
        }

        protected abstract void OnSave(JSON json);
        protected abstract void OnLoad(JSON json);
        protected abstract void OnCreate(ANFManager manager);
        protected abstract void OnRemove(ANFManager manager);
        protected abstract void OnUpdate(ANFManager manager);

    }
}

