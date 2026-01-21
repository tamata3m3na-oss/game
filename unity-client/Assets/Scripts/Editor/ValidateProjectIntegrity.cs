using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditor
{
    /// <summary>
    /// Comprehensive project integrity validator that checks for common Unity project issues.
    /// </summary>
    public class ValidateProjectIntegrity : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationIssue> issues = new List<ValidationIssue>();
        private bool isValidating = false;

        private class ValidationIssue
        {
            public enum IssueType { Critical, Warning, Info, Success }
            public IssueType type;
            public string category;
            public string message;
            public string filePath;
            public string suggestion;
        }

        [MenuItem("Tools/Project Repair/Validate Project Integrity")]
        public static void ShowWindow()
        {
            GetWindow<ValidateProjectIntegrity>("Project Integrity");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Validates project integrity and checks for common issues.\n" +
                "Run this after making significant changes to the project.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Full Validation", GUILayout.Height(40)))
                {
                    RunValidation();
                }

                if (GUILayout.Button("Export Report", GUILayout.Height(40)))
                {
                    ExportReport();
                }
            }

            EditorGUILayout.Space();

            if (isValidating)
            {
                EditorGUILayout.HelpBox("Validating...", MessageType.Info);
                Repaint();
                return;
            }

            DisplayResults();
        }

        private void DisplayResults()
        {
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation data available. Click 'Run Full Validation' to begin.", MessageType.Info);
                return;
            }

            int criticalCount = issues.Count(i => i.type == ValidationIssue.IssueType.Critical);
            int warningCount = issues.Count(i => i.type == ValidationIssue.IssueType.Warning);
            int infoCount = issues.Count(i => i.type == ValidationIssue.IssueType.Info);
            int successCount = issues.Count(i => i.type == ValidationIssue.IssueType.Success);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = Color.red;
                EditorGUILayout.LabelField($"Critical: {criticalCount}", EditorStyles.boldLabel, GUILayout.Width(100));
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.LabelField($"Warnings: {warningCount}", EditorStyles.boldLabel, GUILayout.Width(100));
                GUI.backgroundColor = Color.white;
                EditorGUILayout.LabelField($"Info: {infoCount}", EditorStyles.boldLabel, GUILayout.Width(100));
                GUI.backgroundColor = Color.green;
                EditorGUILayout.LabelField($"Passed: {successCount}", EditorStyles.boldLabel, GUILayout.Width(100));
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var groupedIssues = issues.GroupBy(i => i.category);
            foreach (var group in groupedIssues)
            {
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);

                foreach (var issue in group)
                {
                    MessageType msgType = issue.type == ValidationIssue.IssueType.Critical ? MessageType.Error :
                                         issue.type == ValidationIssue.IssueType.Warning ? MessageType.Warning :
                                         issue.type == ValidationIssue.IssueType.Info ? MessageType.Info :
                                         MessageType.None;

                    if (issue.type == ValidationIssue.IssueType.Success)
                    {
                        GUI.backgroundColor = new Color(0.8f, 1, 0.8f, 1);
                    }

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(issue.message, EditorStyles.wordWrappedLabel);

                        if (!string.IsNullOrEmpty(issue.filePath))
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField($"File: {issue.filePath}", EditorStyles.miniLabel);
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Select", GUILayout.Width(60)))
                                {
                                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.filePath);
                                    if (obj != null) Selection.activeObject = obj;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(issue.suggestion))
                        {
                            EditorGUILayout.LabelField($"Suggestion: {issue.suggestion}", EditorStyles.wordWrappedLabel);
                        }

                        if (msgType != MessageType.None)
                        {
                            EditorGUILayout.HelpBox(issue.type.ToString(), msgType);
                        }
                    }

                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunValidation()
        {
            issues.Clear();
            isValidating = true;
            Repaint();

            CheckAssemblyDefinitions();
            CheckBrokenPrefabReferences();
            CheckMissingScripts();
            CheckScenes();
            CheckScriptConsistency();

            isValidating = false;
            Repaint();
        }

        private void CheckAssemblyDefinitions()
        {
            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Success,
                category = "Assembly Definitions",
                message = "Checking assembly definition files...",
                filePath = "",
                suggestion = ""
            });

            string[] asmdefGuids = AssetDatabase.FindAssets("t:asmdef");
            HashSet<string> asmdefNames = new HashSet<string>();

            foreach (string guid in asmdefGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string content = File.ReadAllText(path);

                // Extract assembly name
                var nameMatch = System.Text.RegularExpressions.Regex.Match(content, "\"name\":\s*\"([^\"]+)\"");
                if (nameMatch.Success)
                {
                    string asmName = nameMatch.Groups[1].Value;
                    
                    // Check for reserved names
                    if (asmName.StartsWith("Assembly-"))
                    {
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Critical,
                            category = "Assembly Definitions",
                            message = $"Assembly definition uses reserved name '{asmName}'",
                            filePath = path,
                            suggestion = "Rename to a non-reserved name like 'GameProject' or 'MyGameAssembly'"
                        });
                    }
                    else
                    {
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Success,
                            category = "Assembly Definitions",
                            message = $"Assembly '{asmName}' uses valid name",
                            filePath = path,
                            suggestion = ""
                        });
                    }

                    asmdefNames.Add(asmName);
                }
            }

            if (asmdefGuids.Length == 0)
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Warning,
                    category = "Assembly Definitions",
                    message = "No custom assembly definitions found",
                    filePath = "",
                    suggestion = "Consider creating separate assemblies for better organization and compilation performance"
                });
            }
        }

        private void CheckBrokenPrefabReferences()
        {
            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Success,
                category = "Prefab References",
                message = "Scanning prefabs for broken references...",
                filePath = "",
                suggestion = ""
            });

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int brokenCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                try
                {
                    string content = File.ReadAllText(path);
                    
                    // Check for broken GUID patterns
                    if (System.Text.RegularExpressions.Regex.IsMatch(content, @"guid:\s*0{16}[0-9a-f]{0,12}"))
                    {
                        brokenCount++;
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Critical,
                            category = "Prefab References",
                            message = "Prefab contains broken script references",
                            filePath = path,
                            suggestion = "Use 'Remove Broken References' tool or manually reassign scripts in Unity Editor"
                        });
                    }

                    // Check for default Unity sprite GUIDs
                    if (content.Contains("guid: 0000000000000000f000000000000000"))
                    {
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Warning,
                            category = "Prefab References",
                            message = "Prefab references Unity's default sprite (Knob)",
                            filePath = path,
                            suggestion = "Assign proper sprite asset in Unity Editor"
                        });
                    }
                }
                catch (Exception e)
                {
                    issues.Add(new ValidationIssue
                    {
                        type = ValidationIssue.IssueType.Critical,
                        category = "Prefab References",
                        message = $"Failed to read prefab: {e.Message}",
                        filePath = path,
                        suggestion = "File may be corrupted, consider recreating the prefab"
                    });
                }
            }

            if (brokenCount == 0)
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Success,
                    category = "Prefab References",
                    message = $"All {prefabGuids.Length} prefabs have valid references",
                    filePath = "",
                    suggestion = ""
                });
            }
        }

        private void CheckMissingScripts()
        {
            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Success,
                category = "Script Files",
                message = "Checking script files and .meta files...",
                filePath = "",
                suggestion = ""
            });

            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            int missingMetaCount = 0;

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string metaPath = path + ".meta";

                if (!File.Exists(metaPath))
                {
                    missingMetaCount++;
                    issues.Add(new ValidationIssue
                    {
                        type = ValidationIssue.IssueType.Warning,
                        category = "Script Files",
                        message = "Script is missing .meta file",
                        filePath = path,
                        suggestion = "Unity will regenerate it, but GUIDs may change. Use AssetDatabase.Refresh()"
                    });
                }
            }

            if (missingMetaCount == 0)
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Success,
                    category = "Script Files",
                    message = $"All {scriptGuids.Length} scripts have .meta files",
                    filePath = "",
                    suggestion = ""
                });
            }
        }

        private void CheckScenes()
        {
            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Success,
                category = "Scene Files",
                message = "Validating scene files...",
                filePath = "",
                suggestion = ""
            });

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                try
                {
                    string content = File.ReadAllText(path);

                    // Check for broken GUIDs in scene
                    if (System.Text.RegularExpressions.Regex.IsMatch(content, @"guid:\s*0{16}[0-9a-f]{0,12}"))
                    {
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Critical,
                            category = "Scene Files",
                            message = "Scene contains broken script references",
                            filePath = path,
                            suggestion = "Open scene in Unity Editor to see missing component warnings"
                        });
                    }
                    else
                    {
                        issues.Add(new ValidationIssue
                        {
                            type = ValidationIssue.IssueType.Success,
                            category = "Scene Files",
                            message = $"Scene '{Path.GetFileNameWithoutExtension(path)}' is valid",
                            filePath = path,
                            suggestion = ""
                        });
                    }
                }
                catch (Exception e)
                {
                    issues.Add(new ValidationIssue
                    {
                        type = ValidationIssue.IssueType.Critical,
                        category = "Scene Files",
                        message = $"Failed to read scene: {e.Message}",
                        filePath = path,
                        suggestion = "File may be corrupted"
                    });
                }
            }

            if (sceneGuids.Length == 0)
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Warning,
                    category = "Scene Files",
                    message = "No scenes found in project",
                    filePath = "",
                    suggestion = "Create at least one scene to build the game"
                });
            }
        }

        private void CheckScriptConsistency()
        {
            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Success,
                category = "Script Consistency",
                message = "Checking for compilation errors...",
                filePath = "",
                suggestion = ""
            });

            // Check if there are any compilation errors
            if (EditorCompilation.Compilation.CompilationTasks.Count > 0)
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Warning,
                    category = "Script Consistency",
                    message = "Scripts are currently compiling",
                    filePath = "",
                    suggestion = "Wait for compilation to complete before validating"
                });
            }
            else
            {
                issues.Add(new ValidationIssue
                {
                    type = ValidationIssue.IssueType.Success,
                    category = "Script Consistency",
                    message = "No active compilation tasks",
                    filePath = "",
                    suggestion = ""
                });
            }

            // Check for common namespaces
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            var namespaces = new HashSet<string>();

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    string content = File.ReadAllText(path);
                    var matches = System.Text.RegularExpressions.Regex.Matches(content, @"namespace\s+([^\s{]+)");
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        namespaces.Add(match.Groups[1].Value);
                    }
                }
                catch { }
            }

            issues.Add(new ValidationIssue
            {
                type = ValidationIssue.IssueType.Info,
                category = "Script Consistency",
                message = $"Found {namespaces.Count} namespaces in project",
                filePath = "",
                suggestion = "Consistent namespace organization improves code maintainability"
            });
        }

        private void ExportReport()
        {
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Report", "No validation data to export.", "OK");
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = EditorUtility.SaveFilePanel(
                "Save Validation Report",
                "",
                $"ValidationReport_{timestamp}.txt",
                "txt"
            );

            if (string.IsNullOrEmpty(filePath))
                return;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Unity Project Integrity Validation Report");
                writer.WriteLine($"Generated: {DateTime.Now}");
                writer.WriteLine(new string('=', 60));
                writer.WriteLine();

                int criticalCount = issues.Count(i => i.type == ValidationIssue.IssueType.Critical);
                int warningCount = issues.Count(i => i.type == ValidationIssue.IssueType.Warning);
                int infoCount = issues.Count(i => i.type == ValidationIssue.IssueType.Info);

                writer.WriteLine($"Summary: {criticalCount} Critical, {warningCount} Warnings, {infoCount} Info");
                writer.WriteLine();

                var groupedIssues = issues.GroupBy(i => i.category);
                foreach (var group in groupedIssues)
                {
                    writer.WriteLine(group.Key);
                    writer.WriteLine(new string('-', 40));

                    foreach (var issue in group)
                    {
                        writer.WriteLine($"[{issue.type}] {issue.message}");
                        if (!string.IsNullOrEmpty(issue.filePath))
                        {
                            writer.WriteLine($"  File: {issue.filePath}");
                        }
                        if (!string.IsNullOrEmpty(issue.suggestion))
                        {
                            writer.WriteLine($"  Suggestion: {issue.suggestion}");
                        }
                        writer.WriteLine();
                    }
                }
            }

            EditorUtility.DisplayDialog("Export Complete", $"Report saved to:\n{filePath}", "OK");
        }
    }
}
