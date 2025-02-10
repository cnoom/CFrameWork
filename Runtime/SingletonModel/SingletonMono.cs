using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace SingletonModel
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, ISingletonMono where T : SingletonMonoBehaviour<T>
    {
        private static T instance;
        private static readonly object Lock = new object();
        public abstract bool onlySingleScene { get; }
        private ISingletonMono singletonMonoImplementation;

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (Lock)
                    {
                        if(instance == null)
                        {
                            instance = FindAnyObjectByType<T>();

                            if(instance == null)
                            {
                                instance = SingletonCreator.CreateMonoSingleton<T>();
                            }
                        }
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if(instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this as T;
                Debug.Assert(instance != null, nameof(instance) + " != null");
                instance.OnSingletonInit();
                if(!onlySingleScene)
                {
                    DontDestroyOnLoad(instance);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if(instance == this)
            {
                Dispose();
            }
        }
        public virtual void Dispose()
        {
            instance = null;
        }

        public virtual void OnSingletonInit()
        {
        }
    }
}