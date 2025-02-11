namespace SingletonModule
{
    public class Singleton<T> : ISingleton where T : Singleton<T>
    {
        
        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (singletonLock)
                    {
                        if(instance == null)
                        {
                            instance = SingletonCreator.CreateSingleton<T>();
                        }
                    }
                }

                return instance;
            }
        }

        public virtual void OnSingletonInit()
        {
        }

        private static T instance;
        /// <summary>
        ///     线程锁
        /// </summary>
        private static readonly object singletonLock = new object();

    }
}