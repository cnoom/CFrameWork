using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace SingletonModule
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, ISingletonMono where T : SingletonMonoBehaviour<T>
    {
        private static T instance;
        private static readonly object Lock = new object();
        private ISingletonMono singletonMonoImplementation;
        private static bool isQuitGame;
        public static T Instance
        {
            get
            {
                if(isQuitGame) return null;
                if(instance) return instance;
                lock (Lock)
                {
                    if(instance) return instance;
                    instance = FindAnyObjectByType<T>();
                    Application.quitting += () => isQuitGame = true;
                    if(!instance)
                    {
                        instance = SingletonCreator.CreateMonoSingleton<T>();
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
                instance = null;
            }
        }
        public abstract bool onlySingleScene { get; }

        public virtual void OnSingletonInit()
        {
        }
    }
}