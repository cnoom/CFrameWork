using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LogModule;

namespace MessageModule
{
    // 定义一个包含处理程序和优先级的结构体
    public struct HandlerInfo
    {
        public WeakReference<Delegate> Handler;
        public int Priority;
        public Action<object> Invoker;

        public HandlerInfo(Delegate handler, int priority, Action<object> invoker)
        {
            Handler = new WeakReference<Delegate>(handler);
            Priority = priority;
            Invoker = invoker;
        }
    }

    public class MessageHandler
    {
        private const int CleanupThreshold = 10; // 清理阈值，当列表中无效项达到该数量时进行清理
        private readonly object lockObject = new object();
        // 存储消息类型和对应的处理程序信息列表
        private readonly ConcurrentDictionary<Type, List<HandlerInfo>> messageDic = new ConcurrentDictionary<Type, List<HandlerInfo>>();
        private int cleanupCounter;
        private Action<Exception> exceptionHandler;

        /// <summary>
        /// 订阅方法,允许指定处理程序的优先级
        /// </summary>
        /// <param name="handler">  注册的处理程序 </param>
        /// <param name="priority"> 优先级大的优先 </param>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>(Action<T> handler, int priority = 0)
        {
            if(handler == null)
            {
                this.LogError("Handler cannot be null.");
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null.");
            }

            Action<object> invoker = msg => handler((T)msg);
            HandlerInfo newHandlerInfo = new HandlerInfo(handler, priority, invoker);

            messageDic.AddOrUpdate(
                typeof(T),
                _ => new List<HandlerInfo>
                {
                    newHandlerInfo
                },
                (_, existingHandlers) =>
                {
                    InsertHandlerWithPriority(existingHandlers, newHandlerInfo);
                    return existingHandlers;
                });
        }

        // 取消订阅方法
        public void UnSubscribe<T>(Action<T> handler)
        {
            if(handler == null)
            {
                this.LogError("Handler cannot be null.");
            }

            Delegate delegateHandler = handler;
            if(!TryGetHandlers(typeof(T), out List<HandlerInfo> handlers)) return;

            lock (lockObject)
            {
                handlers.RemoveAll(h => h.Handler.TryGetTarget(out Delegate target) && target == delegateHandler);
                if(handlers.Count == 0)
                {
                    messageDic.TryRemove(typeof(T), out _);
                }
            }
        }

        // 发布消息方法
        public void Publish<T>(T message)
        {
            if(message == null)
            {
                this.LogError("Message cannot be null.");
            }

            Type messageType = typeof(T);
            if(!TryGetHandlers(messageType, out List<HandlerInfo> handlers)) return;

            List<HandlerInfo> validHandlers = new List<HandlerInfo>();
            foreach (HandlerInfo handlerInfo in handlers)
            {
                if(handlerInfo.Handler.TryGetTarget(out Delegate target))
                {
                    validHandlers.Add(handlerInfo);
                    handlerInfo.Invoker(message);
                }
                else
                {
                    cleanupCounter++;
                }
            }

            if(cleanupCounter >= CleanupThreshold)
            {
                CleanupInvalidHandlers(messageType, handlers);
                cleanupCounter = 0;
            }
        }

        // 调整处理程序优先级的方法
        public void AdjustPriority<T>(Action<T> handler, int newPriority)
        {
            if(handler == null)
            {
                this.LogError("Handler cannot be null.");
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null.");
            }

            Delegate delegateHandler = handler;
            if(!TryGetHandlers(typeof(T), out List<HandlerInfo> handlers)) return;

            lock (lockObject)
            {
                for(var i = 0; i < handlers.Count; i++)
                {
                    if(handlers[i].Handler.TryGetTarget(out Delegate target) && target == delegateHandler)
                    {
                        HandlerInfo oldHandlerInfo = handlers[i];
                        HandlerInfo newHandlerInfo = new HandlerInfo(target, newPriority, oldHandlerInfo.Invoker);
                        handlers.RemoveAt(i);
                        InsertHandlerWithPriority(handlers, newHandlerInfo);
                        break;
                    }
                }
            }
        }

        // 设置异常处理回调
        public void SetExceptionHandler(Action<Exception> exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
        }

        // 尝试获取处理程序列表
        private bool TryGetHandlers(Type messageType, out List<HandlerInfo> handlers)
        {
            return messageDic.TryGetValue(messageType, out handlers);
        }

        // 按优先级插入处理程序
        private void InsertHandlerWithPriority(List<HandlerInfo> handlers, HandlerInfo newHandlerInfo)
        {
            var insertIndex = 0;
            while (insertIndex < handlers.Count && handlers[insertIndex].Priority > newHandlerInfo.Priority)
            {
                insertIndex++;
            }
            handlers.Insert(insertIndex, newHandlerInfo);
        }

        // 清理无效的处理程序
        private void CleanupInvalidHandlers(Type messageType, List<HandlerInfo> handlers)
        {
            lock (lockObject)
            {
                handlers.RemoveAll(h => !h.Handler.TryGetTarget(out _));
                if(handlers.Count == 0)
                {
                    messageDic.TryRemove(messageType, out _);
                }
            }
        }
    }
}