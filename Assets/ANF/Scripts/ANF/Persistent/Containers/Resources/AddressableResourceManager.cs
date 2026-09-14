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
                loadedResources.Add(path, new ResourceData(op, 1, false));

                return obj;
            }
            catch
            {
                return default;
            }
        }

        protected override void TryLoadResourceAsync<T>(string path, System.Action<object> resultCallback)
        {
            try
            {
                AsyncOperationHandle<T> op = Addressables.LoadAssetAsync<T>(path);
                op.Completed += (resOp) =>
                {
                    loadedResources[path].isLoading = false;

                    foreach (System.Action<object> action in loadedResources[path].actions)
                    {
                        action.Invoke(resOp.Result);
                    }
                };

#if UNITY_EDITOR
                Debug.Log($"Loading Resource Async : {path}");
#endif
                loadedResources.Add(path, new ResourceData(op, 1, true));
                loadedResources[path].actions.Add(resultCallback);
            }
            catch
            {
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


        public override void LoadBundle(string bundleName, BundleType bundleType)
        {
            // Load TAG

            if (loadedBundles.TryGetValue(bundleName, out ResourceData data))
            {
                if (data.bundleType == bundleType)
                    data.refCount++;
            }
            else
            {
                AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(bundleName);
                IList<IResourceLocation> bundle = locations.WaitForCompletion();

                ResourceData newData = new ResourceData(locations, 1, false);
                newData.bundleType = bundleType;

                loadedBundles.Add(bundleName, newData);

                foreach (IResourceLocation location in bundle)
                {
                    Debug.Log($"Loading bundle item : {location.PrimaryKey}");

                    switch (bundleType)
                    {
                        case BundleType.AudioClip:
                            GetResource<AudioClip>(location.PrimaryKey);
                            break;
                        case BundleType.TextAsset:
                            GetResource<TextAsset>(location.PrimaryKey);
                            break;
                        case BundleType.GameObject:
                            GetResource<GameObject>(location.PrimaryKey);
                            break;
                        case BundleType.Sprite:
                            GetResource<Sprite>(location.PrimaryKey);
                            break;
                        case BundleType.Texture2D:
                            GetResource<Texture2D>(location.PrimaryKey);
                            break;
                    }
                }
            }
        }

        public override void UnloadBundle(string bundleName, BundleType bundleType)
        {
            // Unload TAG

            if (loadedBundles.TryGetValue(bundleName, out ResourceData data) && data.bundleType == bundleType)
            {
                data.refCount--;

                IList<IResourceLocation> bundle =
                    ((AsyncOperationHandle<IList<IResourceLocation>>)loadedBundles[bundleName].resource).Result;

                foreach (IResourceLocation location in bundle)
                {
                    Debug.Log($"Unloading bundle item : {location.PrimaryKey}");
                    switch (bundleType)
                    {
                        case BundleType.AudioClip:
                            ReleaseResource<AudioClip>(location.PrimaryKey);
                            break;
                        case BundleType.TextAsset:
                            ReleaseResource<TextAsset>(location.PrimaryKey);
                            break;
                        case BundleType.GameObject:
                            ReleaseResource<GameObject>(location.PrimaryKey);
                            break;
                        case BundleType.Sprite:
                            ReleaseResource<Sprite>(location.PrimaryKey);
                            break;
                        case BundleType.Texture2D:
                            ReleaseResource<Texture2D>(location.PrimaryKey);
                            break;
                    }
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
