# Unity Project Repair Guide

This guide explains how to fix broken GUIDs, corrupted asset files, and other common Unity project issues using the repair utilities included in this project.

## 🔴 Critical Issues Fixed

### 1. Assembly Definition Error
**Issue**: `Assembly-CSharp.asmdef` uses a reserved name that Unity doesn't allow.

**Fix**: Renamed to `GameProject.asmdef`
- File: `Assets/Scripts/Assembly-CSharp.asmdef` → `Assets/Scripts/GameProject.asmdef`
- The assembly name is now "GameProject" instead of the reserved "Assembly-CSharp"

### 2. Corrupted ImpactParticles Prefab
**Issue**: Parser failure in `Assets/Prefabs/Particles/ImpactParticles.prefab`

**Fix**: Deleted the corrupted file. The prefab needs to be recreated in Unity Editor.

---

## 🛠️ Repair Tools

### Tools Menu Location
All repair tools are available in Unity Editor under:
```
Tools → Project Repair → [Tool Name]
```

### 1. Prefab Repair Utility
**Location**: `Tools/Project Repair/Prefab Repair Utility`

**Features**:
- **Scan All Prefabs**: Scans all prefabs for broken GUID references
- **Auto-Fix All**: Automatically replaces broken GUIDs with actual script GUIDs
- **Refresh Script GUIDs**: Regenerates .meta files for all scripts

**How to Use**:
1. Open the tool from the menu
2. Click "Scan All Prefabs" to identify issues
3. Review the detected broken references
4. Click "Auto-Fix All" to automatically repair them
5. Click "Refresh Script GUIDs" to ensure all scripts have proper .meta files

**What It Fixes**:
- Broken script GUIDs (like `0000000000000001`)
- Maps them to actual scripts (e.g., `0000000000000001` → `ShipController`)
- Updates prefab files with correct GUIDs

### 2. Remove Broken References
**Location**: `Tools/Project Repair/Remove Broken References`

**Features**:
- **Scan & Remove Broken Components**: Removes MonoBehaviour components with missing scripts
- **Clean Selected Prefab**: Fixes only the currently selected prefab

**How to Use**:
1. Select a prefab in Project window (or use scan all)
2. Open the tool
3. Click "Scan & Remove Broken Components" for full project scan
4. Or click "Clean Selected Prefab" for the selected prefab

**When to Use**:
- When you see "The associated script can not be loaded" errors
- When prefabs have missing MonoBehaviours
- When components are grayed out in Inspector with "Missing Script"

### 3. Validate Project Integrity
**Location**: `Tools/Project Repair/Validate Project Integrity`

**Features**:
- Checks assembly definitions for invalid names
- Scans prefabs for broken references
- Validates scene files
- Checks for missing .meta files
- Validates script compilation status

**How to Use**:
1. Open the tool
2. Click "Run Full Validation"
3. Review results by category
4. Click "Export Report" to save a detailed report

**Categories Checked**:
- ✅ Assembly Definitions
- ✅ Prefab References
- ✅ Script Files
- ✅ Scene Files
- ✅ Script Consistency

### 4. Scene Repair Utility
**Location**: `Tools/Project Repair/Scene Repair Utility`

**Features**:
- **Scan All Scenes**: Scans all scene files for broken GUID references
- **Auto-Fix Scene References**: Automatically replaces broken GUIDs in scenes
- **Validate Open Scene**: Checks the currently open scene for missing scripts

**How to Use**:
1. Open the tool
2. Click "Scan All Scenes" to identify issues across all scenes
3. Review detected broken references
4. Click "Auto-Fix Scene References" to repair them
5. For specific scenes, open the scene and click "Validate Open Scene"

**When to Use**:
- When scenes have "Missing Script" warnings in the Inspector
- When GameObject components show as (Missing Script)
- After fixing script GUIDs and wanting to update scene references

### 5. Force Full Reimport
**Location**: `Tools/Project Repair/Force Full Reimport`

**Features**:
- **Start Full Reimport**: Forces reimport of all assets
- **Reimport All Scripts**: Reimports only .cs files
- **Reimport All Prefabs**: Reimports only prefab files
- **Reimport All Scenes**: Reimports only scene files
- **Regenerate Meta Files**: Deletes and regenerates all .meta files

**How to Use**:
1. Open the tool
2. Choose the reimport option you need
3. For full reimport, enable additional options:
   - "Reimport Library Assets" - includes Library folder (slower)
   - "Clear Cache After Reimport" - clears Unity cache
4. Click "Start" and wait for completion

**⚠️ WARNING**:
- Full reimport can take significant time
- "Regenerate Meta Files" will change all GUIDs
- **Always backup your project before using this tool**

---

## 📋 Step-by-Step Repair Process

### Phase 1: Quick Assessment
1. Open Unity Editor
2. Tools → Project Repair → Validate Project Integrity
3. Click "Run Full Validation"
4. Review the report to understand what needs fixing

### Phase 2: Fix Assembly Definition
**Already fixed in this commit:**
- ✅ `Assembly-CSharp.asmdef` renamed to `GameProject.asmdef`

### Phase 3: Repair Prefabs
1. Tools → Project Repair → Prefab Repair Utility
2. Click "Scan All Prefabs"
3. Review broken references
4. Click "Auto-Fix All" (this will replace broken GUIDs)
5. Click "Refresh Script GUIDs" to generate .meta files

### Phase 4: Repair Scenes
1. Tools → Project Repair → Scene Repair Utility
2. Click "Scan All Scenes"
3. Review broken script references in scenes
4. Click "Auto-Fix Scene References" to repair them
5. Open each scene and verify no errors remain

### Phase 5: Remove Remaining Broken Components
1. Tools → Project Repair → Remove Broken References
2. Click "Scan & Remove Broken Components"
3. This will remove any components that couldn't be auto-fixed

### Phase 6: Validate Again
1. Tools → Project Repair → Validate Project Integrity
2. Click "Run Full Validation"
3. Verify no critical issues remain

### Phase 7: Final Cleanup (If Needed)
If issues persist:
1. Backup your project
2. Tools → Project Repair → Force Full Reimport
3. Enable "Clear Cache After Reimport"
4. Click "Start Full Reimport"
5. Wait for completion

---

## 🎯 Expected Success Criteria

After following this guide, you should achieve:

✅ **No Broken GUID Errors**
- Console should not show "Could not extract GUID" messages

✅ **No Parser Errors**
- No "Parser Failure" messages in Console

✅ **No Assembly Definition Errors**
- Scripts should compile without errors

✅ **Valid Prefabs**
- All prefabs load without missing script warnings
- Components are properly assigned

✅ **Valid Scenes**
- Scenes open successfully
- No broken references in scene hierarchy

✅ **Clean Asset Database**
- All .meta files present
- AssetDatabase is consistent

---

## 🔍 Manual Fixes (If Tools Don't Work)

### Fixing Specific Prefabs Manually

1. **Open Prefab in Unity Editor**
   - Double-click the prefab to open it
   - Look for red "Missing Script" warnings

2. **Reassign Missing Scripts**
   - Click the broken component
   - Look for the Script field in Inspector
   - Drag the correct script from Project window
   - Or use the dropdown to select it

3. **Save Prefab**
   - Ctrl+S (Windows) or Cmd+S (Mac)
   - Unity will update the .prefab file with correct GUID

### Recreating a Corrupted Prefab

If a prefab is severely corrupted:

1. **Create New GameObject**
   - Hierarchy → Create Empty
   - Name it appropriately

2. **Add Components**
   - Add the required components
   - Set up properties and references

3. **Create Prefab**
   - Drag GameObject to Project window
   - Save as new prefab

4. **Update Scenes**
   - Find scenes that reference old prefab
   - Replace with new prefab
   - Or update prefab references manually

---

## 📝 Common GUID Mappings

The `PrefabRepairUtility` auto-maps these broken GUIDs:

| Broken GUID | Script Name |
|-------------|-------------|
| `0000000000000001` | ShipController |
| `0000000000000002` | WeaponController |
| `0000000000000003` | AbilityController |
| `0000000000000004` | Bullet |
| `0000000000001000` | ThreadSafeEventQueue |
| `0000000000001001` | AuthManager |
| `0000000000001002` | NetworkManager |
| `0000000000001003` | HttpNetworkManager |
| `0000000000001004` | InputController |
| `0000000000001005` | GameManager |
| `0000000000001007` | GameTickManager |
| `0000000000001008` | NetworkEventManager |
| `0000000000001009` | GameStateManager |
| `0000000000001010` | AnimationController |
| `0000000000001011` | ParticleController |
| `0000000000001012` | ObjectPool |
| `0000000000001013` | TransitionManager |

---

## 💡 Best Practices

### Prevention
1. **Always Commit .meta Files**
   - Never ignore .meta files in Git
   - This ensures GUID consistency across team members

2. **Avoid Renaming Scripts**
   - If you must rename, let Unity handle it
   - Don't manually rename .cs files without renaming in Unity

3. **Use Version Control**
   - Commit often
   - Tag working states
   - You can always rollback if something breaks

### Before Major Changes
1. **Create Backup**
   - Copy entire project folder
   - Or commit to Git with clear message

2. **Validate Current State**
   - Run Validate Project Integrity
   - Export report for comparison

3. **Document Changes**
   - Keep track of what you're modifying
   - Makes rollback easier

### After Repairs
1. **Test Scenes**
   - Open each scene
   - Verify no errors in Console
   - Test gameplay functionality

2. **Build Project**
   - Try a test build
   - Verify no build errors

3. **Commit Changes**
   - Include .meta files
   - Write descriptive commit message
   - Reference this repair guide if needed

---

## 🆘 Troubleshooting

### Issue: Scripts Still Show "Missing Script"

**Solution**:
1. Check if the script file exists
2. Verify the namespace is correct
3. Check for compilation errors
4. Try "Refresh Script GUIDs" in Prefab Repair Utility

### Issue: Auto-Fix Doesn't Work

**Solution**:
1. The actual script file may not exist
2. The script name may not match the expected name
3. Manually reassign the script in Unity Editor
4. Or create the missing script file

### Issue: Reimport Takes Too Long

**Solution**:
1. Use selective reimport (Scripts/Prefabs/Scenes)
2. Uncheck "Reimport Library Assets"
3. Consider reimporting only the problematic assets

### Issue: GUIDs Keep Changing

**Solution**:
1. Ensure .meta files are in Git
2. Don't manually edit .meta files
3. Don't delete .meta files unless regenerating all

---

## 📚 Additional Resources

### Unity Documentation
- [Asset Database](https://docs.unity3d.com/Manual/AssetDatabase.html)
- [ScriptableObjects](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [Importing Assets](https://docs.unity3d.com/Manual/AssetImportProcess.html)

### Related Tools
- `ProjectHealthCheckWindow` - General project diagnostics
- `ProjectCleanupUtility` - Safe cleanup of Library/Temp folders

---

## ✨ Summary

This repair system provides a comprehensive solution for:

1. ✅ Fixed Assembly-CSharp reserved name error
2. ✅ Removed corrupted ImpactParticles prefab
3. ✅ Created PrefabRepairUtility - Auto-fixes broken GUIDs
4. ✅ Created RemoveBrokenReferences - Removes missing scripts
5. ✅ Created ValidateProjectIntegrity - Full project validation
6. ✅ Created ForceFullReimport - Asset database rebuild

Follow the **Step-by-Step Repair Process** above to fix your project systematically.
