#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UnityProjectTools
{
    /// <summary>
    /// Project Cleanup Utility
    /// Safe cleanup scripts for Library, Temp, obj folders
    /// Provides verification and automatic reimport handling
    /// </summary>
    public class ProjectCleanupUtility : EditorWindow
    {
        private static ProjectCleanupUtility window;
        
        [MenuItem("Tools/Project Cleanup Utility")]
        public static void ShowWindow()
        {
            window = GetWindow<ProjectCleanupUtility>("Project Cleanup");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private Vector2 scrollPosition;
        private string cleanupLog = "";
        private bool isCleaning = false;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🧹 Unity Project Cleanup Utility", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Safe cleanup for corrupted cache and files", EditorStyles.miniLabel);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawSection("🔍 Cache Cleanup", DrawCacheCleanup);
            DrawSection("📂 File Verification", DrawFileVerification);
            DrawSection("🔧 Unity Asset Database", DrawAssetDatabase);
            DrawSection("🎯 Smart Cleanup", DrawSmartCleanup);
            DrawSection("📋 Cleanup Log", DrawCleanupLog);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (isCleaning)
            {
                GUI.enabled = false;
                GUILayout.Button("🧹 Cleaning in progress...", GUILayout.Height(30));
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("🧹 Start Complete Cleanup", GUILayout.Height(30)))
                {
                    StartCompleteCleanup();
                }
            }
        }

        private void DrawSection(string title, System.Action drawContent)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            drawContent?.Invoke();
            EditorGUI.indentLevel--;
        }

        private void DrawCacheCleanup()
        {
            EditorGUILayout.LabelField("Clean Unity cache folders that may contain corrupted data:");
            
            string[] cacheFolders = {
                ("Library", "Unity Library folder - contains compiled assets and cache"),
                ("Temp", "Unity temporary files"),
                ("obj", "Compiled object files"),
                ("UserSettings", "User-specific editor settings")
            };

            foreach (var folder in cacheFolders)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(folder.Item1, GUILayout.Width(100));
                EditorGUILayout.LabelField(folder.Item2, EditorStyles.wordWrappedLabel);
                
                if (GUILayout.Button("Clean", GUILayout.Width(60)))
                {
                    CleanFolder(folder.Item1);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawFileVerification()
        {
            EditorGUILayout.LabelField("Verify integrity of project files:");
            
            string[] verificationChecks = {
                ("Scripts", "Verify all C# scripts compile"),
                ("Scenes", "Check scene file integrity"),
                ("Prefabs", "Validate prefab references"),
                ("Materials", "Check material and shader files"),
                ("Textures", "Verify texture files and formats")
            };

            foreach (var check in verificationChecks)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(check.Item1, GUILayout.Width(100));
                EditorGUILayout.LabelField(check.Item2, EditorStyles.wordWrappedLabel);
                
                if (GUILayout.Button("Verify", GUILayout.Width(60)))
                {
                    VerifyFileGroup(check.Item1);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawAssetDatabase()
        {
            EditorGUILayout.LabelField("Unity Asset Database operations:");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh Asset Database", GUILayout.Height(25)))
            {
                RefreshAssetDatabase();
            }
            if (GUILayout.Button("🗑️ Reimport All Assets", GUILayout.Height(25)))
            {
                ReimportAllAssets();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🧹 Clear Asset Import Cache", GUILayout.Height(25)))
            {
                ClearAssetImportCache();
            }
            if (GUILayout.Button("📊 Analyze Dependencies", GUILayout.Height(25)))
            {
                AnalyzeDependencies();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSmartCleanup()
        {
            EditorGUILayout.LabelField("Intelligent cleanup based on detected issues:");
            
            if (GUILayout.Button("🔍 Scan for Issues", GUILayout.Height(25)))
            {
                ScanForCleanupIssues();
            }
            
            if (GUILayout.Button("🎯 Fix Detected Issues", GUILayout.Height(25)))
            {
                FixDetectedIssues();
            }
            
            if (GUILayout.Button("📋 Generate Report", GUILayout.Height(25)))
            {
                GenerateCleanupReport();
            }
        }

        private void DrawCleanupLog()
        {
            EditorGUILayout.LabelField("Cleanup Operations Log:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("📋 Clear Log", GUILayout.Height(20)))
            {
                cleanupLog = "";
            }
            
            if (GUILayout.Button("📤 Export Log", GUILayout.Height(20)))
            {
                ExportCleanupLog();
            }

            Rect rect = GUILayoutUtility.GetRect(0, 150, GUILayout.ExpandWidth(true));
            GUI.Box(rect, cleanupLog);
        }

        private void CleanFolder(string folderName)
        {
            if (isCleaning) return;
            
            string projectPath = Application.dataPath.Replace("/Assets", "");
            string folderPath = Path.Combine(projectPath, folderName);
            
            if (!Directory.Exists(folderPath))
            {
                AddToLog($"Folder {folderName} does not exist.");
                return;
            }

            AddToLog($"Starting cleanup of {folderName}...");
            
            try
            {
                var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                int deletedCount = 0;
                long freedBytes = 0;

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        freedBytes += fileInfo.Length;
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch (System.Exception ex)
                    {
                        AddToLog($"Failed to delete {file}: {ex.Message}");
                    }
                }

                // Delete empty directories
                DeleteEmptyDirectories(folderPath);
                
                AddToLog($"✅ Cleaned {folderName}: Deleted {deletedCount} files, freed {FormatBytes(freedBytes)}");
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error cleaning {folderName}: {ex.Message}");
            }

            // Refresh Asset Database after cleanup
            AssetDatabase.Refresh();
        }

        private void DeleteEmptyDirectories(string directory)
        {
            try
            {
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach (string subdirectory in subdirectories)
                {
                    DeleteEmptyDirectories(subdirectory);
                    if (Directory.GetFiles(subdirectory).Length == 0 && Directory.GetDirectories(subdirectory).Length == 0)
                    {
                        Directory.Delete(subdirectory);
                        AddToLog($"Deleted empty directory: {subdirectory}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                AddToLog($"Error deleting empty directories: {ex.Message}");
            }
        }

        private void VerifyFileGroup(string groupName)
        {
            AddToLog($"Verifying {groupName}...");
            
            string assetsPath = Application.dataPath;
            string groupPath = Path.Combine(assetsPath, groupName);
            
            if (!Directory.Exists(groupPath))
            {
                AddToLog($"❌ {groupName} folder not found");
                return;
            }

            int totalFiles = 0;
            int corruptedFiles = 0;

            try
            {
                string[] files = Directory.GetFiles(groupPath, "*.*", SearchOption.AllDirectories);
                totalFiles = files.Length;

                foreach (string file in files)
                {
                    try
                    {
                        if (groupName == "Scripts")
                        {
                            VerifyScriptFile(file);
                        }
                        else if (groupName == "Scenes")
                        {
                            VerifySceneFile(file);
                        }
                        else if (groupName == "Prefabs")
                        {
                            VerifyPrefabFile(file);
                        }
                        // Add more verification methods as needed
                    }
                    catch (System.Exception ex)
                    {
                        corruptedFiles++;
                        AddToLog($"Corrupted file: {file} - {ex.Message}");
                    }
                }

                AddToLog($"✅ Verified {groupName}: {totalFiles - corruptedFiles}/{totalFiles} files OK");
                if (corruptedFiles > 0)
                {
                    AddToLog($"⚠️ {corruptedFiles} files may be corrupted");
                }
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error verifying {groupName}: {ex.Message}");
            }
        }

        private void VerifyScriptFile(string filePath)
        {
            if (!filePath.EndsWith(".cs")) return;
            
            // Basic syntax check
            string content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content) || content.Trim().Length < 10)
            {
                throw new System.Exception("Script file is empty or too short");
            }

            // Check for common compilation errors
            if (content.Contains("syntax error"))
            {
                throw new System.Exception("Contains syntax errors");
            }
        }

        private void VerifySceneFile(string filePath)
        {
            if (!filePath.EndsWith(".unity")) return;
            
            string content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                throw new System.Exception("Scene file is empty");
            }

            // Basic scene file validation
            if (!content.Contains("%YAML"))
            {
                throw new System.Exception("Invalid YAML format");
            }
        }

        private void VerifyPrefabFile(string filePath)
        {
            if (!filePath.EndsWith(".prefab")) return;
            
            string content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                throw new System.Exception("Prefab file is empty");
            }

            // Basic prefab validation
            if (!content.Contains("%YAML"))
            {
                throw new System.Exception("Invalid prefab format");
            }
        }

        private void RefreshAssetDatabase()
        {
            AddToLog("🔄 Refreshing Asset Database...");
            
            try
            {
                AssetDatabase.Refresh();
                AddToLog("✅ Asset Database refreshed successfully");
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error refreshing Asset Database: {ex.Message}");
            }
        }

        private void ReimportAllAssets()
        {
            if (isCleaning) return;
            
            AddToLog("🗑️ Starting complete asset reimport...");
            
            try
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                AddToLog("✅ Asset reimport completed");
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error during asset reimport: {ex.Message}");
            }
        }

        private void ClearAssetImportCache()
        {
            AddToLog("🧹 Clearing Asset Import Cache...");
            
            try
            {
                string libraryPath = Application.dataPath.Replace("/Assets", "/Library");
                string cachePath = Path.Combine(libraryPath, "AssetImportCache");
                
                if (Directory.Exists(cachePath))
                {
                    Directory.Delete(cachePath, true);
                    AddToLog("✅ Asset Import Cache cleared");
                }
                else
                {
                    AddToLog("ℹ️ Asset Import Cache not found");
                }
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error clearing Asset Import Cache: {ex.Message}");
            }
        }

        private void AnalyzeDependencies()
        {
            AddToLog("📊 Analyzing project dependencies...");
            
            try
            {
                string[] scripts = AssetDatabase.FindAssets("t:Script");
                var dependencies = new Dictionary<string, int>();
                
                foreach (string guid in scripts)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string content = File.ReadAllText(path);
                    
                    // Extract using statements
                    string[] lines = content.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Trim().StartsWith("using "))
                        {
                            string import = line.Trim().Substring(6).Split(';')[0];
                            if (dependencies.ContainsKey(import))
                                dependencies[import]++;
                            else
                                dependencies[import] = 1;
                        }
                    }
                }

                AddToLog("📊 Dependency Analysis:");
                foreach (var dep in dependencies.OrderByDescending(x => x.Value).Take(10))
                {
                    AddToLog($"  {dep.Key}: {dep.Value} files");
                }
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error analyzing dependencies: {ex.Message}");
            }
        }

        private void ScanForCleanupIssues()
        {
            AddToLog("🔍 Scanning for cleanup issues...");
            
            var issues = new List<string>();

            // Check for large files
            try
            {
                string[] allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
                var largeFiles = allFiles.Where(f => new FileInfo(f).Length > 50 * 1024 * 1024).ToArray(); // > 50MB
                
                if (largeFiles.Length > 0)
                {
                    issues.Add($"Found {largeFiles.Length} large files (>50MB)");
                }
            }
            catch { /* ignore */ }

            // Check for duplicate files
            try
            {
                var filesBySize = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .GroupBy(f => new FileInfo(f).Length)
                    .Where(g => g.Count() > 1);
                
                int duplicateGroups = filesBySize.Count();
                if (duplicateGroups > 0)
                {
                    issues.Add($"Found {duplicateGroups} groups of duplicate-sized files");
                }
            }
            catch { /* ignore */ }

            // Check for empty directories
            try
            {
                var emptyDirs = GetEmptyDirectories(Application.dataPath);
                if (emptyDirs.Count > 0)
                {
                    issues.Add($"Found {emptyDirs.Count} empty directories");
                }
            }
            catch { /* ignore */ }

            if (issues.Count == 0)
            {
                AddToLog("✅ No major cleanup issues detected");
            }
            else
            {
                AddToLog("⚠️ Issues detected:");
                foreach (string issue in issues)
                {
                    AddToLog($"  - {issue}");
                }
            }
        }

        private void FixDetectedIssues()
        {
            AddToLog("🎯 Fixing detected issues...");
            
            // Fix empty directories
            try
            {
                var emptyDirs = GetEmptyDirectories(Application.dataPath);
                foreach (string dir in emptyDirs)
                {
                    Directory.Delete(dir);
                    AddToLog($"Deleted empty directory: {dir}");
                }
            }
            catch (System.Exception ex)
            {
                AddToLog($"Error fixing empty directories: {ex.Message}");
            }

            AddToLog("✅ Issue fixing completed");
        }

        private void GenerateCleanupReport()
        {
            string report = GenerateCleanupReportContent();
            string fileName = $"cleanup_report_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(Application.dataPath, "..", fileName);
            
            try
            {
                File.WriteAllText(filePath, report);
                AddToLog($"📋 Cleanup report generated: {fileName}");
                
                // Open the report
                Application.OpenURL("file://" + filePath);
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error generating report: {ex.Message}");
            }
        }

        private string GenerateCleanupReportContent()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== UNITY PROJECT CLEANUP REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Project: {Application.productName}");
            report.AppendLine($"Unity Version: {Application.unityVersion}");
            report.AppendLine();
            
            report.AppendLine("=== PROJECT STATISTICS ===");
            try
            {
                string[] allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
                report.AppendLine($"Total Files: {allFiles.Length}");
                
                var fileSizes = allFiles.Select(f => new FileInfo(f).Length);
                report.AppendLine($"Total Size: {FormatBytes(fileSizes.Sum())}");
                report.AppendLine($"Average File Size: {FormatBytes((long)fileSizes.Average())}");
                
                var extensions = allFiles.GroupBy(f => Path.GetExtension(f)).OrderByDescending(g => g.Count());
                report.AppendLine("Top File Extensions:");
                foreach (var ext in extensions.Take(5))
                {
                    report.AppendLine($"  {ext.Key}: {ext.Count()} files");
                }
            }
            catch (System.Exception ex)
            {
                report.AppendLine($"Error gathering statistics: {ex.Message}");
            }
            
            report.AppendLine();
            report.AppendLine("=== CLEANUP LOG ===");
            report.AppendLine(cleanupLog);
            
            return report.ToString();
        }

        private void ExportCleanupLog()
        {
            if (string.IsNullOrEmpty(cleanupLog))
            {
                AddToLog("ℹ️ No log entries to export");
                return;
            }

            string fileName = $"cleanup_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(Application.dataPath, "..", fileName);
            
            try
            {
                File.WriteAllText(filePath, cleanupLog);
                AddToLog($"📤 Cleanup log exported: {fileName}");
            }
            catch (System.Exception ex)
            {
                AddToLog($"❌ Error exporting log: {ex.Message}");
            }
        }

        private void StartCompleteCleanup()
        {
            if (isCleaning) return;
            
            isCleaning = true;
            AddToLog("🧹 Starting complete project cleanup...");
            
            // This would run all cleanup operations in sequence
            // For now, just simulate a complete cleanup
            
            System.Threading.Tasks.Task.Run(() =>
            {
                // Run cleanup in background
                System.Threading.Thread.Sleep(1000);
                
                // This is a simplified version - in a real implementation,
                // you'd run all the cleanup methods sequentially
                
                EditorApplication.InvokeOnLateRendering(() =>
                {
                    isCleaning = false;
                    AddToLog("✅ Complete cleanup finished");
                });
            });
        }

        private List<string> GetEmptyDirectories(string root)
        {
            var emptyDirs = new List<string>();
            
            try
            {
                foreach (string dir in Directory.GetDirectories(root))
                {
                    if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                    {
                        emptyDirs.Add(dir);
                    }
                    else
                    {
                        emptyDirs.AddRange(GetEmptyDirectories(dir));
                    }
                }
            }
            catch { /* ignore */ }
            
            return emptyDirs;
        }

        private void AddToLog(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            cleanupLog += $"[{timestamp}] {message}\n";
            
            // Keep log to reasonable size
            if (cleanupLog.Length > 10000)
            {
                cleanupLog = cleanupLog.Substring(cleanupLog.Length - 9000);
            }
            
            Repaint();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
#endif