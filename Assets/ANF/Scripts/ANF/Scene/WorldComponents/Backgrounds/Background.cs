using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ANF.Scene
{
    /// <summary>
    /// Represents an individual background
    /// </summary>
    public class Background : MonoBehaviour
    {
        [Header("Infos")]
        [SerializeField] private BackgroundDefaultData defaultData;
        private Terrain[] terrains;

        [Header("Components")]
        [SerializeField] private Light sunLight;
        [Tooltip("Only one weather effect can be active at all time")]
        [SerializeField] private SerializedDictionary<string, GameObject> weatherEffects;
        [Tooltip("A marker can be used to position objects and characters at runtime")]
        [SerializeField] private SerializedDictionary<string, Transform> markers;
        [Tooltip("This list should contain all interactable objects relating to the background. (Doors, ...)")]
        [SerializeField] private InteractableObject[] interactableObjects;
        [SerializeField] private SerializedDictionary<string, Character> backgroundCharacters;
        [SerializeField] private SerializedDictionary<string, StaticObject> backgroundStaticObjects;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {

            UnityEditor.SceneView sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView && sceneView.camera)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawFrustum(new Vector3(0, 1, -10), sceneView.camera.fieldOfView,
                50.0f, 0.01f,
                sceneView.camera.aspect);
            }

        }
#endif

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
        public BackgroundDefaultData GetDefaultData()
        {
            return defaultData;
        }

        /// <summary>
        /// Sets the sunlight's color
        /// </summary>
        /// <param name="sunColor">The new color</param>
        public void SetSunColor(Color sunColor)
        {
            if (sunLight)
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
		/// Enables all existing terrain's foliage
		/// </summary>
		/// <param name="enabled">True if foliage should be visible</param>
        public void EnableFoliage(bool enabled)
        {
            if (terrains != null)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.detailObjectDensity = enabled ? 1.0f : 0.0f;
                }
            }
        }

        /// <summary>
		/// Changes all existing terrain's quality
		/// </summary>
		/// <param name="terrainQuality">The terrain quality (converted to pixel error)</param>
        public void SetTerrainQuality(TerrainQuality terrainQuality)
        {
            int[] pixelValues = new int[] { 5, 4, 3, 0 };
            int selectedValue = pixelValues[(int)terrainQuality];

            if (terrains != null)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.heightmapMaximumLOD = selectedValue;
                }
            }
        }

        /// <summary>
		/// Changes the light's direction (its transform's forward will be changed)
		/// </summary>
		/// <param name="direction">The new light direction</param>
        public void SetLightDirection(Vector3 direction)
        {
            if (sunLight)
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

            if (manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                foreach (string name in backgroundStaticObjects.Keys)
                {
                    staticObjectManager.AddSceneObject(name, backgroundStaticObjects[name]);
                }
            }

            if (manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                foreach (string name in backgroundCharacters.Keys)
                {
                    characterManager.AddSceneObject(name, backgroundCharacters[name]);
                }
            }

            // Disables active cameras (Cameras inside Background scenes are considered debug cams)
            Camera[] cameras = transform.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
                cam.gameObject.SetActive(false);

            terrains = transform.GetComponentsInChildren<Terrain>();
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

            if (manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                foreach (string name in backgroundStaticObjects.Keys)
                {
                    staticObjectManager.RemoveSceneObject(name, false);
                }
            }

            if (manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                foreach (string name in backgroundCharacters.Keys)
                {
                    characterManager.RemoveSceneObject(name, false);
                }
            }
        }
    }
}