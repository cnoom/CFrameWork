using MessageModel;

namespace AutoModel
{
    /// <summary>
    /// 自动生成的消息处理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAutoHandler<in T>
    {
        void Handle(T msg);
    }
    
    public abstract class AutoHandler<T> : IAutoHandler<T>
    {
        public void Handle(T msg)
        {
            throw new System.NotImplementedException();
        }
    }
}