
namespace SingletonModel
{
    public interface ISingletonMono : ISingleton
    {
        /// <summary>
        /// 是否只存在当前场景
        /// </summary>
        bool onlySingleScene { get; }
    }
}