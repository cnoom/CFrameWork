using System;

namespace MessageModule
{
    public static class CMessageHandlerExtension
    {
        // 批量订阅方法
        public static void SubscribeBatch<T>(this CMessageHandler self,params (Action<T> handler, int priority)[] handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers), "Handlers array cannot be null.");
            }

            foreach ((Action<T> handler, int priority) in handlers)
            {
                self.Subscribe(handler, priority);
            }
        }
        
        // 批量取消订阅方法
        public static void UnSubscribeBatch<T>(this CMessageHandler self,params Action<T>[] handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers), "Handlers array cannot be null.");
            }

            foreach (var handler in handlers)
            {
                self.UnSubscribe(handler);
            }
        }
    }
}