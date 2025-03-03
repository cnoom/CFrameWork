using System;
using System.Reflection;
using UnityEngine;

namespace SingletonModule
{
    internal class SingletonCreator
    {
        public static T CreateSingleton<T>() where T : class, ISingleton
        {
            T instance = CreateNonPublicConstructorObject<T>();
            instance.OnSingletonInit();
            return instance;
        }

        public static T CreateMonoSingleton<T>() where T : MonoBehaviour, ISingletonMono
        {
            GameObject go = new GameObject("Singleton_" + typeof(T).Name);
            T instance = go.AddComponent<T>();
            instance.OnSingletonInit();
            return instance;
        }

        private static T CreateNonPublicConstructorObject<T>() where T : class, ISingleton
        {
            Type type = typeof(T);
            // 获取私有构造函数
            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // 获取无参构造函数
            ConstructorInfo ctor = Array.Find(constructorInfos, c => c.GetParameters().Length == 0);

            if(ctor == null)
            {
                throw new Exception("Non-Publish Constructor() not found! in " + type);
            }

            T result = (T)ctor.Invoke(null);
            return result;
        }
    }
}