using System;
using UnityEngine;

namespace LogModule
{
    public class CSimpleLog : ILog
    {
        private Action<string> onLog;
        private Action<string> onLogError;
        private Action<string> onLogWarning;

        public void Log(string message, Color color = default)
        {
            color = color == default ? Color.white : color;
            Debug.Log(ColorString(message, color));
            onLog?.Invoke(message);
        }

        public void LogWarning(string message, Color color = default)
        {
            color = color == default ? Color.yellow : color;
            Debug.LogWarning(ColorString(message, color));
            onLogWarning?.Invoke(message);
        }

        public void LogError(string message, Color color = default)
        {
            color = color == default ? Color.red : color;
            Debug.LogError(ColorString(message, color));
            onLogError?.Invoke(message);
        }

        public void RegisterLogHandler(Action<string> handler)
        {
            onLog += handler;
        }

        public void RegisterLogWarningHandler(Action<string> handler)
        {
            onLogWarning += handler;
        }

        public void RegisterLogErrorHandler(Action<string> handler)
        {
            onLogError += handler;
        }

        public void UnRegisterLogHandler(Action<string> handler)
        {
            onLog -= handler;
        }

        public void UnRegisterLogWarningHandler(Action<string> handler)
        {
            onLogWarning -= handler;
        }

        public void UnRegisterLogErrorHandler(Action<string> handler)
        {
            onLogError -= handler;
        }

        private static string ColorString(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
    }
}