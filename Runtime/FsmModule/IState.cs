namespace FsmModule
{
    public interface IState<T>
    {
        T Onwner { get; set; }

        void Enter();
        void Update();
        void Exit();
        bool Condition();
    }
}