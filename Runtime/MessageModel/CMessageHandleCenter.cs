using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SingletonModel;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace MessageModel
{
    public class CMessageHandleCenter : SingletonMonoBehaviour<CMessageHandleCenter>
    {
        // 存储消息类型和对应的委托列表的并发字典
        private ConcurrentDictionary<Type, List<PrioritizedDelegate>> messageHandleDic;

        // 用于管理委托列表的对象池，减少内存分配和垃圾回收
        private ObjectPool<List<PrioritizedDelegate>> listPool;

        /// <summary>
        /// 异常处理回调，当消息处理过程中出现异常时调用，传入消息类型、异常信息和调用栈
        /// </summary>
        public Action<Type, Exception, StackTrace> ExceptionHandler { get; set; }

        // 用于线程同步的锁对象
        private readonly object listLock = new object();

        // 用于取消定期清理无效弱引用任务的令牌源
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        
        public override bool onlySingleScene => false;

        /// <summary>
        /// 单例初始化方法，在对象创建时调用
        /// </summary>
        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            // 初始化消息处理字典
            messageHandleDic = new ConcurrentDictionary<Type, List<PrioritizedDelegate>>();
            // 初始化委托列表对象池
            listPool = new ObjectPool<List<PrioritizedDelegate>>(
                () => new List<PrioritizedDelegate>(),
                null,
                list => list.Clear(),
                null,
                false,
                10,
                100);
            // 订阅场景卸载事件
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            // 启动定期清理无效弱引用的任务
            Task.Run(() => CleanupInvalidWeakReferences(cancellationTokenSource.Token));
        }

        /// <summary>
        /// 对象销毁时调用的方法，用于取消定期清理任务和取消订阅场景卸载事件
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 取消场景卸载事件的订阅
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            // 取消定期清理任务
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 当场景卸载时调用的方法，用于清理该场景下的非全局委托
        /// </summary>
        /// <param name="scene">卸载的场景对象</param>
        private void OnSceneUnloaded(Scene scene)
        {
            string sceneName = scene.name;
            var keysToRemove = new List<Type>();

            // 遍历所有消息类型
            foreach (var key in messageHandleDic.Keys)
            {
                var list = messageHandleDic[key];
                lock (listLock)
                {
                    // 移除该场景下的非全局委托
                    list.RemoveAll(pd => pd.sceneName == sceneName && !pd.isGlobal);
                    if (list.Count == 0)
                    {
                        keysToRemove.Add(key);
                    }
                }
            }

            // 移除空的消息类型对应的列表
            foreach (var key in keysToRemove)
            {
                messageHandleDic.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 订阅指定类型的消息处理程序，支持过滤条件
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="action">消息处理的委托</param>
        /// <param name="isGlobal">是否为全局委托，默认为 false</param>
        /// <param name="priority">委托的优先级，默认为 0</param>
        /// <param name="filter">消息过滤函数，默认为 null</param>
        public void Subscribe<T>(MulticastDelegate action, bool isGlobal = false, int priority = 0, Func<object, bool> filter = null)
        {
            if (action == null)
            {
                Debug.LogError("Cannot subscribe a null action.");
                return;
            }

            string currentSceneName = isGlobal ? null : SceneManager.GetActiveScene().name;

            messageHandleDic.AddOrUpdate(
                typeof(T),
                // 如果消息类型不存在，创建一个新的委托列表
                _ => CreateNewHandlerList(priority, action, currentSceneName, isGlobal, filter, typeof(Action<T>)),
                // 如果消息类型已存在，添加新的委托到列表中
                (_, list) =>
                {
                    var newDelegate = new PrioritizedDelegate
                    {
                        priority = priority,
                        handlerRef = new WeakReference<MulticastDelegate>(action),
                        sceneName = currentSceneName,
                        isGlobal = isGlobal,
                        filter = filter,
                        delegateType = typeof(Action<T>)
                    };
                    lock (listLock)
                    {
                        list.Add(newDelegate);
                    }
                    return list;
                });
        }

        /// <summary>
        /// 创建一个新的委托列表，并添加初始委托
        /// </summary>
        /// <param name="priority">委托的优先级</param>
        /// <param name="action">消息处理的委托</param>
        /// <param name="sceneName">委托所属的场景名称</param>
        /// <param name="isGlobal">是否为全局委托</param>
        /// <param name="filter">消息过滤函数</param>
        /// <param name="delegateType">委托的实际类型</param>
        /// <returns>新的委托列表</returns>
        private List<PrioritizedDelegate> CreateNewHandlerList(int priority, MulticastDelegate action, string sceneName, bool isGlobal, Func<object, bool> filter, Type delegateType)
        {
            var list = listPool.Get();
            list.Add(new PrioritizedDelegate
            {
                priority = priority,
                handlerRef = new WeakReference<MulticastDelegate>(action),
                sceneName = sceneName,
                isGlobal = isGlobal,
                filter = filter,
                delegateType = delegateType
            });
            return list;
        }

        /// <summary>
        /// 取消订阅指定类型的消息处理程序
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="action">要取消订阅的委托</param>
        public void UnSubscribe<T>(MulticastDelegate action)
        {
            if (action == null)
            {
                Debug.LogError("Cannot unsubscribe a null action.");
                return;
            }

            if (messageHandleDic.TryGetValue(typeof(T), out var list))
            {
                lock (listLock)
                {
                    var toRemove = list.Find(pd =>
                    {
                        if (pd.handlerRef.TryGetTarget(out var target))
                        {
                            return target == action;
                        }
                        return false;
                    });
                    if (toRemove != null)
                    {
                        list.Remove(toRemove);
                        if (list.Count == 0)
                        {
                            messageHandleDic.TryRemove(typeof(T), out _);
                            listPool.Release(list);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发布指定类型的消息，触发所有订阅该消息类型的委托
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="t">消息的内容</param>
        public void Publish<T>(T t)
        {
            if (messageHandleDic.TryGetValue(typeof(T), out var list))
            {
                var handlers = new List<PrioritizedDelegate>();
                lock (listLock)
                {
                    handlers.AddRange(list);
                }
                handlers.Sort((a, b) => b.priority.CompareTo(a.priority));

                // 遍历所有委托并执行
                foreach (var pd in handlers)
                {
                    if (pd.handlerRef.TryGetTarget(out var handler) && (pd.filter == null || pd.filter(t)))
                    {
                        try
                        {
                            if (pd.delegateType == typeof(Action<T>))
                            {
                                ((Action<T>)handler)(t);
                            }
                            else if (pd.delegateType == typeof(Func<T, Task>))
                            {
                                ((Func<T, Task>)handler)(t).Wait();
                            }
                            else
                            {
                                handler.DynamicInvoke(t);
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleException(typeof(T), ex, handler, new StackTrace(true));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 批量发布指定类型的消息，依次触发每个消息对应的委托
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="messages">消息列表</param>
        public void PublishBatch<T>(IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                Publish(message);
            }
        }

        /// <summary>
        /// 异步发布指定类型的消息
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="t">消息的内容</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task PublishAsync<T>(T t)
        {
            await Task.Run(() => Publish(t));
        }

        /// <summary>
        /// 处理消息处理过程中出现的异常，记录详细的错误信息
        /// </summary>
        /// <param name="messageType">消息的类型</param>
        /// <param name="ex">抛出的异常</param>
        /// <param name="handler">出现异常的委托</param>
        /// <param name="stackTrace">异常发生时的调用栈</param>
        private void HandleException(Type messageType, Exception ex, MulticastDelegate handler = null, StackTrace stackTrace = null)
        {
            string handlerInfo = handler != null ? $"Handler: {handler.Method.Name}" : "Handler: null";
            string errorMessage = $"Error invoking message handler for {messageType.Name}. {handlerInfo}: {ex.Message}";

            if (ExceptionHandler != null)
            {
                ExceptionHandler(messageType, new Exception(errorMessage, ex), stackTrace);
            }
            else
            {
                Debug.LogError($"{errorMessage}\n{stackTrace}");
            }
        }

        /// <summary>
        /// 清理所有的消息处理程序和委托列表
        /// </summary>
        public void Clear()
        {
            foreach (var list in messageHandleDic.Values)
            {
                lock (listLock)
                {
                    list.Clear();
                }
                listPool.Release(list);
            }
            messageHandleDic.Clear();
        }

        /// <summary>
        /// 定期清理无效的弱引用，释放内存
        /// </summary>
        /// <param name="cancellationToken">用于取消任务的令牌</param>
        private async Task CleanupInvalidWeakReferences(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

                foreach (var key in messageHandleDic.Keys)
                {
                    var list = messageHandleDic[key];
                    lock (listLock)
                    {
                        // 移除无效的弱引用
                        list.RemoveAll(pd =>!pd.handlerRef.TryGetTarget(out _));
                        if (list.Count == 0)
                        {
                            messageHandleDic.TryRemove(key, out _);
                            listPool.Release(list);
                        }
                    }
                }
            }
        }
    }
}