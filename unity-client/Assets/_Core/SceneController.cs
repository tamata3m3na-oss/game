using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleStar.Core
{
    public static class SceneController
    {
        private static bool isLoading;
        private static string currentScene;

        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoadCompleted;

        public static async Task LoadSceneAsync(string sceneName)
        {
            if (isLoading || string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning($"[SceneController] Cannot load scene {sceneName}. Loading: {isLoading}, Empty: {string.IsNullOrEmpty(sceneName)}");
                return;
            }

            Debug.Log($"[SceneController] Loading scene: {sceneName}");
            
            isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            var loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                await Task.Yield();
            }

            await Task.Delay(100);

            loadOperation.allowSceneActivation = true;

            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }

            currentScene = sceneName;
            isLoading = false;

            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneController] Scene loaded successfully: {sceneName}");
        }

        public static string GetCurrentScene()
        {
            return currentScene ?? SceneManager.GetActiveScene().name;
        }

        public static bool IsLoading()
        {
            return isLoading;
        }
    }
}