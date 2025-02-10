using System;

namespace MessageModel
{
    // 用于存储带优先级的委托
    public class PrioritizedDelegate
    {
        /// <summary>
        /// 委托的优先级，数值越大优先级越高
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// 委托的弱引用，用于避免内存泄漏
        /// </summary>
        public WeakReference<MulticastDelegate> handlerRef { get; set; }

        /// <summary>
        /// 委托所属的场景名称，若为全局委托则为 null
        /// </summary>
        public string sceneName { get; set; }

        /// <summary>
        /// 标记委托是否为全局委托
        /// </summary>
        public bool isGlobal { get; set; }

        /// <summary>
        /// 消息过滤函数，用于决定是否处理该消息
        /// </summary>
        public Func<object, bool> filter { get; set; }

        /// <summary>
        /// 存储委托的实际类型，用于避免重复的类型转换
        /// </summary>
        public Type delegateType { get; set; }
    }
}