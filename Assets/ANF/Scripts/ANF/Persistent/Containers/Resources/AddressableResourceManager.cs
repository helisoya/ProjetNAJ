using System;
using System.Collections.Generic;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents a Resource Manager that uses Addressables to load resources
    /// </summary>
    [System.Serializable]
    public class AddressableResourceManager : ResourceManager
    {

        public override DataContainer CloneContainer()
        {
            return new AddressableResourceManager();
        }

        protected override void OnInitialize(ANFSettings settings)
        {
        }

        protected override T TryConvertResource<T>(string path)
        {
            if (loadedResources.TryGetValue(path, out ResourceData data))
            {
                AsyncOperationHandle<T> op = (AsyncOperationHandle<T>)data.resource;
                if (op.IsValid())
                {
#if UNITY_EDITOR
                    Debug.Log($"Getting Resource : {path}");
#endif
                    return op.Result;
                }
            }

            return default;
        }

        protected override T TryLoadResource<T>(string path)
        {
            try
            {
                AsyncOperationHandle<T> op = Addressables.LoadAssetAsync<T>(path);
                T obj = op.WaitForCompletion();

                if (op.Status == AsyncOperationStatus.Failed || obj == null)
                    return obj;

#if UNITY_EDITOR
                Debug.Log($"Loaded Resource : {path}");
#endif
                loadedResources.Add(path, new ResourceData(op, 1));

                return obj;
            }
            catch
            {
                return default;
            }
        }

        protected override void TryReleaseResource<T>(string path)
        {
            if (loadedResources.TryGetValue(path, out ResourceData data))
            {
                AsyncOperationHandle<T> op = (AsyncOperationHandle<T>)data.resource;
                if (op.IsValid())
                {
#if UNITY_EDITOR
                    Debug.Log($"Releasing Resource : {path}");
#endif
                    Addressables.Release(op);
                }
            }
        }


        public override void LoadBundle<T>(string bundleName)
        {
            // Load TAG

            if (loadedBundles.TryGetValue(bundleName, out ResourceData data))
            {
                data.refCount++;
            }
            else
            {
                AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(bundleName);
                IList<IResourceLocation> bundle = locations.WaitForCompletion();

                loadedBundles.Add(bundleName, new ResourceData(locations, 1));

                foreach (IResourceLocation location in bundle)
                {
                    Debug.Log($"Loading bundle item : {location.PrimaryKey}");
                    GetResource<T>(location.PrimaryKey);
                }
            }
        }

        public override void UnloadBundle<T>(string bundleName)
        {
            // Unload TAG

            if (loadedBundles.TryGetValue(bundleName, out ResourceData data))
            {
                data.refCount--;

                IList<IResourceLocation> bundle =
                    ((AsyncOperationHandle<IList<IResourceLocation>>)loadedBundles[bundleName].resource).Result;

                foreach (IResourceLocation location in bundle)
                {
                    Debug.Log($"Unloading bundle item : {location.PrimaryKey}");
                    ReleaseResource<T>(location.PrimaryKey);
                }

                bundle = null;

                if (data.refCount <= 0)
                {
                    Addressables.Release((AsyncOperationHandle<IList<IResourceLocation>>)loadedBundles[bundleName].resource);
                    loadedBundles.Remove(bundleName);
                }
            }
        }

        public override void Reset()
        {
            // ???
        }

        public override void Save(JSON json)
        {
        }

        public override void Load(JSON json)
        {
        }

        public override T GetSpritesheetResource<T>(string spritesheetPath, string resourceName)
        {
            return GetResource<T>($"{spritesheetPath}[{resourceName}]");
        }

        public override void ReleaseSpritesheetResource<T>(string spritesheetPath, string resourceName)
        {
            ReleaseResource<T>($"{spritesheetPath}[{resourceName}]");
        }
    }
}
