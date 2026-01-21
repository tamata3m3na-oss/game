using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEditor
{
    /// <summary>
    /// Utility to remove broken references from prefabs by removing components
    /// that reference missing scripts.
    /// </summary>
    public class RemoveBrokenReferences : EditorWindow
    {
        private Vector2 scrollPosition;
        private string statusLog = "";
        private bool isProcessing = false;

        [MenuItem("Tools/Project Repair/Remove Broken References")]
        public static void ShowWindow()
        {
            GetWindow<RemoveBrokenReferences>("Remove Broken References");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "This tool removes components with broken script references from prefabs.\n" +
                "Use this when a prefab has 'The associated script can not be loaded' errors.",
                MessageType.Warning
            );

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan & Remove Broken Components", GUILayout.Height(40)))
                {
                    ScanAndRemoveBrokenComponents();
                }

                if (GUILayout.Button("Clean Selected Prefab", GUILayout.Height(40)))
                {
                    CleanSelectedPrefab();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status Log:", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(statusLog, GUILayout.Height(300), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
        }

        private void ScanAndRemoveBrokenComponents()
        {
            if (isProcessing) return;
            
            isProcessing = true;
            statusLog = "Scanning all prefabs for broken references...\n\n";
            Repaint();

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int totalProcessed = 0;
            int totalFixed = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ImpactParticles")) continue; // Skip deleted prefab

                int componentsRemoved = RemoveBrokenComponentsFromPrefab(path);
                
                if (componentsRemoved > 0)
                {
                    totalFixed++;
                    statusLog += $"[FIXED] {path} - Removed {componentsRemoved} broken components\n";
                }
                
                totalProcessed++;
            }

            statusLog += $"\n=== Summary ===\n";
            statusLog += $"Total prefabs scanned: {totalProcessed}\n";
            statusLog += $"Prefabs with fixes: {totalFixed}\n";

            AssetDatabase.Refresh();
            isProcessing = false;
            Repaint();

            EditorUtility.DisplayDialog("Complete", 
                $"Scanned {totalProcessed} prefabs.\nFixed {totalFixed} prefabs.", 
                "OK");
        }

        private void CleanSelectedPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                statusLog = "Please select a prefab in the Project window first.\n";
                Repaint();
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
            {
                statusLog = "Selected object is not a prefab.\n";
                Repaint();
                return;
            }

            statusLog = $"Cleaning {path}...\n\n";
            Repaint();

            int removed = RemoveBrokenComponentsFromPrefab(path);

            statusLog += removed > 0 
                ? $"Removed {removed} broken components.\n" 
                : "No broken components found.\n";

            AssetDatabase.Refresh();
            Repaint();
        }

        private int RemoveBrokenComponentsFromPrefab(string prefabPath)
        {
            try
            {
                string[] lines = File.ReadAllLines(prefabPath);
                var outputLines = new List<string>(lines);
                int removedCount = 0;

                // Track which GameObject IDs have broken MonoBehaviour components
                var gameObjectsWithBrokenComponents = new HashSet<string>();
                
                // First pass: identify all GameObjects with broken MonoBehaviour references
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    
                    // Check for MonoBehaviour with broken GUID
                    if (line.Trim().StartsWith("--- !u!114 &"))
                    {
                        string componentId = ExtractComponentId(line);
                        
                        // Look ahead to find the Script reference
                        for (int j = i; j < Math.Min(i + 20, lines.Length); j++)
                        {
                            if (lines[j].Contains("m_Script:"))
                            {
                                string scriptLine = lines[j];
                                
                                // Check if GUID is broken (all zeros or invalid pattern)
                                if (HasBrokenGUID(scriptLine))
                                {
                                    // Find which GameObject this component belongs to
                                    string gameObjectId = FindGameObjectId(lines, i);
                                    if (!string.IsNullOrEmpty(gameObjectId))
                                    {
                                        gameObjectsWithBrokenComponents.Add(gameObjectId);
                                    }
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }

                if (gameObjectsWithBrokenComponents.Count == 0)
                {
                    return 0;
                }

                // Second pass: remove the broken components
                var linesToRemove = new HashSet<int>();
                
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    
                    if (line.Trim().StartsWith("--- !u!114 &"))
                    {
                        string componentId = ExtractComponentId(line);
                        
                        // Check if this component has a broken script
                        bool isBroken = false;
                        for (int j = i; j < Math.Min(i + 20, lines.Length); j++)
                        {
                            if (lines[j].Contains("m_Script:"))
                            {
                                isBroken = HasBrokenGUID(lines[j]);
                                break;
                            }
                            // Stop if we hit the next component or object
                            if (lines[j].Trim().StartsWith("--- !u!") && j > i)
                            {
                                break;
                            }
                        }
                        
                        if (isBroken)
                        {
                            // Mark all lines until next component/GameObject for removal
                            for (int j = i; j < lines.Length; j++)
                            {
                                linesToRemove.Add(j);
                                if (lines[j].Trim().StartsWith("--- !u!") && j > i)
                                {
                                    linesToRemove.Remove(j); // Don't remove the next component header
                                    break;
                                }
                            }
                            
                            // Also remove from GameObject's component list
                            for (int k = 0; k < lines.Length; k++)
                            {
                                if (lines[k].Contains("- component:") && k > 0)
                                {
                                    // Check if this references our component
                                    for (int m = k - 1; m < Math.Max(0, k - 5); m--)
                                    {
                                        if (lines[m].Contains("m_Component:"))
                                        {
                                            if (lines[k].Contains(componentId))
                                            {
                                                linesToRemove.Add(k);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            removedCount++;
                        }
                    }
                }

                // Build cleaned file
                var cleanedLines = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!linesToRemove.Contains(i))
                    {
                        cleanedLines.Add(lines[i]);
                    }
                }

                if (cleanedLines.Count < lines.Length)
                {
                    File.WriteAllLines(prefabPath, cleanedLines);
                    return removedCount;
                }
            }
            catch (Exception e)
            {
                statusLog += $"Error processing {prefabPath}: {e.Message}\n";
            }

            return 0;
        }

        private string ExtractComponentId(string line)
        {
            // Extract component ID from "--- !u!114 &1234567890"
            Match match = Regex.Match(line, @"--- !u!114 &(\d+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string FindGameObjectId(string[] lines, int startIndex)
        {
            // Search backwards from component to find the GameObject
            for (int i = startIndex; i >= 0; i--)
            {
                if (lines[i].Contains("m_GameObject:") && lines[i].Contains("{fileID:"))
                {
                    Match match = Regex.Match(lines[i], @"m_GameObject: \{fileID: (\d+)\}");
                    return match.Success ? match.Groups[1].Value : "";
                }
            }
            return "";
        }

        private bool HasBrokenGUID(string scriptLine)
        {
            // Check for broken GUID patterns
            // Pattern 1: All zeros with varying lengths (e.g., 0000000000000001, 0000000000000004)
            if (Regex.IsMatch(scriptLine, @"guid: 0{16}[0-9a-f]{0,12}"))
            {
                return true;
            }
            
            // Pattern 2: Classic Unity missing script GUID
            if (scriptLine.Contains("guid: 0000000000000000f000000000000000"))
            {
                return true;
            }
            
            return false;
        }
    }
}
