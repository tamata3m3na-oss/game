using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEditor
{
    /// <summary>
    /// Utility to repair broken GUID references in Unity scene files.
    /// </summary>
    public class SceneRepairUtility : EditorWindow
    {
        private Vector2 scrollPosition;
        private string statusMessage = "";
        private List<SceneIssue> issues = new List<SceneIssue>();
        private bool isProcessing = false;

        private class SceneIssue
        {
            public enum Severity { Critical, Warning, Info }
            public Severity severity;
            public string sceneName;
            public string description;
            public string gameObjectName;
            public string brokenGuid;
            public string lineContent;
            public int lineNumber;
        }

        [MenuItem("Tools/Project Repair/Scene Repair Utility")]
        public static void ShowWindow()
        {
            GetWindow<SceneRepairUtility>("Scene Repair Utility");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Scenes and validates Unity scene files for broken GUID references.\n" +
                "This tool helps fix 'Missing Script' errors in scenes.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan All Scenes", GUILayout.Height(40)))
                {
                    ScanAllScenes();
                }

                if (GUILayout.Button("Auto-Fix Scene References", GUILayout.Height(40)))
                {
                    AutoFixSceneReferences();
                }

                if (GUILayout.Button("Validate Open Scene", GUILayout.Height(40)))
                {
                    ValidateOpenScene();
                }
            }

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(statusMessage))
            {
                MessageType msgType = statusMessage.Contains("Error") ? MessageType.Error :
                                     statusMessage.Contains("Warning") ? MessageType.Warning :
                                     MessageType.Info;
                EditorGUILayout.HelpBox(statusMessage, msgType);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Issues Found: {issues.Count}", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var groupedIssues = issues.GroupBy(i => i.sceneName);
            foreach (var group in groupedIssues)
            {
                EditorGUILayout.LabelField($"Scene: {group.Key}", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);

                foreach (var issue in group)
                {
                    Color bgColor = issue.severity == SceneIssue.Severity.Critical ? new Color(1, 0.8f, 0.8f, 1) :
                                   issue.severity == SceneIssue.Severity.Warning ? new Color(1, 1, 0.8f, 1) :
                                   Color.white;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUI.backgroundColor = bgColor;

                        EditorGUILayout.LabelField($"[{issue.severity}] {issue.description}", EditorStyles.boldLabel);
                        
                        if (!string.IsNullOrEmpty(issue.gameObjectName))
                        {
                            EditorGUILayout.LabelField($"GameObject: {issue.gameObjectName}", EditorStyles.wordWrappedLabel);
                        }
                        
                        if (!string.IsNullOrEmpty(issue.brokenGuid))
                        {
                            EditorGUILayout.LabelField($"Broken GUID: {issue.brokenGuid}", EditorStyles.miniLabel);
                        }
                        
                        if (!string.IsNullOrEmpty(issue.lineContent))
                        {
                            EditorGUILayout.TextArea(issue.lineContent, EditorStyles.miniTextArea, GUILayout.Height(60));
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Open Scene", GUILayout.Width(100)))
                            {
                                string scenePath = AssetDatabase.FindAssets($"t:Scene {group.Key}")
                                    .Select(g => AssetDatabase.GUIDToAssetPath(g))
                                    .FirstOrDefault(p => p.Contains(group.Key));
                                
                                if (!string.IsNullOrEmpty(scenePath))
                                {
                                    EditorSceneManager.OpenScene(scenePath);
                                }
                            }
                        }

                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ScanAllScenes()
        {
            if (isProcessing) return;

            issues.Clear();
            isProcessing = true;
            statusMessage = "Scanning all scenes...";
            Repaint();

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            Dictionary<string, string> scriptGuids = GetAllScriptGUIDs();

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                try
                {
                    string[] lines = File.ReadAllLines(scenePath);
                    ScanSceneFile(lines, sceneName, scenePath, scriptGuids);
                }
                catch (Exception e)
                {
                    issues.Add(new SceneIssue
                    {
                        severity = SceneIssue.Severity.Critical,
                        sceneName = sceneName,
                        description = $"Failed to read scene file: {e.Message}",
                        brokenGuid = "",
                        lineContent = "",
                        lineNumber = 0
                    });
                }
            }

            int criticalCount = issues.Count(i => i.severity == SceneIssue.Severity.Critical);
            int warningCount = issues.Count(i => i.severity == SceneIssue.Severity.Warning);
            statusMessage = $"Scan complete. Found {criticalCount} critical issues and {warningCount} warnings across {sceneGuids.Length} scenes.";
            isProcessing = false;
            Repaint();
        }

        private void ScanSceneFile(string[] lines, string sceneName, string scenePath, Dictionary<string, string> scriptGuids)
        {
            string currentGameObject = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Track current GameObject
                if (line.Contains("--- !u!1 &"))
                {
                    currentGameObject = ExtractGameObjectName(lines, i);
                }

                // Check for broken MonoBehaviour GUIDs
                if (line.Contains("m_Script: {fileID: 11500000, guid:"))
                {
                    Match guidMatch = Regex.Match(line, @"guid:\s*([a-f0-9]+)");
                    if (guidMatch.Success)
                    {
                        string guid = guidMatch.Groups[1].Value;
                        
                        if (IsBrokenGUID(guid))
                        {
                            string expectedScript = GuessScriptFromGUID(guid);
                            string actualGuid = FindScriptGUID(expectedScript, scriptGuids);

                            issues.Add(new SceneIssue
                            {
                                severity = SceneIssue.Severity.Critical,
                                sceneName = sceneName,
                                description = "Broken script reference",
                                gameObjectName = currentGameObject,
                                brokenGuid = guid,
                                lineContent = line.Trim(),
                                lineNumber = i + 1
                            });
                        }
                    }
                }

                // Check for GameObject references with broken GUIDs
                if (Regex.IsMatch(line, @"fileID:\s*\d+, guid:\s*0{16}[0-9a-f]{0,12}"))
                {
                    Match guidMatch = Regex.Match(line, @"guid:\s*([a-f0-9]+)");
                    if (guidMatch.Success)
                    {
                        string guid = guidMatch.Groups[1].Value;
                        
                        issues.Add(new SceneIssue
                        {
                            severity = SceneIssue.Severity.Warning,
                            sceneName = sceneName,
                            description = "Broken GameObject reference",
                            gameObjectName = currentGameObject,
                            brokenGuid = guid,
                            lineContent = line.Trim(),
                            lineNumber = i + 1
                        });
                    }
                }
            }
        }

        private void AutoFixSceneReferences()
        {
            if (isProcessing) return;
            if (issues.Count == 0)
            {
                statusMessage = "No issues to fix. Scan scenes first.";
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Auto-Fix",
                "This will modify scene files to fix broken script GUIDs.\n\n" +
                "Make sure you have a backup of your scenes.\n\nContinue?",
                "Yes, Fix All",
                "Cancel"))
            {
                return;
            }

            Dictionary<string, string> scriptGuids = GetAllScriptGUIDs();
            int fixedCount = 0;
            var scenesModified = new HashSet<string>();

            foreach (var issue in issues.Where(i => i.severity == SceneIssue.Severity.Critical))
            {
                if (string.IsNullOrEmpty(issue.brokenGuid) || issue.description != "Broken script reference")
                    continue;

                string expectedScript = GuessScriptFromGUID(issue.brokenGuid);
                string actualGuid = FindScriptGUID(expectedScript, scriptGuids);

                if (actualGuid != null)
                {
                    string scenePath = AssetDatabase.FindAssets($"t:Scene {issue.sceneName}")
                        .Select(g => AssetDatabase.GUIDToAssetPath(g))
                        .FirstOrDefault(p => p.Contains(issue.sceneName));

                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(scenePath);
                            bool modified = false;

                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].Contains($"guid: {issue.brokenGuid}") && 
                                    lines[i].Contains("m_Script:"))
                                {
                                    lines[i] = lines[i].Replace($"guid: {issue.brokenGuid}", $"guid: {actualGuid}");
                                    modified = true;
                                    fixedCount++;
                                    break;
                                }
                            }

                            if (modified)
                            {
                                File.WriteAllLines(scenePath, lines);
                                scenesModified.Add(scenePath);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[SceneRepair] Error fixing {scenePath}: {e.Message}");
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
            
            foreach (string scenePath in scenesModified)
            {
                AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);
            }

            statusMessage = $"Fixed {fixedCount} references in {scenesModified.Count} scene(s). Rescan recommended.";
            issues.Clear();
            isProcessing = false;
            Repaint();
        }

        private void ValidateOpenScene()
        {
            if (isProcessing) return;

            issues.Clear();
            Scene currentScene = SceneManager.GetActiveScene();
            
            if (string.IsNullOrEmpty(currentScene.name))
            {
                statusMessage = "No scene is currently open.";
                return;
            }

            isProcessing = true;
            statusMessage = $"Validating {currentScene.name}...";
            Repaint();

            // Check for GameObjects with missing scripts
            GameObject[] allObjects = currentScene.GetRootGameObjects();
            Dictionary<string, string> scriptGuids = GetAllScriptGUIDs();

            foreach (GameObject rootObj in allObjects)
            {
                CheckGameObjectForMissingScripts(rootObj, currentScene.name, scriptGuids);
            }

            int missingScriptCount = issues.Count;
            statusMessage = $"Found {missingScriptCount} GameObjects with missing scripts in {currentScene.name}.";
            isProcessing = false;
            Repaint();
        }

        private void CheckGameObjectForMissingScripts(GameObject obj, string sceneName, Dictionary<string, string> scriptGuids)
        {
            // Check all components
            Component[] components = obj.GetComponents<Component>();
            
            foreach (Component component in components)
            {
                if (component == null)
                {
                    issues.Add(new SceneIssue
                    {
                        severity = SceneIssue.Severity.Critical,
                        sceneName = sceneName,
                        description = "GameObject has missing script (component is null)",
                        gameObjectName = GetGameObjectPath(obj),
                        brokenGuid = "Unknown",
                        lineContent = "Component detected as null in scene",
                        lineNumber = 0
                    });
                }
            }

            // Recursively check children
            foreach (Transform child in obj.transform)
            {
                CheckGameObjectForMissingScripts(child.gameObject, sceneName, scriptGuids);
            }
        }

        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            
            while (current != null)
            {
                path = $"{current.name}/{path}";
                current = current.parent;
            }
            
            return path;
        }

        private Dictionary<string, string> GetAllScriptGUIDs()
        {
            var guidMap = new Dictionary<string, string>();
            
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string scriptName = Path.GetFileNameWithoutExtension(path);
                guidMap[scriptName] = guid;
            }
            
            return guidMap;
        }

        private string FindScriptGUID(string scriptName, Dictionary<string, string> guidMap)
        {
            if (guidMap.ContainsKey(scriptName))
                return guidMap[scriptName];
            
            // Try case-insensitive match
            foreach (var kvp in guidMap)
            {
                if (kvp.Key.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            
            return null;
        }

        private string GuessScriptFromGUID(string brokenGuid)
        {
            switch (brokenGuid.ToLower())
            {
                case "0000000000000001":
                    return "ShipController";
                case "0000000000000002":
                    return "WeaponController";
                case "0000000000000003":
                    return "AbilityController";
                case "0000000000000004":
                    return "Bullet";
                case "0000000000000005":
                    return "HealthBar";
                case "0000000000001000":
                    return "ThreadSafeEventQueue";
                case "0000000000001001":
                    return "AuthManager";
                case "0000000000001002":
                    return "NetworkManager";
                case "0000000000001003":
                    return "HttpNetworkManager";
                case "0000000000001004":
                    return "InputController";
                case "0000000000001005":
                    return "GameManager";
                case "0000000000001006":
                    return "SnapshotProcessor";
                case "0000000000001007":
                    return "GameTickManager";
                case "0000000000001008":
                    return "NetworkEventManager";
                case "0000000000001009":
                    return "GameStateManager";
                case "0000000000001010":
                    return "AnimationController";
                case "0000000000001011":
                    return "ParticleController";
                case "0000000000001012":
                    return "ObjectPool";
                case "0000000000001013":
                    return "TransitionManager";
                default:
                    return "Unknown";
            }
        }

        private bool IsBrokenGUID(string guid)
        {
            // Check for pattern: all zeros followed by some digits
            return Regex.IsMatch(guid, @"^0{16}[0-9a-f]{0,12}$");
        }

        private string ExtractGameObjectName(string[] lines, int startIndex)
        {
            for (int i = startIndex; i < Math.Min(startIndex + 20, lines.Length); i++)
            {
                if (lines[i].Contains("m_Name:"))
                {
                    Match match = Regex.Match(lines[i], @"m_Name:\s*(.+)");
                    if (match.Success)
                    {
                        return match.Groups[1].Value.Trim();
                    }
                }
            }
            return "";
        }
    }
}
