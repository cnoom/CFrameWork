using System;

namespace Base
{
    public static class GameModuleExtensions
    {
        public static void Subscribe<TEvent>(this GameModule module, Action<TEvent> handler, int priority = 0)
        {
            GameApp.Instance.MessageHandler.Subscribe(handler, priority);
        }

        public static void UnSubscribe<TEvent>(this GameModule module, Action<TEvent> handler)
        {
            GameApp.Instance.MessageHandler.UnSubscribe(handler);
        }

        public static void Publish<TEvent>(this GameModule module, TEvent e)
        {
            GameApp.Instance.MessageHandler.Publish(e);
        }

        public static void RegisterService<TInterface>(this GameModule module, TInterface implementation)
        {
            GameApp.Instance.RegisterService(typeof(TInterface), implementation);
        }

        public static TInterface GetService<TInterface>(this GameModule module, bool required = true)
            where TInterface : class
        {
            var service = GameApp.Instance.GetService(typeof(TInterface)) as TInterface;
            if(service == null && required)
            {
                throw new InvalidOperationException($"未注册的服务契约: {typeof(TInterface).Name}");
            }
            return service;
        }

        public static TInterface GetService<TInterface>(this GameModule module, Func<TInterface> factory)
            where TInterface : class
        {
            var service = module.GetService<TInterface>(required: false);
            if(service != null) return service;
            GameApp.Instance.RegisterService(typeof(TInterface), factory());
            return module.GetService<TInterface>();
        }

        public static GameModule GetModule<TModule>(this GameModule module) where TModule : GameModule
            => GameApp.Instance.GetModule<TModule>();

        public static GameModule GetModule(this GameModule module, Type moduleType)
            => GameApp.Instance.GetModule(moduleType);
    }
}