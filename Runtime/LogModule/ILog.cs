using UnityEngine;

namespace LogModule
{
    public interface ILog
    {
        void Log(string message, Color color = default);
        void LogWarning(string message, Color color = default);
        void LogError(string message, Color color = default);
    }
}