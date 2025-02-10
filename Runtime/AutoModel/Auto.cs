using MessageModel;

namespace AutoModel
{
    ///  自动执行函数使用方式
    ///  自动执行的方法通过注册消息来执行：
    ///  1. 注册消息：MessageHandleCenter.Instance.RegisterMessage<AAutoHandler<T>>(AutoFunc)
    ///  2. 继承某接口或某抽象类自动发送消息将自己传递出去：MessageHandleCenter.Instance.Publish<Type>(t)
    public interface IAuto
    {

    }
}