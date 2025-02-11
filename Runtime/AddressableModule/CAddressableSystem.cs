using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using SingletonModule;

namespace AddressableModule
{
    public class CAddressableSystem : SingletonMonoBehaviour<CAddressableSystem>
    {
        public override bool onlySingleScene => false;

        // 资源缓存最大容量
        private const int MaxAssetCacheSize = 100;
        // 场景缓存最大容量
        private const int MaxSceneCacheSize = 10;

        // 资源缓存字典，键为资源地址，值为资源引用数据
        private readonly Dictionary<string, AssetReferenceData> _assetCache = new Dictionary<string, AssetReferenceData>();
        // 场景缓存字典，键为场景地址，值为场景引用数据
        private readonly Dictionary<string, SceneReferenceData> _sceneCache = new Dictionary<string, SceneReferenceData>();

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetAddress">资源地址</param>
        /// <param name="progressCallback">加载进度回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载成功返回资源实例，失败返回默认值</returns>
        public async Task<T> LoadAssetAsync<T>(string assetAddress, Action<float> progressCallback = null)
        {
            // 检查资源是否已缓存
            if(TryGetCachedAsset(assetAddress, out var cachedData))
            {
                IncrementReferenceCount(cachedData);
                return (T)cachedData.asset;
            }

            try
            {
                // 开始异步加载资源
                var handle = Addressables.LoadAssetAsync<T>(assetAddress);
                while (!handle.IsDone)
                {
                    progressCallback?.Invoke(handle.PercentComplete);
                    await Task.Yield();
                }
                await handle.Task;

                // 检查加载状态
                if(handle.Status == AsyncOperationStatus.Succeeded)
                {
                    EnsureAssetCacheCapacity();
                    // 创建新的资源引用数据
                    var newData = CreateAssetReferenceData(handle);
                    // 将新数据添加到缓存中
                    _assetCache.Add(assetAddress, newData);
                    return handle.Result;
                }

                // 加载失败，记录错误日志
                LogLoadError("资源加载", assetAddress, handle.OperationException?.Message ?? "未知错误");
                return default;
            }
            catch (Exception e)
            {
                // 加载异常，记录异常日志
                LogLoadException("资源加载", assetAddress, e);
                return default;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="assetAddress">资源地址</param>
        public void ReleaseAsset(string assetAddress)
        {
            // 检查资源是否存在于缓存中
            if(!_assetCache.TryGetValue(assetAddress, out var data)) return;

            // 减少引用计数
            DecrementReferenceCount(data);

            // 引用计数为 0 时释放资源
            if(ShouldRelease(data))
            {
                ReleaseAssetHandle(data.handle);
                _assetCache.Remove(assetAddress);
            }
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        /// <param name="loadMode">加载模式，默认为单场景加载</param>
        /// <param name="progressCallback">加载进度回调</param>
        /// <returns>加载成功返回场景实例，失败返回默认值</returns>
        public async Task<SceneInstance> LoadSceneAsync(string sceneAddress, LoadSceneMode loadMode = LoadSceneMode.Single, Action<float> progressCallback = null)
        {
            // 检查场景是否已缓存
            if(TryGetCachedScene(sceneAddress, out var cachedData))
            {
                IncrementReferenceCount(cachedData);
                return cachedData.sceneInstance;
            }

            try
            {
                // 开始异步加载场景
                var handle = Addressables.LoadSceneAsync(sceneAddress, loadMode);
                while (!handle.IsDone)
                {
                    progressCallback?.Invoke(handle.PercentComplete);
                    await Task.Yield();
                }
                await handle.Task;

                // 检查加载状态
                if(handle.Status == AsyncOperationStatus.Succeeded)
                {
                    EnsureSceneCacheCapacity();
                    // 创建新的场景引用数据
                    var newData = CreateSceneReferenceData(handle);
                    // 将新数据添加到缓存中
                    _sceneCache.Add(sceneAddress, newData);
                    return handle.Result;
                }

                // 加载失败，记录错误日志
                LogLoadError("场景加载", sceneAddress, handle.OperationException?.Message ?? "未知错误");
                return default;
            }
            catch (Exception e)
            {
                // 加载异常，记录异常日志
                LogLoadException("场景加载", sceneAddress, e);
                return default;
            }
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        public async Task UnloadSceneAsync(string sceneAddress)
        {
            // 检查场景是否存在于缓存中
            if(!_sceneCache.TryGetValue(sceneAddress, out var data)) return;

            // 减少引用计数
            DecrementReferenceCount(data);

            // 引用计数为 0 时卸载场景
            if(ShouldRelease(data))
            {
                await UnloadSceneHandleAsync(data.handle);
                _sceneCache.Remove(sceneAddress);
            }
        }

        /// <summary>
        /// 预加载多个资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetAddresses">资源地址列表</param>
        public async Task PreloadAssetsAsync<T>(List<string> assetAddresses)
        {
            var tasks = new List<Task<T>>();
            foreach (var address in assetAddresses)
            {
                tasks.Add(LoadAssetAsync<T>(address));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 预加载多个场景
        /// </summary>
        /// <param name="sceneAddresses">场景地址列表</param>
        /// <param name="loadMode">加载模式</param>
        public async Task PreloadScenesAsync(List<string> sceneAddresses, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            var tasks = new List<Task<SceneInstance>>();
            foreach (var address in sceneAddresses)
            {
                tasks.Add(LoadSceneAsync(address, loadMode));
            }
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Preload scenes failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理资源缓存
        /// </summary>
        public void ClearAssetCache()
        {
            foreach (var pair in _assetCache)
            {
                ReleaseAssetHandle(pair.Value.handle);
            }
            _assetCache.Clear();
        }

        /// <summary>
        /// 清理场景缓存
        /// </summary>
        public async Task ClearSceneCacheAsync()
        {
            foreach (var pair in _sceneCache)
            {
                await UnloadSceneHandleAsync(pair.Value.handle);
            }
            _sceneCache.Clear();
        }

        #region 辅助方法

        private bool TryGetCachedAsset(string assetAddress, out AssetReferenceData data)
        {
            return _assetCache.TryGetValue(assetAddress, out data);
        }

        private bool TryGetCachedScene(string sceneAddress, out SceneReferenceData data)
        {
            return _sceneCache.TryGetValue(sceneAddress, out data);
        }

        private void IncrementReferenceCount(ReferenceDataBase data)
        {
            data.referenceCount++;
        }

        private void DecrementReferenceCount(ReferenceDataBase data)
        {
            data.referenceCount--;
        }

        private bool ShouldRelease(ReferenceDataBase data)
        {
            return data.referenceCount <= 0;
        }

        private AssetReferenceData CreateAssetReferenceData(AsyncOperationHandle handle)
        {
            return new AssetReferenceData
            {
                asset = handle.Result,
                handle = handle,
                referenceCount = 1
            };
        }

        private SceneReferenceData CreateSceneReferenceData(AsyncOperationHandle<SceneInstance> handle)
        {
            return new SceneReferenceData
            {
                sceneInstance = handle.Result,
                handle = handle,
                referenceCount = 1
            };
        }

        private void ReleaseAssetHandle(AsyncOperationHandle handle)
        {
            try
            {
                Addressables.Release(handle);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Release asset failed: {ex.Message}");
                throw;
            }
        }

        private async Task UnloadSceneHandleAsync(AsyncOperationHandle<SceneInstance> handle)
        {
            try
            {
                await Addressables.UnloadSceneAsync(handle).Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unload scene failed: {ex.Message}");
            }
        }

        private void EnsureAssetCacheCapacity()
        {
            if(_assetCache.Count >= MaxAssetCacheSize)
            {
                // 实现 LRU 策略清理缓存
                string lruAssetAddress = null;
                int minReferenceCount = int.MaxValue;
                foreach (var pair in _assetCache)
                {
                    if(pair.Value.referenceCount < minReferenceCount)
                    {
                        minReferenceCount = pair.Value.referenceCount;
                        lruAssetAddress = pair.Key;
                    }
                }

                if(lruAssetAddress != null)
                {
                    ReleaseAsset(lruAssetAddress);
                }
            }
        }

        private void EnsureSceneCacheCapacity()
        {
            if(_sceneCache.Count >= MaxSceneCacheSize)
            {
                // 实现 LRU 策略清理缓存
                string lruSceneAddress = null;
                int minReferenceCount = int.MaxValue;
                foreach (var pair in _sceneCache)
                {
                    if(pair.Value.referenceCount < minReferenceCount)
                    {
                        minReferenceCount = pair.Value.referenceCount;
                        lruSceneAddress = pair.Key;
                    }
                }

                if(lruSceneAddress != null)
                {
                    UnloadSceneAsync(lruSceneAddress).Wait();
                }
            }
        }

        private void LogLoadError(string operationType, string address, string message)
        {
            Debug.LogError($"[{operationType}] 加载失败: {address}, 错误信息: {message}");
        }

        private void LogLoadException(string operationType, string address, Exception e)
        {
            Debug.LogError($"[{operationType}] 加载异常: {address}, 异常信息: {e.Message}");
        }

        #endregion

        // 引用数据基类，用于统一管理引用计数
        private abstract class ReferenceDataBase
        {
            public int referenceCount;
        }

        // 资源引用数据类
        private class AssetReferenceData : ReferenceDataBase
        {
            public object asset;
            public AsyncOperationHandle handle;
        }

        // 场景引用数据类
        private class SceneReferenceData : ReferenceDataBase
        {
            public SceneInstance sceneInstance;
            public AsyncOperationHandle<SceneInstance> handle;
        }
    }
}