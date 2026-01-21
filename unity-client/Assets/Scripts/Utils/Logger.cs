using UnityEngine;

namespace PvpGame.Utils
{
    public static class Logger
    {
        public static void Log(string message, Object context = null)
        {
            Debug.Log($"<color=white>[LOG] {message}</color>", context);
        }

        public static void LogWarning(string message, Object context = null)
        {
            Debug.LogWarning($"<color=yellow>[WARNING] {message}</color>", context);
        }

        public static void LogError(string message, Object context = null)
        {
            Debug.LogError($"<color=red>[ERROR] {message}</color>", context);
        }

        public static void LogSuccess(string message, Object context = null)
        {
            Debug.Log($"<color=green>[SUCCESS] {message}</color>", context);
        }

        public static void LogNetwork(string message, Object context = null)
        {
            Debug.Log($"<color=cyan>[NETWORK] {message}</color>", context);
        }

        public static void LogAuth(string message, Object context = null)
        {
            Debug.Log($"<color=magenta>[AUTH] {message}</color>", context);
        }

        public static void LogGame(string message, Object context = null)
        {
            Debug.Log($"<color=blue>[GAME] {message}</color>", context);
        }
    }
}
