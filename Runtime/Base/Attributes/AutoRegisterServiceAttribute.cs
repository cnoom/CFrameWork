using System;

namespace Base.Attributes
{
    
    /// <summary>
    /// 自动注册服务特性(需要类继承MonoBehaviour)，用于标记需要自动注册的服务类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutoRegisterServiceAttribute : Attribute
    {
        public const string AssetsServiceKey = "AssetsService";
        /// <summary>
        /// 如果存在同名服务是否会替换为当前服务
        /// </summary>
        public bool IsSubstitute;
        public string ServiceName;
        public AutoRegisterServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}