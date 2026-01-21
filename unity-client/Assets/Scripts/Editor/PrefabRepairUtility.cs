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
    /// Editor utility to repair broken prefab references and validate GUIDs.
    /// Automatically maps broken GUIDs to actual scripts in the project.
    /// </summary>
    public class PrefabRepairUtility : EditorWindow
    {
        private Vector2 scrollPosition;
        private string statusMessage = "";
        private bool isScanning = false;
        private List<RepairIssue> issues = new List<RepairIssue>();
        
        private class RepairIssue
        {
            public enum Severity { Critical, Warning, Info }
            public Severity severity;
            public string filePath;
            public string description;
            public string brokenGuid;
            public string suggestedFix;
            public bool canAutoFix;
        }

        [MenuItem("Tools/Project Repair/Prefab Repair Utility")]
        public static void ShowWindow()
        {
            GetWindow<PrefabRepairUtility>("Prefab Repair Utility");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "This tool scans for broken GUID references in prefabs and helps repair them.",
                MessageType.Info
            );

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan All Prefabs", GUILayout.Height(40)))
                {
                    ScanForBrokenReferences();
                }
                
                if (GUILayout.Button("Auto-Fix All", GUILayout.Height(40)))
                {
                    AutoFixAllIssues();
                }
                
                if (GUILayout.Button("Refresh Script GUIDs", GUILayout.Height(40)))
                {
                    RefreshScriptGUIDs();
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

            foreach (var issue in issues)
            {
                Color bgColor = issue.severity == RepairIssue.Severity.Critical ? new Color(1, 0.8f, 0.8f, 1) :
                               issue.severity == RepairIssue.Severity.Warning ? new Color(1, 1, 0.8f, 1) :
                               Color.white;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUI.backgroundColor = bgColor;
                    
                    EditorGUILayout.LabelField($"[{issue.severity}] {Path.GetFileName(issue.filePath)}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(issue.description, EditorStyles.wordWrappedLabel);
                    
                    if (!string.IsNullOrEmpty(issue.brokenGuid))
                    {
                        EditorGUILayout.LabelField($"Broken GUID: {issue.brokenGuid}", EditorStyles.miniLabel);
                    }
                    
                    if (!string.IsNullOrEmpty(issue.suggestedFix))
                    {
                        EditorGUILayout.LabelField($"Suggested Fix: {issue.suggestedFix}", EditorStyles.wordWrappedLabel);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Select File", GUILayout.Width(100)))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.filePath);
                        }
                    }
                    
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ScanForBrokenReferences()
        {
            issues.Clear();
            statusMessage = "Scanning project...";
            isScanning = true;
            
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            Dictionary<string, string> scriptGuids = GetAllScriptGUIDs();

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ImpactParticles")) continue; // Skip deleted prefab

                string[] lines = File.ReadAllLines(path);
                bool hasBrokenReference = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    
                    // Check for broken GUID patterns
                    var brokenGuids = Regex.Matches(line, @"guid: (00000000[a-f0-9]{0,24})");
                    
                    foreach (Match match in brokenGuids)
                    {
                        string brokenGuid = match.Groups[1].Value;
                        hasBrokenReference = true;
                        
                        string scriptName = GuessScriptNameFromGUID(brokenGuid);
                        string actualGuid = FindScriptGUID(scriptName, scriptGuids);
                        
                        var issue = new RepairIssue
                        {
                            severity = RepairIssue.Severity.Critical,
                            filePath = path,
                            description = $"Line {i + 1}: Broken script reference",
                            brokenGuid = brokenGuid,
                            suggestedFix = actualGuid != null ? $"Replace with actual GUID: {actualGuid}" : $"Script '{scriptName}' not found in project",
                            canAutoFix = actualGuid != null
                        };
                        
                        issues.Add(issue);
                    }
                    
                    // Check for default Unity GUID issues
                    if (line.Contains("guid: 0000000000000000f000000000000000") && !line.Contains("m_Sprite"))
                    {
                        var issue = new RepairIssue
                        {
                            severity = RepairIssue.Severity.Warning,
                            filePath = path,
                            description = $"Line {i + 1}: Default Unity GUID reference (likely missing sprite)",
                            brokenGuid = "0000000000000000f000000000000000",
                            suggestedFix = "Assign proper sprite in Unity Editor",
                            canAutoFix = false
                        };
                        
                        issues.Add(issue);
                    }
                }

                if (hasBrokenReference)
                {
                    // Add summary issue for this prefab
                    var summaryIssue = new RepairIssue
                    {
                        severity = RepairIssue.Severity.Critical,
                        filePath = path,
                        description = $"This prefab has {issues.Count(i => i.filePath == path)} broken references",
                        brokenGuid = "",
                        suggestedFix = "Use Auto-Fix or manually assign scripts in Unity Editor",
                        canAutoFix = true
                    };
                    issues.Add(summaryIssue);
                }
            }

            // Check for missing .meta files on scripts
            CheckForMissingScriptMetas();

            int criticalCount = issues.Count(i => i.severity == RepairIssue.Severity.Critical);
            int warningCount = issues.Count(i => i.severity == RepairIssue.Severity.Warning);
            statusMessage = $"Scan complete. Found {criticalCount} critical issues and {warningCount} warnings.";
            isScanning = false;
            Repaint();
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

        private string GuessScriptNameFromGUID(string brokenGuid)
        {
            // Map broken GUIDs to likely script names
            // Based on patterns found in the prefabs
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
                    return "HealthBar"; // Likely custom component
                case "0000000000001000":
                    return "ThreadSafeEventQueue";
                case "0000000000001001":
                    return "AuthManager";
                case "0000000000001002":
                    return "NetworkManager";
                case "0000000000001003":
                    return "HttpNetworkManager"; // or similar
                case "0000000000001004":
                    return "InputController";
                case "0000000000001005":
                    return "GameManager";
                case "0000000000001006":
                    return "SnapshotProcessor"; // or similar
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

        private void CheckForMissingScriptMetas()
        {
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            foreach (string guid in scriptGuids)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                string metaPath = scriptPath + ".meta";
                
                if (!File.Exists(metaPath))
                {
                    var issue = new RepairIssue
                    {
                        severity = RepairIssue.Severity.Warning,
                        filePath = scriptPath,
                        description = "Missing .meta file - Unity will regenerate it",
                        brokenGuid = "No meta file",
                        suggestedFix = "Refresh Script GUIDs to generate .meta files",
                        canAutoFix = true
                    };
                    issues.Add(issue);
                }
            }
        }

        private void AutoFixAllIssues()
        {
            if (issues.Count == 0)
            {
                statusMessage = "No issues to fix.";
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Auto-Fix",
                "This will modify multiple prefab files. Make sure you have a backup.\n\nContinue?",
                "Yes, Fix All",
                "Cancel"))
            {
                return;
            }

            Dictionary<string, string> scriptGuids = GetAllScriptGUIDs();
            int fixedCount = 0;

            foreach (var issue in issues.Where(i => i.canAutoFix && i.severity == RepairIssue.Severity.Critical))
            {
                if (string.IsNullOrEmpty(issue.brokenGuid) || issue.description.Contains("broken references"))
                    continue; // Skip summary issues

                string scriptName = GuessScriptNameFromGUID(issue.brokenGuid);
                string actualGuid = FindScriptGUID(scriptName, scriptGuids);

                if (actualGuid != null)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(issue.filePath);
                        bool modified = false;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Contains($"guid: {issue.brokenGuid}"))
                            {
                                lines[i] = lines[i].Replace($"guid: {issue.brokenGuid}", $"guid: {actualGuid}");
                                modified = true;
                                fixedCount++;
                            }
                        }

                        if (modified)
                        {
                            File.WriteAllLines(issue.filePath, lines);
                            Debug.Log($"[PrefabRepair] Fixed {issue.filePath}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[PrefabRepair] Error fixing {issue.filePath}: {e.Message}");
                    }
                }
            }

            AssetDatabase.Refresh();
            statusMessage = $"Auto-fixed {fixedCount} references. Rescan recommended.";
            issues.Clear();
            Repaint();
        }

        private void RefreshScriptGUIDs()
        {
            if (!EditorUtility.DisplayDialog("Confirm Refresh",
                "This will regenerate .meta files for all scripts.\n\nContinue?",
                "Yes, Refresh",
                "Cancel"))
            {
                return;
            }

            AssetDatabase.Refresh();
            
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            statusMessage = "Script GUIDs refreshed. Rescan prefabs to verify.";
            Repaint();
        }
    }
}
