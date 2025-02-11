using System;
using System.Collections;
using SingletonModule;
using UnityEngine;

namespace ExceptionCaptureModule
{
    public class CExceptionSystem : SingletonMonoBehaviour<CExceptionSystem>
    {
        public override bool onlySingleScene => false;
        private Action<Exception, string> exceptionHandler;
        private Action<string, string> logHandler;
        public override void OnSingletonInit()
        {
            Application.logMessageReceived += HandleUnityLog;
            AppDomain.CurrentDomain.UnhandledException += OnHandleException;
        }

        public void Init()
        {
            
        }

        private void OnHandleException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.LogError("异常!" + e.ExceptionObject);
        }

        private void HandleUnityLog(string condition, string stacktrace, LogType type)
        {
            if(type == LogType.Exception)
            {
                HandleException(condition, stacktrace, "CFrameworkCapture:");
            }
        }

        public void RegisterExceptionHandler(Action<Exception, string> handler)
        {
            exceptionHandler += handler;
        }

        public void RegisterLogHandler(Action<string, string> handler)
        {
            logHandler += handler;
        }

        public void UnRegisterExceptionHandler(Action<Exception, string> handler)
        {
            exceptionHandler -= handler;
        }

        public void UnRegisterLogHandler(Action<string, string> handler)
        {
            logHandler -= handler;
        }

        private void HandleException(string condition, string stackTrace, string source)
        {
            Debug.LogError(source + condition + "\n" + stackTrace);
            logHandler?.Invoke(condition, stackTrace);
        }

        public void HandleException(Exception exception, string source)
        {
            Debug.LogError("异常!" + exception.Message);
            exceptionHandler?.Invoke(exception, source);
        }
    }
}