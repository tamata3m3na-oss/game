#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PvpGame.Editor
{
    public class ProjectHealthCheck : EditorWindow
    {
        [MenuItem("PvP/Project Health Check")]
        public static void ShowWindow()
        {
            GetWindow<ProjectHealthCheck>("Health Check");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity PvP Client - Health Check", EditorStyles.boldLabel);
            GUILayout.Space(10);

            CheckUnityVersion();
            CheckScenes();
            CheckPackages();
            CheckScripts();

            GUILayout.Space(20);

            if (GUILayout.Button("Open Scene Setup Guide"))
            {
                Application.OpenURL("file://" + Application.dataPath + "/../SCENE_SETUP_GUIDE.md");
            }

            if (GUILayout.Button("Open README"))
            {
                Application.OpenURL("file://" + Application.dataPath + "/../README.md");
            }
        }

        private void CheckUnityVersion()
        {
            GUILayout.Label("Unity Version:", EditorStyles.boldLabel);
            string version = Application.unityVersion;
            bool isCorrect = version.StartsWith("2022.3");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Current: {version}");
            GUILayout.Label(isCorrect ? "✓" : "✗", isCorrect ? GetGreenStyle() : GetRedStyle());
            EditorGUILayout.EndHorizontal();

            if (!isCorrect)
            {
                EditorGUILayout.HelpBox("Project requires Unity 2022.3.62f3", MessageType.Warning);
            }

            GUILayout.Space(10);
        }

        private void CheckScenes()
        {
            GUILayout.Label("Scenes:", EditorStyles.boldLabel);

            string[] requiredScenes = { "Login", "Lobby", "Game", "Result" };
            int foundScenes = 0;

            foreach (string sceneName in requiredScenes)
            {
                string scenePath = $"Assets/Scenes/{sceneName}.unity";
                bool exists = System.IO.File.Exists(scenePath);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{sceneName}.unity");
                GUILayout.Label(exists ? "✓" : "✗", exists ? GetGreenStyle() : GetRedStyle());
                EditorGUILayout.EndHorizontal();

                if (exists) foundScenes++;
            }

            if (foundScenes < 4)
            {
                EditorGUILayout.HelpBox("Some scenes are missing. See SCENE_SETUP_GUIDE.md", MessageType.Warning);
            }

            GUILayout.Space(10);
        }

        private void CheckPackages()
        {
            GUILayout.Label("Required Packages:", EditorStyles.boldLabel);

            string[] packages = {
                "com.unity.inputsystem",
                "com.unity.textmeshpro",
                "com.unity.ugui"
            };

            foreach (string package in packages)
            {
                bool exists = System.IO.Directory.Exists($"Packages/{package}") ||
                              System.IO.Directory.Exists($"Library/PackageCache/{package}@*");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(package);
                GUILayout.Label(exists ? "✓" : "?", exists ? GetGreenStyle() : GetYellowStyle());
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        private void CheckScripts()
        {
            GUILayout.Label("Core Scripts:", EditorStyles.boldLabel);

            string[] scripts = {
                "Assets/Scripts/Auth/AuthManager.cs",
                "Assets/Scripts/Network/NetworkManager.cs",
                "Assets/Scripts/Game/GameManager.cs",
                "Assets/Scripts/UI/LoginUI.cs",
                "Assets/Scripts/UI/LobbyUI.cs",
                "Assets/Scripts/Config/GameConfig.cs"
            };

            int foundScripts = 0;

            foreach (string script in scripts)
            {
                bool exists = System.IO.File.Exists(script);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(System.IO.Path.GetFileName(script));
                GUILayout.Label(exists ? "✓" : "✗", exists ? GetGreenStyle() : GetRedStyle());
                EditorGUILayout.EndHorizontal();

                if (exists) foundScripts++;
            }

            if (foundScripts == scripts.Length)
            {
                EditorGUILayout.HelpBox("All core scripts present!", MessageType.Info);
            }
        }

        private GUIStyle GetGreenStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.green;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        private GUIStyle GetRedStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.red;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        private GUIStyle GetYellowStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.yellow;
            style.fontStyle = FontStyle.Bold;
            return style;
        }
    }
}
#endif
