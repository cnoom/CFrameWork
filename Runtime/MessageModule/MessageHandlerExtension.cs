using System;
using LogModule;

namespace MessageModule
{
    public static class MessageHandlerExtension
    {
        // 批量订阅方法
        public static void SubscribeBatch<T>(this MessageHandler self, params (Action<T> handler, int priority)[] handlers)
        {
            if(handlers == null)
            {
                self.LogError("Handlers array cannot be null.");
                throw new ArgumentNullException(nameof(handlers), "Handlers array cannot be null.");
            }

            foreach ((Action<T> handler, int priority) in handlers)
            {
                self.Subscribe(handler, priority);
            }
        }

        // 批量取消订阅方法
        public static void UnSubscribeBatch<T>(this MessageHandler self, params Action<T>[] handlers)
        {
            if(handlers == null)
            {
                self.LogError("Handlers array cannot be null.");
                throw new ArgumentNullException(nameof(handlers), "Handlers array cannot be null.");
            }

            foreach (Action<T> handler in handlers)
            {
                self.UnSubscribe(handler);
            }
        }
    }
}