# Scene Requirements Documentation

## Phase 3: GameManager - Scene-Based Manager Architecture

### Overview
This document outlines the scene setup requirements after removing runtime GameObject instantiation from GameManager. All managers must now be created as scene prefabs instead of being dynamically instantiated at runtime.

---

## ✅ What Changed

### Removed (Violates Unity Lifecycle)
- ❌ `new GameObject()` calls in GameManager
- ❌ `AddComponent<T>()` calls at runtime
- ❌ `InitializeManagers()` method that created managers dynamically
- ❌ Runtime GameObject creation that breaks Unity initialization

### Added (Scene-Based Approach)
- ✅ Public manager references in GameManager
- ✅ `FindObjectOfType<T>()` to locate existing managers
- ✅ Clear logging for missing managers
- ✅ Validation methods to ensure managers exist
- ✅ Documentation explaining why changes were made

---

## 🎯 Scene Setup Requirements

### LoginScene Requirements

The LoginScene must contain the following manager GameObjects as prefabs:

#### 1. AuthManager
- **Component**: `AuthManager.cs`
- **Purpose**: Handles user authentication, login, registration
- **Setup**: 
  - Create GameObject named "AuthManager" in LoginScene
  - Add `AuthManager` component
  - Configure `ServerUrl` in inspector (default: http://localhost:3000)
  - Mark as DontDestroyOnLoad (handled by component)

#### 2. NetworkManager  
- **Component**: `NetworkManager.cs`
- **Purpose**: Manages WebSocket connection to game server
- **Setup**:
  - Create GameObject named "NetworkManager" in LoginScene
  - Add `NetworkManager` component
  - Configure `ServerUrl` and `PvpNamespace` in inspector
  - Mark as DontDestroyOnLoad (handled by component)

#### 3. InputController
- **Component**: `InputController.cs`
- **Purpose**: Handles player input (touch and keyboard)
- **Setup**:
  - Create GameObject named "InputController" in LoginScene
  - Add `InputController` component
  - Configure input settings in inspector
  - Mark as DontDestroyOnLoad (handled by component)

#### 4. GameManager (Optional)
- **Component**: `GameManager.cs`
- **Purpose**: High-level coordinator that references other managers
- **Setup**:
  - Create GameObject named "GameManager" in LoginScene
  - Add `GameManager` component
  - Optionally assign manager references in inspector (or let it find them automatically)
  - Mark as DontDestroyOnLoad (handled by component)

---

## 🔍 How GameManager Finds Managers

### Method 1: Inspector Assignment (Recommended)
Drag and drop manager GameObjects onto GameManager's public fields in Unity Inspector:
- `authManager`
- `networkManager`  
- `inputController`

### Method 2: Automatic Discovery (Fallback)
If not assigned in inspector, GameManager will automatically find managers using `FindObjectOfType<T>()` in `Start()`:

```csharp
private void FindManagers()
{
    if (authManager == null)
        authManager = FindObjectOfType<AuthManager>();
    
    if (networkManager == null)
        networkManager = FindObjectOfType<NetworkManager>();
    
    if (inputController == null)
        inputController = FindObjectOfType<InputController>();
}
```

---

## 📊 Validation & Logging

### Console Output on Success
```
[GameManager] AuthManager found and referenced successfully.
[GameManager] NetworkManager found and referenced successfully.
[GameManager] InputController found and referenced successfully.
```

### Console Output on Missing Managers
```
[GameManager] AuthManager not found! Please add AuthManager to LoginScene prefab.
[GameManager] NetworkManager not found! Please add NetworkManager to LoginScene prefab.
[GameManager] InputController not found! Please add InputController to LoginScene prefab.
```

---

## 🚀 Migration Guide

### For Existing Projects

If you have an existing project that used runtime instantiation:

1. **Open LoginScene** in Unity Editor

2. **Create Manager GameObjects**:
   - Right-click in Hierarchy → Create Empty
   - Name it "AuthManager"
   - Add `AuthManager` component
   - Repeat for NetworkManager and InputController

3. **Optional: Create Prefabs**:
   - Drag each manager GameObject to Project/Assets/Prefabs folder
   - This allows reusing managers across scenes

4. **Test**:
   - Play the scene
   - Check Console for validation logs
   - Ensure all managers are found successfully

### For New Projects

1. Use the BootstrapRunner pattern (already implemented in Bootstrap.cs)
2. BootstrapRunner ensures managers exist before scenes load
3. GameManager acts as a coordinator and validator

---

## 🔧 Technical Details

### Why This Change Was Necessary

**Problem**: Runtime GameObject creation violated Unity's lifecycle:
```csharp
// ❌ OLD CODE (REMOVED)
GameObject authObj = new GameObject("AuthManager");
authManager = authObj.AddComponent<AuthManager>();
DontDestroyOnLoad(authObj);
```

**Issues**:
- MonoManager might not be ready when creating GameObjects at runtime
- Causes NULL reference exceptions later
- Breaks scene-based initialization flow
- Harder to debug and configure

**Solution**: Scene-based prefabs:
```csharp
// ✅ NEW CODE
authManager = FindObjectOfType<AuthManager>();
if (authManager == null)
    Debug.LogWarning("AuthManager not found! Add to scene.");
```

**Benefits**:
- Respects Unity lifecycle
- Easier to configure via Inspector
- Clear error messages
- No NULL references
- Better debugging experience

---

## 📋 Checklist

Before marking Phase 3 complete, verify:

- [ ] GameManager.cs has no `new GameObject()` calls
- [ ] GameManager.cs has no `AddComponent<T>()` calls  
- [ ] All managers defined in scene prefabs (LoginScene)
- [ ] Logging clearly shows when managers are found/missing
- [ ] Code compiles without errors
- [ ] Console shows successful manager validation on play

---

## 🎓 Best Practices

### DO:
✅ Create managers as scene prefabs  
✅ Use FindObjectOfType for discovery  
✅ Add clear logging for debugging  
✅ Validate manager existence  
✅ Document scene requirements  

### DON'T:
❌ Create GameObjects at runtime in Awake/Start  
❌ Use AddComponent dynamically for managers  
❌ Silently fail when managers are missing  
❌ Mix runtime creation with scene-based setup  

---

## 📞 Support

If managers are not being found:
1. Check that manager GameObjects exist in LoginScene
2. Verify components are attached to GameObjects
3. Check Console for validation warnings
4. Ensure no duplicate managers exist (singletons prevent duplicates)
5. Verify BootstrapRunner is creating managers before scenes load

---

**Last Updated**: Phase 3 Implementation  
**Related Files**: 
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Bootstrap/Bootstrap.cs`
- `Assets/Scripts/Auth/AuthManager.cs`
- `Assets/Scripts/Network/NetworkManager.cs`
- `Assets/Scripts/Input/InputController.cs`
