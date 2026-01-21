using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditor
{
    /// <summary>
    /// Forces a complete reimport of all assets in the project.
    /// Use this to fix corruption, regenerate .meta files, or resolve GUID conflicts.
    /// </summary>
    public class ForceFullReimport : EditorWindow
    {
        private static ForceFullReimport window;
        private bool isReimporting = false;
        private string currentAsset = "";
        private int processedCount = 0;
        private int totalCount = 0;
        private float progress = 0f;
        private bool reimportLibrary = false;
        private bool clearCache = false;

        [MenuItem("Tools/Project Repair/Force Full Reimport")]
        public static void ShowWindow()
        {
            window = GetWindow<ForceFullReimport>("Force Full Reimport");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "⚠️ WARNING: This will force Unity to reimport ALL assets.\n\n" +
                "This can take significant time depending on project size.\n" +
                "Use this when:\n" +
                "- Assets are corrupted\n" +
                "- .meta files are missing or invalid\n" +
                "- GUID references are broken\n" +
                "- AssetDatabase needs to be rebuilt\n\n" +
                "RECOMMENDATION: Backup your project before proceeding!",
                MessageType.Warning
            );

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                reimportLibrary = EditorGUILayout.ToggleLeft("Reimport Library Assets (slower)", reimportLibrary);
                clearCache = EditorGUILayout.ToggleLeft("Clear Cache After Reimport", clearCache);
            }

            EditorGUILayout.Space();

            if (isReimporting)
            {
                // Progress view
                EditorGUILayout.HelpBox($"Reimporting... Please wait.", MessageType.Info);
                
                Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(progressRect, progress, $"Progress: {processedCount}/{totalCount}");
                
                EditorGUILayout.LabelField($"Current: {currentAsset}", EditorStyles.wordWrappedLabel);
                
                Repaint();
            }
            else
            {
                // Normal view
                if (GUILayout.Button("Start Full Reimport", GUILayout.Height(50)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm Full Reimport",
                        "This will reimport ALL assets in the project.\n\n" +
                        "This operation cannot be undone.\n\n" +
                        "Are you sure you want to continue?",
                        "Yes, Start Reimport",
                        "Cancel"))
                    {
                        StartReimport();
                    }
                }

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reimport All Scripts"))
                    {
                        ReimportScripts();
                    }

                    if (GUILayout.Button("Reimport All Prefabs"))
                    {
                        ReimportPrefabs();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reimport All Scenes"))
                    {
                        ReimportScenes();
                    }

                    if (GUILayout.Button("Regenerate Meta Files"))
                    {
                        RegenerateMetaFiles();
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Quick Refresh Only"))
                {
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Refresh Complete", 
                        "AssetDatabase has been refreshed.\n\n" +
                        "This updates Unity's internal asset database but doesn't force reimports.", 
                        "OK");
                }
            }
        }

        private void StartReimport()
        {
            if (isReimporting) return;

            isReimporting = true;
            processedCount = 0;
            totalCount = 0;
            progress = 0f;
            currentAsset = "Preparing...";

            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (clearCache)
                    {
                        EditorUtility.DisplayProgressBar("Clearing Cache", "Clearing Unity cache...", 0.1f);
                        ClearUnityCache();
                    }

                    // Collect all assets
                    List<string> assetPaths = new List<string>();
                    
                    if (reimportLibrary)
                    {
                        // Include everything
                        string[] allGuids = AssetDatabase.FindAssets("");
                        foreach (string guid in allGuids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            if (!string.IsNullOrEmpty(path) && !path.StartsWith("Packages/"))
                            {
                                assetPaths.Add(path);
                            }
                        }
                    }
                    else
                    {
                        // Exclude Library folder
                        string projectPath = Directory.GetCurrentDirectory();
                        var assetsDir = new DirectoryInfo(Path.Combine(projectPath, "Assets"));
                        
                        if (assetsDir.Exists)
                        {
                            foreach (var file in assetsDir.GetFiles("*", SearchOption.AllDirectories))
                            {
                                string relativePath = "Assets" + file.FullName.Substring(assetsDir.FullName.Length);
                                if (!relativePath.EndsWith(".meta") && !relativePath.EndsWith(".cs"))
                                {
                                    assetPaths.Add(relativePath);
                                }
                            }
                        }
                    }

                    totalCount = assetPaths.Count;

                    // Reimport in batches
                    ReimportAssetsBatch(assetPaths, 0, 50);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ForceFullReimport] Error: {e.Message}");
                    isReimporting = false;
                    EditorUtility.ClearProgressBar();
                }
            };
        }

        private void ReimportAssetsBatch(List<string> assetPaths, int startIndex, int batchSize)
        {
            if (startIndex >= assetPaths.Count)
            {
                // Done
                isReimporting = false;
                progress = 1f;
                currentAsset = "Complete!";
                
                EditorUtility.ClearProgressBar();
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Reimport Complete", 
                    $"Successfully reimported {processedCount} assets.\n\n" +
                    "If you still see errors, try restarting Unity.", 
                    "OK");
                
                Repaint();
                return;
            }

            int endIndex = Mathf.Min(startIndex + batchSize, assetPaths.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                currentAsset = assetPaths[i];
                progress = (float)i / totalCount;
                
                try
                {
                    AssetDatabase.ImportAsset(assetPaths[i], ImportAssetOptions.ForceUpdate);
                    processedCount++;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ForceFullReimport] Failed to reimport {assetPaths[i]}: {e.Message}");
                }
            }

            Repaint();

            // Schedule next batch
            EditorApplication.delayCall += () =>
            {
                ReimportAssetsBatch(assetPaths, endIndex, batchSize);
            };
        }

        private void ReimportScripts()
        {
            if (!EditorUtility.DisplayDialog("Reimport All Scripts",
                "This will reimport all .cs script files in the project.\n\nContinue?",
                "Yes", "Cancel"))
            {
                return;
            }

            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            int count = 0;

            EditorUtility.DisplayProgressBar("Reimporting Scripts", "Reimporting scripts...", 0f);

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Reimporting Scripts", 
                    path, 
                    (float)count / scriptGuids.Length))
                {
                    break;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                
                // Also ensure .meta file exists
                string metaPath = path + ".meta";
                if (!File.Exists(metaPath))
                {
                    // Let Unity regenerate it
                    File.WriteAllText(metaPath, "fileFormatVersion: 2\nguid: " + guid + "\n");
                }

                count++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Complete", 
                $"Reimported {count} scripts.", 
                "OK");
        }

        private void ReimportPrefabs()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int count = 0;

            EditorUtility.DisplayProgressBar("Reimporting Prefabs", "Reimporting prefabs...", 0f);

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Reimporting Prefabs", 
                    path, 
                    (float)count / prefabGuids.Length))
                {
                    break;
                }

                try
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to reimport {path}: {e.Message}");
                }

                count++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Complete", 
                $"Reimported {count} prefabs.", 
                "OK");
        }

        private void ReimportScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            int count = 0;

            EditorUtility.DisplayProgressBar("Reimporting Scenes", "Reimporting scenes...", 0f);

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Reimporting Scenes", 
                    path, 
                    (float)count / sceneGuids.Length))
                {
                    break;
                }

                try
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to reimport {path}: {e.Message}");
                }

                count++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Complete", 
                $"Reimported {count} scenes.", 
                "OK");
        }

        private void RegenerateMetaFiles()
        {
            if (!EditorUtility.DisplayDialog("Regenerate Meta Files",
                "This will delete all .meta files and force Unity to regenerate them.\n\n" +
                "WARNING: All custom GUIDs will be lost and reassigned.\n\n" +
                "Continue?",
                "Yes, Regenerate", 
                "Cancel"))
            {
                return;
            }

            int deletedCount = 0;

            EditorUtility.DisplayProgressBar("Regenerating Meta Files", "Deleting old .meta files...", 0f);

            string projectPath = Directory.GetCurrentDirectory();
            var assetsDir = new DirectoryInfo(Path.Combine(projectPath, "Assets"));
            
            if (assetsDir.Exists)
            {
                var metaFiles = assetsDir.GetFiles("*.meta", SearchOption.AllDirectories);
                
                for (int i = 0; i < metaFiles.Length; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Deleting Meta Files", 
                        metaFiles[i].Name, 
                        (float)i / metaFiles.Length))
                    {
                        break;
                    }

                    try
                    {
                        metaFiles[i].Delete();
                        deletedCount++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to delete {metaFiles[i].FullName}: {e.Message}");
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorUtility.DisplayDialog("Meta Files Regenerated", 
                $"Deleted {deletedCount} .meta files.\n\n" +
                "Unity will regenerate them automatically.\n" +
                "Note: This may take a while and all GUIDs have changed.\n\n" +
                "You may need to reassign references in prefabs and scenes.", 
                "OK");
        }

        private void ClearUnityCache()
        {
            try
            {
                string projectPath = Directory.GetCurrentDirectory();
                string libraryPath = Path.Combine(projectPath, "Library");
                
                if (Directory.Exists(libraryPath))
                {
                    // Only delete specific cache folders, not the entire Library
                    string[] cacheFolders = { "ShaderCache", "Artifacts", "Bee", "SourceAssetDB" };
                    
                    foreach (string folder in cacheFolders)
                    {
                        string folderPath = Path.Combine(libraryPath, folder);
                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                            Debug.Log($"[ForceFullReimport] Deleted cache folder: {folder}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ForceFullReimport] Failed to clear cache: {e.Message}");
            }
        }
    }
}
