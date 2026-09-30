using ANF.Persistent;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Scene
{
    public abstract class SceneObjectManager<Type> : WorldComponent where Type : SceneObject
    {
        private const int LOAD_CACHE_FRAME_LIFE = 2;

        /// <summary>
        /// Object Instance
        /// </summary>
        public struct ObjectInstance<T>
        {
            public bool loadedFromResources;
            public T obj;
        }

        [Header("Infos")]
        [SerializeField] protected string prefabsPath;
        private Dictionary<string, ObjectInstance<Type>> objects;
        private bool skipModeEnabled;
        private Dictionary<string, JSON> loadDataCache;
        private ResourceManager resourceManager;

        private float loadDataCacheFrameSurvival;

        public override void OnInitialize()
        {
            objects = new Dictionary<string, ObjectInstance<Type>>();
        }

        public void OnSkipModeToggle(bool enabled)
        {
            if (skipModeEnabled != enabled)
            {
                skipModeEnabled = enabled;
                foreach (ObjectInstance<Type> obj in objects.Values)
                    obj.obj.OnSkipModeToggle(enabled);
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetPlayerData().GetComponent(out resourceManager);
        }

        public override void OnUpdate()
        {
            foreach (ObjectInstance<Type> obj in objects.Values)
            {
                obj.obj.UpdateObject(manager);
            }
        }

        /// <summary>
        /// Adds a scene object to the database
        /// </summary>
        /// <param name="name">The object's name</param>
        /// <param name="obj">The object</param>
        /// <returns>True if the operation was a success</returns>
        public bool AddSceneObject(string name, Type obj)
        {
            if (objects.ContainsKey(name) || obj == null ||
                (loadDataCache != null && !loadDataCache.ContainsKey(name)))
            {
                // Don't add if key exists, object is null
                // OR if a load chache exists and 
                return false;
            }
            else
            {
                obj.Create(manager);
                obj.OnSkipModeToggle(skipModeEnabled);
                objects.Add(name, new ObjectInstance<Type>() { obj = obj, loadedFromResources = false });

                if (loadDataCache != null && loadDataCache.TryGetValue(name, out JSON loadData))
                {
                    obj.Load(loadData);
                    loadDataCache.Remove(name);
                }

                return true;
            }
        }

        /// <summary>
        /// Adds a scene object to the database
        /// </summary>
        /// <param name="name">The object's name</param>
        /// <param name="obj">The created object</param>
        /// <returns>True if the operation was a success</returns>
        public bool AddSceneObject(string name, out Type obj)
        {
            if (resourceManager == null || objects.ContainsKey(name))
            {
                obj = null;
                return false;
            }
            else
            {
                GameObject resource = resourceManager.GetResource<GameObject>(prefabsPath + name);

                if (resource == null || !resource.TryGetComponent<Type>(out Type castedResource))
                {
                    obj = null;
                    return false;
                }

                if (castedResource == null)
                {
                    obj = null;
                    return false;
                }

                obj = Object.Instantiate(castedResource, manager.transform);
                obj.Create(manager);
                obj.OnSkipModeToggle(skipModeEnabled);
                objects.Add(name, new ObjectInstance<Type>() { obj = obj, loadedFromResources = true });
                return true;
            }
        }

        /// <summary>
        /// Removes a scene object from the database
        /// </summary>
        /// <param name="name">The object's name</param>
        /// <param name="destroyGameObject">True if the object must be deleted. Leave to true in most cases</param>
        /// <returns>True if the operation was a success</returns>
        public bool RemoveSceneObject(string name, bool destroyGameObject = true)
        {
            if (objects.ContainsKey(name))
            {
                objects[name].obj.Remove(manager);
                if (destroyGameObject)
                {
                    Object.Destroy(objects[name].obj.gameObject);
                    if (resourceManager != null)
                    {
                        resourceManager.ReleaseResource<GameObject>(prefabsPath + name);
                    }
                }
                objects.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
		/// Removes all scene objects from the database
        /// <paramref name="removeOnlyResourcesObjects"/>True if only resources loaded objects should be removed</param>
		/// </summary>
        public void RemoveAllSceneObjects(bool removeOnlyResourcesObjects = true)
        {
            foreach (string name in objects.Keys)
            {
                if (!removeOnlyResourcesObjects || objects[name].loadedFromResources)
                {
                    objects[name].obj.Remove(manager);
                    Object.Destroy(objects[name].obj.gameObject);
                }
            }
            objects.Clear();
        }

        /// <summary>
        /// Gets a scene object from the database
        /// </summary>
        /// <param name="obj">The found object</param>
        /// <param name="name">The object's name</param>
        /// <returns>True if the object was found</returns>
        public bool GetSceneObject(string name, out Type obj)
        {
            if (objects.ContainsKey(name))
            {
                obj = objects[name].obj;
                return true;
            }

            obj = null;
            return false;
        }

        public override void OnEnabled()
        {
        }

        public override void OnDisabled()
        {
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnSave(JSON json)
        {
            JSON objJSON = new JSON();
            foreach (KeyValuePair<string, ObjectInstance<Type>> pair in objects)
            {
                JSON objectJSON = new JSON();
                pair.Value.obj.Save(objectJSON);
                objectJSON.Add("loadedFromResources", pair.Value.loadedFromResources);

                objJSON.Add(pair.Key, objectJSON);
            }
            json.Add("objects", objJSON);
        }

        public override bool OnLoad(JSON json)
        {
            loadDataCache = new Dictionary<string, JSON>();
            loadDataCacheFrameSurvival = LOAD_CACHE_FRAME_LIFE;
            List<string> existingNonResources = new List<string>();
            if (json.ContainsKey("objects"))
            {
                JSON allObjects = json.GetJSON("objects");
                foreach (string key in allObjects.Keys)
                {
                    JSON objJSON = allObjects.GetJSON(key);

                    if (objJSON.GetBool("loadedFromResources"))
                    {
                        // Resource Object
                        if (AddSceneObject(key, out Type obj))
                            obj.Load(objJSON);
                    }
                    else
                    {
                        // User generated object
                        // Ex : Background object
                        // The object may already exists, or may be created later
                        // Destroy objects that exists in memory but not in the save file

                        if (objects.TryGetValue(key, out ObjectInstance<Type> obj))
                        {
                            if (obj.loadedFromResources)
                            {
                                obj.obj.Load(objJSON);
                                existingNonResources.Add(key);
                            }
                        }
                        else
                        {
                            loadDataCache.Add(key, objJSON);
                        }
                    }
                }
            }

            // Remove unused objects
            List<string> allKeys = new List<string>(objects.Keys);
            foreach (string objectId in allKeys)
            {
                if (!objects[objectId].loadedFromResources &&
                    !existingNonResources.Contains(objectId))
                    RemoveSceneObject(objectId);
            }

            return false;
        }

        public override bool OnChangeScene()
        {
            foreach (string key in objects.Keys)
            {
                objects[key].obj.Remove(manager);
                if (objects[key].loadedFromResources)
                {
                    Object.Destroy(objects[key].obj.gameObject);
                    if (resourceManager != null)
                    {
                        resourceManager.ReleaseResource<GameObject>(prefabsPath + key);
                    }
                }
            }
            resourceManager = null;
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            // Keeping the load cache intact will prevent manual scene objects from being created
            // It is destroyed after X frames (1 by default)
            if (loadDataCache != null)
            {
                if (manager.GetWorld().GetComponent(out BackgroundManager backgroundManager))
                {
                    if (backgroundManager.IsLoadingOrCleaningUp(false))
                        return true;
                }

                if (updateComponent)
                    loadDataCacheFrameSurvival--;

                if (loadDataCacheFrameSurvival <= 0)
                {
                    if (updateComponent)
                    {
                        if (loadDataCache.Count > 0)
                            Debug.LogWarning($"Object Cache deleted with still {loadDataCache.Count} cached objects");
                        loadDataCache = null;
                    }
                    return false;
                }
                return true;
            }

            return false;
        }
    }
}
