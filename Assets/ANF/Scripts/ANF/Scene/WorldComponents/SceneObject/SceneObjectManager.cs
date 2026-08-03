using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Scene
{
    public abstract class SceneObjectManager<Type> : WorldComponent where Type : SceneObject
    {
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
            if (objects.ContainsKey(name) || obj == null)
            {
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
            if (objects.ContainsKey(name))
            {
                obj = null;
                return false;
            }
            else
            {
                Type resource = Resources.Load<Type>(prefabsPath + name);

                if (resource == null)
                {
                    obj = null;
                    return false;
                }

                obj = Object.Instantiate(resource, manager.transform);
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
                    Object.Destroy(objects[name].obj.gameObject);
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

        public override void OnLoad(JSON json)
        {
            loadDataCache = new Dictionary<string, JSON>();
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

                        if (objects.TryGetValue(key, out ObjectInstance<Type> obj))
                        {
                            if (obj.loadedFromResources)
                                obj.obj.Load(objJSON);
                        }
                        else
                        {
                            loadDataCache.Add(key, objJSON);
                        }
                    }
                }
            }
        }

        public override bool OnChangeScene()
        {
            foreach (ObjectInstance<Type> obj in objects.Values)
            {
                obj.obj.Remove(manager);
            }
            return true;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return false;
        }
    }
}
