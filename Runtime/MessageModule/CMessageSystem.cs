using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SingletonModule;

namespace MessageModule
{
    public class CMessageSystem : Singleton<CMessageSystem>
    {
        private readonly Dictionary<Type, CMessageHandler> messageHandlers = new Dictionary<Type, CMessageHandler>();
        // 获取指定类型的消息处理程序，若不存在则创建新的
        private CMessageHandler GetOrCreateHandler(Type handleType)
        {
            if(!messageHandlers.TryGetValue(handleType, out var handler))
            {
                handler = new CMessageHandler();
                messageHandlers[handleType] = handler;
            }
            return handler;
        }

        // 获取指定逻辑类型的消息处理程序
        public CMessageHandler GetMessageHandler<THandler>() where THandler : IMessageHandler
        {
            return GetOrCreateHandler(typeof(THandler));
        }

        // 订阅指定逻辑类型的消息
        public void Subscribe<THandler, TMessage>([NotNull] Action<TMessage> handler, int priority = 0) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            var messageHandler = GetOrCreateHandler(typeof(THandler));
            messageHandler.Subscribe(handler, priority);
        }

        // 取消订阅指定逻辑类型的消息
        public void UnSubscribe<THandler, TMessage>([NotNull] Action<TMessage> handler) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out var messageHandler))
            {
                messageHandler.UnSubscribe(handler);
            }
        }

        // 发布指定逻辑类型的消息
        public void Publish<THandler, TMessage>([NotNull] TMessage message) where THandler : IMessageHandler
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message), "发布的消息不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out var messageHandler))
            {
                messageHandler.Publish(message);
            }
        }

        // 调整指定逻辑类型处理程序的优先级
        public void AdjustPriority<THandler, TMessage>([NotNull] Action<TMessage> handler, int newPriority) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out var messageHandler))
            {
                messageHandler.AdjustPriority(handler, newPriority);
            }
        }
    }
}