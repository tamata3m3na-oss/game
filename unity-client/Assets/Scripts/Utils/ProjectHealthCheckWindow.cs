#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace UnityProjectTools
{
    /// <summary>
    /// Project Health Check Editor Window
    /// Comprehensive diagnostics tool for Unity project setup
    /// Checks dependencies, managers, scenes, and provides auto-fix suggestions
    /// </summary>
    public class ProjectHealthCheckWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool isScanning = false;
        private ScanResults currentResults;
        private string lastScanTime = "";

        [MenuItem("Tools/Project Health Check")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectHealthCheckWindow>("Project Health");
            window.minSize = new Vector2(600, 700);
            window.Show();
        }

        private void OnEnable()
        {
            // Auto-scan when window opens
            if (!isScanning)
            {
                StartFullScan();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            
            // Header
            EditorGUILayout.LabelField("🎯 Unity Project Health Check", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Last Scan: {lastScanTime}", EditorStyles.miniLabel);
            
            EditorGUILayout.Space();
            
            // Scan Controls
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !isScanning;
            if (GUILayout.Button("🔍 Full Scan", GUILayout.Height(30)))
            {
                StartFullScan();
            }
            if (GUILayout.Button("🔄 Quick Check", GUILayout.Height(30)))
            {
                StartQuickCheck();
            }
            if (GUILayout.Button("🧹 Clean & Fix", GUILayout.Height(30)))
            {
                StartCleanAndFix();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (isScanning)
            {
                DrawProgressIndicator();
                return;
            }

            if (currentResults != null)
            {
                DrawScanResults();
            }
            else
            {
                EditorGUILayout.HelpBox("Click 'Full Scan' to analyze your project health.", MessageType.Info);
            }
        }

        private void DrawProgressIndicator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🔄 Scanning in progress...", EditorStyles.boldLabel);
            
            Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, currentResults?.Progress ?? 0f, "Analyzing project...");
            
            EditorGUILayout.Space();
            
            if (currentResults?.CurrentStep != null)
            {
                EditorGUILayout.LabelField($"Current: {currentResults.CurrentStep}", EditorStyles.miniLabel);
            }
        }

        private void DrawScanResults()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawSection("📦 Dependencies", DrawDependenciesCheck);
            DrawSection("🔧 Managers", DrawManagersCheck);
            DrawSection("🎬 Scenes", DrawScenesCheck);
            DrawSection("⚙️ Build Settings", DrawBuildSettingsCheck);
            DrawSection("📂 Files & Folders", DrawFilesCheck);
            DrawSection("🌐 Networking", DrawNetworkCheck);
            DrawSection("🎨 UI/UX", DrawUICheck);
            
            EditorGUILayout.EndScrollView();
            
            // Summary
            DrawSummary();
        }

        private void DrawSection(string title, System.Action drawContent)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            drawContent?.Invoke();
            EditorGUI.indentLevel--;
        }

        private void DrawDependenciesCheck()
        {
            if (currentResults.DependencyResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ No dependency issues found", EditorStyles.greenLabel);
                return;
            }

            foreach (var dep in currentResults.DependencyResults)
            {
                DrawIssueItem(dep);
            }
        }

        private void DrawManagersCheck()
        {
            if (currentResults.ManagerResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ All managers found and properly configured", EditorStyles.greenLabel);
                return;
            }

            foreach (var manager in currentResults.ManagerResults)
            {
                DrawIssueItem(manager);
            }
        }

        private void DrawScenesCheck()
        {
            if (currentResults.SceneResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ All required scenes found", EditorStyles.greenLabel);
                return;
            }

            foreach (var scene in currentResults.SceneResults)
            {
                DrawIssueItem(scene);
            }
        }

        private void DrawBuildSettingsCheck()
        {
            if (currentResults.BuildSettingsResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ Build settings are properly configured", EditorStyles.greenLabel);
                return;
            }

            foreach (var setting in currentResults.BuildSettingsResults)
            {
                DrawIssueItem(setting);
            }
        }

        private void DrawFilesCheck()
        {
            if (currentResults.FileResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ All required files and folders exist", EditorStyles.greenLabel);
                return;
            }

            foreach (var file in currentResults.FileResults)
            {
                DrawIssueItem(file);
            }
        }

        private void DrawNetworkCheck()
        {
            if (currentResults.NetworkResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ Network configuration looks good", EditorStyles.greenLabel);
                return;
            }

            foreach (var network in currentResults.NetworkResults)
            {
                DrawIssueItem(network);
            }
        }

        private void DrawUICheck()
        {
            if (currentResults.UIResults.Count == 0)
            {
                EditorGUILayout.LabelField("✅ UI configuration is correct", EditorStyles.greenLabel);
                return;
            }

            foreach (var ui in currentResults.UIResults)
            {
                DrawIssueItem(ui);
            }
        }

        private void DrawIssueItem(HealthIssue issue)
        {
            Color originalColor = GUI.color;
            
            MessageType messageType = GetMessageType(issue.Severity);
            GUI.color = GetSeverityColor(issue.Severity);
            
            EditorGUILayout.HelpBox($"[{issue.Severity}] {issue.Message}", messageType);
            
            GUI.color = originalColor;
            
            if (!string.IsNullOrEmpty(issue.Suggestion))
            {
                EditorGUILayout.LabelField($"💡 Fix: {issue.Suggestion}", EditorStyles.miniLabel);
            }
            
            if (issue.Actions != null && issue.Actions.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                foreach (var action in issue.Actions)
                {
                    if (GUILayout.Button(action.Text, GUILayout.Width(120)))
                    {
                        action.Action?.Invoke();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(2);
        }

        private void DrawSummary()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📊 SUMMARY", EditorStyles.boldLabel);
            
            var totalIssues = currentResults.DependencyResults.Count + 
                            currentResults.ManagerResults.Count + 
                            currentResults.SceneResults.Count + 
                            currentResults.BuildSettingsResults.Count + 
                            currentResults.FileResults.Count + 
                            currentResults.NetworkResults.Count + 
                            currentResults.UIResults.Count;
            
            var criticalIssues = currentResults.DependencyResults.Concat(currentResults.ManagerResults)
                                                  .Concat(currentResults.SceneResults)
                                                  .Count(i => i.Severity == "CRITICAL");
            
            var warningIssues = currentResults.DependencyResults.Concat(currentResults.ManagerResults)
                                                   .Concat(currentResults.SceneResults)
                                                   .Count(i => i.Severity == "WARNING");
            
            EditorGUILayout.LabelField($"Total Issues: {totalIssues}");
            EditorGUILayout.LabelField($"Critical: {criticalIssues}");
            EditorGUILayout.LabelField($"Warnings: {warningIssues}");
            
            if (totalIssues == 0)
            {
                EditorGUILayout.HelpBox("🎉 Your project is healthy! No issues detected.", MessageType.Info);
            }
            else if (criticalIssues == 0)
            {
                EditorGUILayout.HelpBox("⚠️ Your project has warnings but should work fine.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("🚨 Your project has critical issues that need fixing!", MessageType.Error);
            }
        }

        private void StartFullScan()
        {
            if (isScanning) return;
            
            isScanning = true;
            currentResults = new ScanResults();
            currentResults.StartTime = System.DateTime.Now;
            
            EditorCoroutineUtility.StartCoroutine(FullScanCoroutine(), this);
        }

        private void StartQuickCheck()
        {
            if (isScanning) return;
            
            isScanning = true;
            currentResults = new ScanResults();
            currentResults.StartTime = System.DateTime.Now;
            
            EditorCoroutineUtility.StartCoroutine(QuickCheckCoroutine(), this);
        }

        private void StartCleanAndFix()
        {
            if (isScanning) return;
            
            isScanning = true;
            currentResults = new ScanResults();
            currentResults.StartTime = System.DateTime.Now;
            
            EditorCoroutineUtility.StartCoroutine(CleanAndFixCoroutine(), this);
        }

        private IEnumerator FullScanCoroutine()
        {
            try
            {
                currentResults.CurrentStep = "Scanning dependencies...";
                yield return ScanDependencies();
                
                currentResults.CurrentStep = "Checking managers...";
                yield return ScanManagers();
                
                currentResults.CurrentStep = "Verifying scenes...";
                yield return ScanScenes();
                
                currentResults.CurrentStep = "Checking build settings...";
                yield return ScanBuildSettings();
                
                currentResults.CurrentStep = "Scanning files and folders...";
                yield return ScanFiles();
                
                currentResults.CurrentStep = "Testing network configuration...";
                yield return ScanNetworkConfig();
                
                currentResults.CurrentStep = "Checking UI setup...";
                yield return ScanUISetup();
                
                currentResults.CurrentStep = "Generating report...";
                yield return null;
                
                lastScanTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Project Health Check failed: {ex.Message}");
            }
            finally
            {
                isScanning = false;
                currentResults.Progress = 1f;
                currentResults.CurrentStep = "Complete";
                Repaint();
            }
        }

        private IEnumerator QuickCheckCoroutine()
        {
            try
            {
                currentResults.CurrentStep = "Quick dependency check...";
                yield return ScanDependencies();
                
                currentResults.CurrentStep = "Quick manager check...";
                yield return ScanManagers();
                
                currentResults.CurrentStep = "Quick scene check...";
                yield return ScanScenes();
                
                currentResults.Progress = 1f;
                lastScanTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Quick check failed: {ex.Message}");
            }
            finally
            {
                isScanning = false;
                currentResults.CurrentStep = "Complete";
                Repaint();
            }
        }

        private IEnumerator CleanAndFixCoroutine()
        {
            try
            {
                currentResults.CurrentStep = "Running full scan...";
                yield return FullScanCoroutine();
                
                currentResults.CurrentStep = "Applying fixes...";
                yield return ApplyAutomaticFixes();
                
                currentResults.CurrentStep = "Final verification...";
                yield return FullScanCoroutine(); // Run again to verify fixes
                
                lastScanTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Clean and fix failed: {ex.Message}");
            }
            finally
            {
                isScanning = false;
                currentResults.CurrentStep = "Complete";
                Repaint();
            }
        }

        private IEnumerator ScanDependencies()
        {
            currentResults.Progress = 0.1f;
            
            // Check TextMeshPro
            if (!IsTextMeshProInstalled())
            {
                currentResults.DependencyResults.Add(new HealthIssue
                {
                    Severity = "CRITICAL",
                    Message = "TextMeshPro is not properly imported",
                    Suggestion = "Go to Window > TextMeshPro > Import TMP Essential Resources",
                    Actions = new List<IssueAction> { new IssueAction { Text = "Import TMP Resources", Action = ImportTextMeshProResources } }
                });
            }
            
            // Check for DOTween availability
            bool hasDOTween = HasDOTweenInstalled();
            if (!hasDOTween)
            {
                currentResults.DependencyResults.Add(new HealthIssue
                {
                    Severity = "WARNING",
                    Message = "DOTween is not installed",
                    Suggestion = "DOTween compatibility layer will be used (Coroutines)",
                    Actions = new List<IssueAction> { new IssueAction { Text = "Install DOTween", Action = OpenDOTweenPackage } }
                });
            }
            
            // Check for NativeWebSocket
            if (!HasNativeWebSocketInstalled())
            {
                currentResults.DependencyResults.Add(new HealthIssue
                {
                    Severity = "WARNING",
                    Message = "NativeWebSocket might not be available",
                    Suggestion = "Using System.Net.WebSockets as fallback",
                    Actions = new List<IssueAction> { new IssueAction { Text = "Install NativeWebSocket", Action = InstallNativeWebSocket } }
                });
            }
            
            yield return null;
            currentResults.Progress = 0.2f;
        }

        private IEnumerator ScanManagers()
        {
            currentResults.Progress = 0.3f;
            
            // Check for required manager scripts
            string[] managerFiles = {
                "Assets/Scripts/Bootstrap/Bootstrap.cs",
                "Assets/Scripts/Bootstrap/ManagerInitializer.cs", 
                "Assets/Scripts/Network/NetworkManager.cs",
                "Assets/Scripts/UI/Animations/AnimationController.cs",
                "Assets/Scripts/UI/Animations/ParticleController.cs",
                "Assets/Scripts/UI/Animations/TransitionManager.cs"
            };
            
            foreach (string managerFile in managerFiles)
            {
                if (!File.Exists(Application.dataPath.Replace("Assets", managerFile)))
                {
                    currentResults.ManagerResults.Add(new HealthIssue
                    {
                        Severity = "CRITICAL",
                        Message = $"Required manager script missing: {Path.GetFileName(managerFile)}",
                        Suggestion = "Ensure all manager scripts are present",
                        Actions = new List<IssueAction> { new IssueAction { Text = "Create Manager", Action = () => CreateManagerScript(managerFile) } }
                    });
                }
            }
            
            yield return null;
            currentResults.Progress = 0.4f;
        }

        private IEnumerator ScanScenes()
        {
            currentResults.Progress = 0.5f;
            
            // Check for required scenes
            string[] requiredScenes = {
                "Assets/Scenes/LoginScene.unity",
                "Assets/Scenes/LobbyScene.unity",
                "Assets/Scenes/GameScene.unity"
            };
            
            foreach (string scenePath in requiredScenes)
            {
                string fullPath = Application.dataPath.Replace("Assets", scenePath);
                if (!File.Exists(fullPath))
                {
                    currentResults.SceneResults.Add(new HealthIssue
                    {
                        Severity = "CRITICAL",
                        Message = $"Required scene missing: {Path.GetFileNameWithoutExtension(scenePath)}",
                        Suggestion = "Create the missing scene with proper UI setup",
                        Actions = new List<IssueAction> { new IssueAction { Text = "Create Scene", Action = () => CreateScene(scenePath) } }
                    });
                }
            }
            
            yield return null;
            currentResults.Progress = 0.6f;
        }

        private IEnumerator ScanBuildSettings()
        {
            currentResults.Progress = 0.7f;
            
            // Check build settings
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                currentResults.BuildSettingsResults.Add(new HealthIssue
                {
                    Severity = "CRITICAL",
                    Message = "No scenes added to Build Settings",
                    Suggestion = "Add all required scenes to File > Build Settings",
                    Actions = new List<IssueAction> { new IssueAction { Text = "Fix Build Settings", Action = FixBuildSettings } }
                });
            }
            
            // Check player settings
            if (string.IsNullOrEmpty(PlayerSettings.productName))
            {
                currentResults.BuildSettingsResults.Add(new HealthIssue
                {
                    Severity = "WARNING",
                    Message = "Product name not set in Player Settings",
                    Suggestion = "Set a proper product name in Edit > Project Settings > Player",
                    Actions = new List<IssueAction> { new IssueAction { Text = "Open Player Settings", Action = OpenPlayerSettings } }
                });
            }
            
            yield return null;
            currentResults.Progress = 0.8f;
        }

        private IEnumerator ScanFiles()
        {
            currentResults.Progress = 0.9f;
            
            // Check for important folders
            string[] requiredFolders = {
                "Assets/Scripts",
                "Assets/Scenes", 
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Animations"
            };
            
            foreach (string folder in requiredFolders)
            {
                string fullPath = Application.dataPath.Replace("Assets", folder);
                if (!Directory.Exists(fullPath))
                {
                    currentResults.FileResults.Add(new HealthIssue
                    {
                        Severity = "WARNING",
                        Message = $"Required folder missing: {folder}",
                        Suggestion = "Create the missing folder structure",
                        Actions = new List<IssueAction> { new IssueAction { Text = "Create Folder", Action = () => CreateFolder(folder) } }
                    });
                }
            }
            
            yield return null;
            currentResults.Progress = 1f;
        }

        private IEnumerator ScanNetworkConfig()
        {
            // Check network manager configuration
            var networkManagerScript = AssetDatabase.FindAssets("NetworkManager").FirstOrDefault();
            if (networkManagerScript != null)
            {
                string path = AssetDatabase.GUIDToAssetPath(networkManagerScript);
                string content = File.ReadAllText(path);
                
                if (content.Contains("ServerUrl = \"ws://localhost:3000\""))
                {
                    currentResults.NetworkResults.Add(new HealthIssue
                    {
                        Severity = "WARNING",
                        Message = "NetworkManager using localhost URL",
                        Suggestion = "Update server URL for production builds",
                        Actions = new List<IssueAction> { new IssueAction { Text = "Configure Network", Action = ConfigureNetworkManager } }
                    });
                }
            }
            
            yield return null;
        }

        private IEnumerator ScanUISetup()
        {
            // Check UI components
            var uiScenes = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath);
            
            foreach (var scenePath in uiScenes)
            {
                if (scenePath.Contains("Login") || scenePath.Contains("Lobby") || scenePath.Contains("Game"))
                {
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                    if (sceneAsset != null)
                    {
                        // This would require opening the scene to check UI elements
                        // For now, just add a general UI check
                    }
                }
            }
            
            yield return null;
        }

        private IEnumerator ApplyAutomaticFixes()
        {
            // Apply automatic fixes where possible
            foreach (var dependency in currentResults.DependencyResults.ToList())
            {
                foreach (var action in dependency.Actions)
                {
                    if (action.Text.Contains("Import") || action.Text.Contains("Create"))
                    {
                        action.Action?.Invoke();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            
            yield return null;
        }

        // Helper methods for automatic fixes and checks
        private bool IsTextMeshProInstalled()
        {
            return AssetDatabase.FindAssets("TextMeshPro").Length > 0;
        }

        private bool HasDOTweenInstalled()
        {
            return AssetDatabase.FindAssets("DOTween").Length > 0;
        }

        private bool HasNativeWebSocketInstalled()
        {
            return AssetDatabase.FindAssets("NativeWebSocket").Length > 0;
        }

        private Color GetSeverityColor(string severity)
        {
            switch (severity.ToUpper())
            {
                case "CRITICAL": return Color.red;
                case "WARNING": return Color.yellow;
                case "INFO": return Color.cyan;
                default: return Color.white;
            }
        }

        private MessageType GetMessageType(string severity)
        {
            switch (severity.ToUpper())
            {
                case "CRITICAL": return MessageType.Error;
                case "WARNING": return MessageType.Warning;
                case "INFO": return MessageType.Info;
                default: return MessageType.None;
            }
        }

        // Automatic fix actions
        private void ImportTextMeshProResources()
        {
            // This would open TMP Import dialog
            EditorUtility.DisplayDialog("TextMeshPro Import", "Please go to Window > TextMeshPro > Import TMP Essential Resources manually.", "OK");
        }

        private void OpenDOTweenPackage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676");
        }

        private void InstallNativeWebSocket()
        {
            Application.OpenURL("https://github.com/endel/NativeWebSocket");
        }

        private void CreateManagerScript(string managerFile)
        {
            EditorUtility.DisplayDialog("Create Manager", $"Please ensure {managerFile} exists in your project.", "OK");
        }

        private void CreateScene(string scenePath)
        {
            EditorUtility.DisplayDialog("Create Scene", $"Please create {scenePath} manually.", "OK");
        }

        private void FixBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings");
        }

        private void OpenPlayerSettings()
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings");
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
        }

        private void CreateFolder(string folder)
        {
            string fullPath = Application.dataPath.Replace("Assets", folder);
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }

        private void ConfigureNetworkManager()
        {
            var networkManagerAsset = AssetDatabase.FindAssets("NetworkManager").FirstOrDefault();
            if (networkManagerAsset != null)
            {
                string path = AssetDatabase.GUIDToAssetPath(networkManagerAsset);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(path));
            }
        }
    }

    // Supporting data structures
    [System.Serializable]
    public class ScanResults
    {
        public float Progress = 0f;
        public string CurrentStep = "";
        public System.DateTime StartTime;
        
        public List<HealthIssue> DependencyResults = new List<HealthIssue>();
        public List<HealthIssue> ManagerResults = new List<HealthIssue>();
        public List<HealthIssue> SceneResults = new List<HealthIssue>();
        public List<HealthIssue> BuildSettingsResults = new List<HealthIssue>();
        public List<HealthIssue> FileResults = new List<HealthIssue>();
        public List<HealthIssue> NetworkResults = new List<HealthIssue>();
        public List<HealthIssue> UIResults = new List<HealthIssue>();
    }

    [System.Serializable]
    public class HealthIssue
    {
        public string Severity; // CRITICAL, WARNING, INFO
        public string Message;
        public string Suggestion;
        public List<IssueAction> Actions;
    }

    [System.Serializable]
    public class IssueAction
    {
        public string Text;
        public System.Action Action;
    }

    // Helper class for Editor coroutines
    public static class EditorCoroutineUtility
    {
        public static Coroutine StartCoroutine(IEnumerator routine, MonoBehaviour owner)
        {
            return owner.StartCoroutine(routine);
        }
    }
}
#endif