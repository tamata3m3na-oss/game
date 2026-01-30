# Unity Build Settings - Scene GUID Mappings

## Scene Order in Build Settings

This document shows the correct order of scenes in the Unity Build Settings with their corresponding GUIDs.

### Scene Order Configuration

| Index | Scene Name | GUID | Path |
|-------|-----------|------|------|
| **0** | **Splash** | `a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6` | `Assets/Scenes/Splash.unity` |
| **1** | **Login** | `d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6` | `Assets/Scenes/Login.unity` |
| **2** | **Lobby** | `e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6` | `Assets/Scenes/Lobby.unity` |
| **3** | **Game** | `c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5` | `Assets/Scenes/Game.unity` |
| **4** | **Result** | `f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7` | `Assets/Scenes/Result.unity` |

## EditorBuildSettings.asset Content

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes:
  - enabled: 1
    path: Assets/Scenes/Splash.unity
    guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6
  - enabled: 1
    path: Assets/Scenes/Login.unity
    guid: d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6
  - enabled: 1
    path: Assets/Scenes/Lobby.unity
    guid: e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6
  - enabled: 1
    path: Assets/Scenes/Game.unity
    guid: c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5
  - enabled: 1
    path: Assets/Scenes/Result.unity
    guid: f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7
  m_configObjects: {}
```

## Scene Details

### 1. Splash (Index 0)
- **Purpose**: Initial splash screen and game bootstrap
- **GUID**: `a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6`
- **Controllers**: SplashController, GameBootstrap
- **Next Scene**: Login (or Lobby if auto-login successful)

### 2. Login (Index 1)
- **Purpose**: User authentication
- **GUID**: `d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6`
- **Controllers**: LoginController
- **Next Scene**: Lobby (on successful login)

### 3. Lobby (Index 2)
- **Purpose**: Matchmaking lobby and queue management
- **GUID**: `e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6`
- **Controllers**: LobbyController
- **Next Scene**: Game (when match is ready)

### 4. Game (Index 3)
- **Purpose**: Main gameplay scene
- **GUID**: `c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5`
- **Controllers**: GameController, InputSender, MatchHUD
- **Next Scene**: Result (when match ends)

### 5. Result (Index 4)
- **Purpose**: Match results display and rating updates
- **GUID**: `f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7`
- **Controllers**: ResultController
- **Next Scene**: Lobby (return to matchmaking)

## Verification

All scenes have been verified with:
- ✅ Valid 32-character hex GUIDs
- ✅ Proper .meta files
- ✅ Correct scene GUID in scene files (m_SceneGUID)
- ✅ All script references with valid GUIDs
- ✅ No broken references
- ✅ Valid YAML structure

## Build Flow

```
Splash (0)
   ↓
Login (1)
   ↓
Lobby (2)
   ↓
Game (3)
   ↓
Result (4)
   ↓
Lobby (2) [loop]
```

## Notes

- All scenes are enabled in build settings
- The order is critical for proper scene navigation
- SceneManager.LoadScene() uses scene names for transitions
- Build index can also be used with SceneManager.LoadScene(buildIndex)
