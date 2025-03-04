using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Base
{
    /// <summary>
    /// 游戏模块基类
    /// 生命周期为: :注册->资源加载->自动注入依赖->开始->资源释放->取消注册
    /// </summary>
    public abstract class GameModule : MonoBehaviour
    {
        /// <summary>
        /// 模块是否随场景切换销毁
        /// </summary>
        public virtual bool IsDestroyOnChangeScene { get; protected set; } = false;

        /// <summary>
        /// 模块是否初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 所需模块依赖
        /// </summary>
        public List<Type> Dependencies { get; protected set; } = new List<Type>();

        protected GameModule() { }

        public virtual IEnumerator LoadResources()
        {
            yield break;
        }

        public virtual void OnRegister()
        {

        }

        internal void InitializeWithDependencies()
        {
            AutoInjectDependencies();
            OnStart();
            IsInitialized = true;
        }

        internal void AutoInjectDependencies()
        {
            IEnumerable<FieldInfo> fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<ModuleDependency>(true) != null);

            foreach (var field in fields)
            {
                GameModule module = this.GetModule(field.FieldType);
                if(!module) throw new MissingMemberException($"未找到依赖模块: {field.FieldType.Name}");
                field.SetValue(this, module);
            }
        }

        public virtual void OnStart() { }

        public virtual void ReleaseResources()
        {
        }

        public virtual void UnRegister()
        {

        }

        /// <summary>
        /// 自动注册周期
        /// </summary>
        private void Awake()
        {
            GameApp.Instance.RegisterModule(this);
        }
        private void Start() { }
        private void OnEnable() { }
        private void OnDisable() { }
        /// <summary>
        ///  自动取消注册周期
        /// </summary>
        private void OnDestroy()
        {
            ReleaseResources();
            if(!GameApp.Instance) return;
            GameApp.Instance.UnRegisterModule(this);
        }

        protected class ModuleDependency : Attribute
        {
        }
    }

}