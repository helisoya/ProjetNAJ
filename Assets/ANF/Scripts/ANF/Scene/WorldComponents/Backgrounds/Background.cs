using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Represents an individual background
    /// </summary>
    public class Background : MonoBehaviour
    {
        [Header("Infos")]
        [SerializeField] private BackgroundData defaultData;

        [Header("Components")]
        [SerializeField] private Light sunLight;
        [Tooltip("Only one weather effect can be active at all time")]
        [SerializeField] private SerializedDictionary<string, GameObject> weatherEffects;
        [Tooltip("A marker can be used to position objects and characters at runtime")]
        [SerializeField] private SerializedDictionary<string, Transform> markers;
        [Tooltip("This list should contain all interactable objects relating to the background. (Doors, ...)")]
        [SerializeField] private InteractableObject[] interactableObjects;

        void Awake()
        {
            if (defaultData != null & sunLight)
            {
                defaultData.currentLightDirection = sunLight.transform.forward;
            }
        }

        /// <summary>
		/// Gets the background's default data
		/// </summary>
		/// <returns>The default data</returns>
        public BackgroundData GetDefaultData()
        {
            return defaultData;
        }

        /// <summary>
        /// Sets the sunlight's color
        /// </summary>
        /// <param name="sunColor">The new color</param>
        public void SetSunColor(Color sunColor)
        {
            sunLight.color = sunColor;
        }

        /// <summary>
		/// Changes the current weather effect.
        /// Can be null for no effect
		/// </summary>
		/// <param name="effect">The new effect</param>
        public void SetWeatherEffect(string effect)
        {
            foreach (string key in weatherEffects.Keys)
            {
                weatherEffects[key].SetActive(key == effect);
            }
        }

        /// <summary>
		/// Changes the light's direction (its transform's forward will be changed)
		/// </summary>
		/// <param name="direction">The new light direction</param>
        public void SetLightDirection(Vector3 direction)
        {
            sunLight.transform.forward = direction;
        }

        /// <summary>
        /// Checks if the marker exists
        /// </summary>
        /// <param name="marker">The marker's name</param>
        /// <returns>True if the marker exists</returns>
        public bool MarkerExists(string marker)
        {
            return markers.ContainsKey(marker);
        }

        /// <summary>
        /// Finds a marker's position
        /// </summary>
        /// <param name="marker">The marker's name</param>
        /// <returns>The marker's position</returns>
        public Vector3 GetMarkerPosition(string marker)
        {
            if (markers.ContainsKey(marker))
            {
                return markers[marker].position;
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Finds a marker's rotation
        /// </summary>
        /// <param name="marker">The marker's name</param>
        /// <returns>The marker's rotation</returns>
        public Vector3 GetMarkerRotation(string marker)
        {
            if (markers.ContainsKey(marker))
            {
                return markers[marker].eulerAngles;
            }
            return Vector3.zero;
        }

        public void OnCreate(ANFManager manager)
        {
            if (manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode))
            {
                foreach (InteractableObject interactableObject in interactableObjects)
                {
                    interactionMode.Register(interactableObject);
                }
            }
        }

        public void OnRemove(ANFManager manager)
        {
            manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode);

            foreach (InteractableObject interactableObject in interactableObjects)
            {
                interactableObject.StopAllTween();
                if (interactionMode != null)
                    interactionMode.UnRegister(interactableObject);
            }
        }
    }
}