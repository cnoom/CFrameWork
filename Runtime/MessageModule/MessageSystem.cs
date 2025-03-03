using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LogModule;
using SingletonModule;

namespace MessageModule
{
    public class MessageSystem : Singleton<MessageSystem>
    {
        private readonly Dictionary<Type, MessageHandler> messageHandlers = new Dictionary<Type, MessageHandler>();

        private MessageSystem() { }

        public MessageHandler GetOrCreateHandler<TMessageHandler>() where TMessageHandler : IMessageHandler
        {
            Type handleType = typeof(TMessageHandler);
            if(!messageHandlers.TryGetValue(handleType, out MessageHandler handler))
            {
                handler = new MessageHandler();
                messageHandlers[handleType] = handler;
            }
            return handler;
        }

        public void Clear<TMessageHandler>() where TMessageHandler : IMessageHandler
        {
            Type handleType = typeof(TMessageHandler);
            if(!messageHandlers.TryGetValue(handleType, out MessageHandler handler))
            {
                return;
            }
            messageHandlers.Remove(handleType);
        }

        /// <summary>
        /// 订阅方法,允许指定处理程序的优先级
        /// </summary>
        /// <param name="handler">  注册的处理程序 </param>
        /// <param name="priority"> 优先级大的优先 </param>
        public void Subscribe<THandler, TMessage>([NotNull] Action<TMessage> handler, int priority = 0) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                this.LogError("消息处理程序不能为空。");
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            MessageHandler messageHandler = GetOrCreateHandler<THandler>();
            messageHandler.Subscribe(handler, priority);
        }

        // 取消订阅指定逻辑类型的消息
        public void UnSubscribe<THandler, TMessage>([NotNull] Action<TMessage> handler) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                this.LogError("消息处理程序不能为空。");
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out MessageHandler messageHandler))
            {
                messageHandler.UnSubscribe(handler);
            }
        }

        // 发布指定逻辑类型的消息
        public void Publish<THandler, TMessage>([NotNull] TMessage message) where THandler : IMessageHandler
        {
            if(message == null)
            {
                this.LogError("发布的消息不能为空。");
                throw new ArgumentNullException(nameof(message), "发布的消息不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out MessageHandler messageHandler))
            {
                messageHandler.Publish(message);
            }
        }

        // 调整指定逻辑类型处理程序的优先级
        public void AdjustPriority<THandler, TMessage>([NotNull] Action<TMessage> handler, int newPriority) where THandler : IMessageHandler
        {
            if(handler == null)
            {
                this.LogError("消息处理程序不能为空。");
                throw new ArgumentNullException(nameof(handler), "消息处理程序不能为空。");
            }
            if(messageHandlers.TryGetValue(typeof(THandler), out MessageHandler messageHandler))
            {
                messageHandler.AdjustPriority(handler, newPriority);
            }
        }
    }
}