using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AddressableService;
using Base.Attributes;
using LogModule;
using MessageModule;
using SingletonModule;
using UnityEngine;

namespace Base
{
    // 游戏入口和模块管理
    public class GameApp : SingletonMonoBehaviour<GameApp>
    {
        public override bool onlySingleScene => false;

        // 模块容器
        private readonly Dictionary<Type, GameModule> modules = new();
        private readonly Dictionary<Type, object> services = new();


        // 核心消息总线（关键通讯枢纽）
        public readonly MessageHandler MessageHandler = new();


        // 获取模块API
        private DependencyGraph<GameModule> dependencyGraph = new();

        #region 模组操作

        public GameModule GetModule(Type type) => modules[type];
        public T GetModule<T>() where T : GameModule => GetModule(typeof(T)) as T;

        public void UnRegisterModule(GameModule module)
        {
            var type = module.GetType();
            dependencyGraph.RemoveNode(type);
            modules.Remove(type);
        }

        public void RegisterModule(GameModule module)
        {
            // 添加模块到依赖图
            modules.Add(module.GetType(), module);
            dependencyGraph.AddNode(module);
            if(!module.IsDestroyOnChangeScene) module.transform.SetParent(transform);
            module.OnRegister();
            foreach (Type dependency in module.Dependencies)
            {
                dependencyGraph.AddDependency(module, dependency);
            }

            // 延迟初始化直到所有依赖就绪
            StartCoroutine(InitializeModuleWithDependencies(module));
        }

        private IEnumerator InitializeModuleWithDependencies(GameModule module)
        {
            // 等待所有依赖模块初始化完成
            foreach (var dependencyType in module.Dependencies)
            {
                while (!modules.ContainsKey(dependencyType) || !modules[dependencyType].IsInitialized)
                {
                    yield return null;
                }
            }
            yield return module.LoadResources();
            module.InitializeWithDependencies();
        }

        #endregion

        protected override void Awake()
        {
            if(!Application.isPlaying) return;
            base.Awake();
            InitServices();
        }

        #region 服务模块操作

        public void RegisterService(Type serviceType, object implementation)
        {
            if(services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"服务 {serviceType.Name} 已存在，将被覆盖");
            }
            services[serviceType] = implementation;
        }


        public T GetService<T>() => (T)GetService(typeof(T));
        public object GetService(Type serviceType)
        {
            return services.TryGetValue(serviceType, out var service)
                ? service
                : null;
        }

        protected virtual void InitServices()
        {
            GameObject autoRegisterServiceObject = new GameObject("AutoRegisterService");
            autoRegisterServiceObject.transform.SetParent(transform);
            Dictionary<string, Type> servicesDict = new Dictionary<string, Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes().Where(t => t.GetCustomAttribute<AutoRegisterServiceAttribute>() != null))
                {
                    TrySetAutoService(servicesDict, type);
                }
            }
            
            foreach ((string serviceName, Type type) in servicesDict)
            {
                GameObject serviceObject = new GameObject(serviceName);
                serviceObject.transform.SetParent(autoRegisterServiceObject.transform);
                RegisterService(type, serviceObject.AddComponent(type));
            }
        }

        private void TrySetAutoService(Dictionary<string, Type> dict, Type type)
        {
            if(!typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                this.LogError($"类型 {type.Name} 未继承 MonoBehaviour，无法自动注册!");
                return;
            }
            AutoRegisterServiceAttribute newAttribute = type.GetCustomAttribute<AutoRegisterServiceAttribute>();
            if(dict.TryAdd(newAttribute.ServiceName, type))
            {
                return;
            }
            if(newAttribute.IsSubstitute)
            {
                dict[newAttribute.ServiceName] = type;
            }
        }

        #endregion

        protected new virtual void OnDestroy()
        {
            foreach (GameModule module in modules.Values)
            {
                module.ReleaseResources();
                module.UnRegister();
            }
            modules.Clear();
            services.Clear();
        }
    }

}