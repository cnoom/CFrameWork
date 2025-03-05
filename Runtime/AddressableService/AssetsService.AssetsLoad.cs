// 文件：AssetsSystem.cs

using System;
using System.Collections;
using System.Collections.Generic;
using LogModule;
using SingletonModule;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressableService
{
    public partial class AssetsService
    {

        // 同步加载（慎用，可能造成卡顿）
        public T LoadAsset<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                // 修改后
                this.LogError("资源键无效");
                return default;
            }
            try
            {
                AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(key, typeof(T));
                locationHandle.WaitForCompletion();

                if(locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count <= 0)
                {
                    Debug.LogError($"LoadAsset failed: Could not find resource location for key {key}");
                    return default;
                }
                IResourceLocation location = locationHandle.Result[0];
                if (assetHandles.TryGetValue(location.PrimaryKey, out AsyncOperationHandle assetHandle))
                {
                    referenceCount[location.PrimaryKey]++;
                    return (T)assetHandle.Result;
                }
                #if UNITY_EDITOR
                this.LogWarning("[性能警告] 主线程同步加载可能造成卡顿，建议使用异步加载方法");
                #endif
                AsyncOperationHandle<T> operation = Addressables.LoadAssetAsync<T>(location);
                TrackHandle(location.PrimaryKey, operation);
                T result = operation.WaitForCompletion();
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadAsset failed: {e.Message}");
                return default;
            }
        }

        // 异步加载完整实现
        public void LoadAssetAsync<T>(string key, Action<T> onLoaded)
        {
            if (string.IsNullOrEmpty(key))
            {
                this.LogError("Empty asset key");
                return;
            }

            StartCoroutine(LoadAssetAsyncRoutine(key, onLoaded));
        }

        public IEnumerator LoadAssetAsyncRoutine<T>(string key, Action<T> onLoaded)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(key, typeof(T));
            yield return locationHandle;

            if(locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count <= 0)
            {
                this.LogError($"Failed to find resource location for key {key}");
                yield break;
            }
            IResourceLocation location = locationHandle.Result[0];
            string primaryKey = location.PrimaryKey;
            if(assetHandles.TryGetValue(primaryKey, out AsyncOperationHandle assetHandle))
            {
                referenceCount[primaryKey]++;
                onLoaded?.Invoke((T)assetHandle.Result);
                yield break;
            }
            while (loadingHandles.ContainsKey(primaryKey))
            {
                yield return loadingHandles[primaryKey];
                if(!assetHandles.TryGetValue(primaryKey, out AsyncOperationHandle existingHandle)) continue;
                referenceCount[primaryKey]++;
                onLoaded?.Invoke((T)existingHandle.Result);
                yield break;
            }
            
            var operation = Addressables.LoadAssetAsync<T>(location);
            loadingHandles[primaryKey] = operation;
            TrackHandle(primaryKey, operation);
            yield return operation;
            loadingHandles.Remove(primaryKey);
            if(operation.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(operation.Result);
            }
            else
            {
                this.LogError($"资源加载失败: {key} [{operation.OperationException}]");
                Addressables.Release(operation); // 新增释放操作
            }
        }

        /// <summary>
        ///     通过AssetReference加载资源
        /// </summary>
        public void LoadByReference<T>(AssetReference reference, Action<T> onLoaded, Action<string> onError = null)
        {
            if (!reference.RuntimeKeyIsValid())
            {
                onError?.Invoke("Invalid AssetReference");
                return;
            }

            StartCoroutine(LoadAssetByReferenceCoroutine(reference, onLoaded, onError));
        }

        public IEnumerator LoadAssetByReferenceCoroutine<T>(AssetReference reference, Action<T> onLoaded, Action<string> onError = null)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(reference, typeof(T));
            yield return locationHandle;

            if(locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count <= 0)
            {
                onError?.Invoke($"Failed to find resource location for reference {reference.AssetGUID}");
                yield break;
            }
            IResourceLocation location = locationHandle.Result[0];
            if(assetHandles.TryGetValue(location.PrimaryKey, out AsyncOperationHandle assetHandle))
            {
                referenceCount[location.PrimaryKey]++;
                onLoaded?.Invoke((T)assetHandle.Result);
                yield break;
            }

            AsyncOperationHandle<T> operation = Addressables.LoadAssetAsync<T>(location);
            TrackHandle(location.PrimaryKey, operation);
            yield return operation;

            if(operation.Status != AsyncOperationStatus.Succeeded)
            {
                onError?.Invoke($"Failed to load asset by reference: {reference.AssetGUID}");
                yield break;
            }
            onLoaded?.Invoke(operation.Result);
        }

        /// <summary>
        ///     通过标签加载多个资源
        /// </summary>
        public void LoadAssetsByLabel<T>(string label, Action<T> onLoaded, Action<string> onError = null)
        {
            StartCoroutine(LoadAssetsByLabelCoroutine(label, onLoaded, onError));
        }

        public IEnumerator LoadAssetsByLabelCoroutine<T>(string label, Action<T> onLoaded, Action<string> onError = null)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            yield return locationHandle;

            if(locationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                onError?.Invoke($"Failed to load assets by label: {label}");
                yield break;
            }

            foreach (var location in locationHandle.Result)
            {
                if(assetHandles.TryGetValue(location.PrimaryKey, out AsyncOperationHandle assetHandle))
                {
                    referenceCount[location.PrimaryKey]++;
                    onLoaded?.Invoke((T)assetHandle.Result);
                    continue;
                }
                AsyncOperationHandle<T> operation = Addressables.LoadAssetAsync<T>(location);
                TrackHandle(location.PrimaryKey, operation);

                yield return operation;
                if(operation.Status != AsyncOperationStatus.Succeeded)
                {
                    onError?.Invoke($"Failed to load assets by label: {label}");
                    continue;
                }
                onLoaded?.Invoke(operation.Result);
            }
        }
    }
}