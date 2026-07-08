using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Scene
{
    public abstract class SceneObjectManager<Type> : WorldComponent where Type : SceneObject
    {
        [Header("Infos")]
        [SerializeField] protected string prefabsPath;
        private Dictionary<string, Type> objects;
        private bool skipModeEnabled;

        public override void OnInitialize()
        {
            objects = new Dictionary<string, Type>();
        }

        public void OnSkipModeToggle(bool enabled)
        {
            if (skipModeEnabled != enabled)
            {
                skipModeEnabled = enabled;
                foreach (Type obj in objects.Values)
                    obj.OnSkipModeToggle(enabled);
            }
        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {
            foreach (Type obj in objects.Values)
            {
                obj.UpdateObject(manager);
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
                objects.Add(name, obj);
                return true;
            }
        }

        /// <summary>
        /// Removes a scene object from the database
        /// </summary>
        /// <param name="name">The object's name</param>
        /// <returns>True if the operation was a success</returns>
        public bool RemoveSceneObject(string name)
        {
            if (objects.ContainsKey(name))
            {
                objects[name].Remove(manager);
                objects.Remove(name);
                return true;
            }
            return false;
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
                obj = objects[name];
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
            foreach (KeyValuePair<string, Type> pair in objects)
            {
                JSON objectJSON = new JSON();
                pair.Value.Save(objectJSON);

                json.Add(pair.Key, objectJSON);
            }
        }

        public override void OnLoad(JSON json)
        {
            foreach (string key in json.Keys)
            {
                if (AddSceneObject(key, out Type obj))
                    obj.Load(json.GetJSON(key));
            }
        }

        public override void OnChangeScene()
        {
            foreach (Type obj in objects.Values)
            {
                obj.Remove(manager);
            }
        }
    }

}
