using UnityEngine;

namespace LogModule
{
    public static class LogExtensions
    {
        private static ILog _log = new CSimpleLog();

        public static void RegisterLog(ILog log)
        {
            _log = log;
        }

        public static void Log(this object obj, string message, Color color = default)
        {
            _log.Log($"{obj.GetType().Name}: {message}", color);
        }

        public static void LogWarning(this object obj, string message, Color color = default)
        {
            color = color == default ? Color.yellow : color;
            Debug.LogWarning(ColorString($"{obj.GetType().Name}: {message}", color));
        }

        public static void LogError(this object obj, string message, Color color = default)
        {
            color = color == default ? Color.red : color;
            Debug.LogError(ColorString($"{obj.GetType().Name}: {message}", color));
        }

        private static string ColorString(string text, Color color)
        {
            return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
        }
    }
}