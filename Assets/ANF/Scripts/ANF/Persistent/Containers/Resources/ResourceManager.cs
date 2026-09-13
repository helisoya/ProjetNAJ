using System.Collections.Generic;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Persistent
{
    public abstract class ResourceManager : DataContainer
    {
        protected Dictionary<string, ResourceData> loadedResources = new Dictionary<string, ResourceData>();
        protected Dictionary<string, ResourceData> loadedBundles = new Dictionary<string, ResourceData>();

        public abstract DataContainer CloneContainer();
        public abstract void Load(JSON json);
        public abstract void Save(JSON json);
        public abstract void Reset();

        public void Initialize(ANFSettings settings)
        {
            OnInitialize(settings);
        }

        /// <summary>
		/// Loads a spritesheet resource in memory
		/// </summary>
		/// <typeparam name="T">The resource's type</typeparam>
		/// <param name="spritesheetPath">The path to the sprite sheet</param>
		/// <param name="resourceName">The resource's name</param>
        public abstract T GetSpritesheetResource<T>(string spritesheetPath, string resourceName) where T : Object;

        /// <summary>
		/// Releases a spritesheet resource
		/// </summary>
		/// <typeparam name="T">The resource's type</typeparam>
		/// <param name="spritesheetPath">The path to the sprite sheet</param>
		/// <param name="resourceName">The resource's name</param>
        public abstract void ReleaseSpritesheetResource<T>(string spritesheetPath, string resourceName) where T : Object;

        /// <summary>
		/// Loads a resource in memory
		/// </summary>
		/// <typeparam name="T">The resource's type</typeparam>
		/// <param name="path">The resource's path</param>
        public T GetResource<T>(string path)
        {
            if (loadedResources.ContainsKey(path))
            {
                // Has already loaded a resource with that name
                T value = TryConvertResource<T>(path);
                if (value != null) // Could parse the value
                    loadedResources[path].refCount++;
                return value;
            }

            // Has not loaded / found the resource yet
            T foundResource = TryLoadResource<T>(path);
            return foundResource;
        }

        /// <summary>
        /// Unloads a resource from memory
        /// </summary>
        /// <typeparam name="T">The resource's type</typeparam>
        /// <param name="path">The resource's path</param>
        public void ReleaseResource<T>(string path)
        {
            if (loadedResources.ContainsKey(path))
            {
                loadedResources[path].refCount--;
                if (loadedResources[path].refCount <= 0)
                {
                    TryReleaseResource<T>(path);
                    loadedResources.Remove(path);
                }
            }
        }

        /// <summary>
		/// Loads a new bundle in memory
		/// </summary>
		/// <param name="bundleName">The bundle's name</param>
        public abstract void LoadBundle<T>(string bundleName);

        /// <summary>
		/// Releases a bundle from memory
		/// </summary>
		/// <param name="bundleName">The bundle's name</param>
        public abstract void UnloadBundle<T>(string bundleName);


        /// <summary>
		/// Called on init
		/// </summary>
		/// <param name="settings">The ANF Settings</param>
        protected abstract void OnInitialize(ANFSettings settings);

        /// <summary>
        /// Tries to convert a existing resources to the correct type
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="path">The resource's path</param>
        /// <returns>The casted resource, or null if impossible</returns>
        protected abstract T TryConvertResource<T>(string path);

        /// <summary>
        /// Tries to load a new resources to memory
        /// </summary>
        /// <typeparam name="T">The resource's type</typeparam>
        /// <param name="path">The resource's path</param>
        /// <returns>The resource if found</returns>
        protected abstract T TryLoadResource<T>(string path);

        /// <summary>
        /// Tries to release a resource from memory
        /// </summary>
        /// <typeparam name="T">The resource's type</typeparam>
        /// <param name="path">The resource's path</param>
        protected abstract void TryReleaseResource<T>(string path);


        /// <summary>
        /// Represents a loaded resource. Depending on the manager type, the object may not be the actual resource
        /// </summary>
        protected class ResourceData
        {
            public int refCount;
            public object resource;

            public ResourceData(object resource, int refCount)
            {
                this.refCount = refCount;
                this.resource = resource;
            }
        }

    }

}
