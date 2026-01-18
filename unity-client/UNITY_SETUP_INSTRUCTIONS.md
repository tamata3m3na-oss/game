# Unity Client Setup Instructions

## ⚠️ CRITICAL: Follow these steps IN ORDER before opening the project in Unity

### 1. Install NativeWebSocket Package

**Method 1: Via Package Manager (Recommended)**
1. Open Unity Editor
2. Go to `Window > Package Manager`
3. Click the `+` button in the top-left
4. Select `Add package from git URL...`
5. Enter: `https://github.com/endel/NativeWebSocket.git#upm`
6. Click `Add`

**Method 2: Via manifest.json**
The file `Packages/manifest.json` has been prepared with the correct dependency.
Unity will automatically install it when you open the project.

### 2. Install DOTween (REQUIRED)

DOTween is used extensively for UI animations and effects. You MUST install it:

**Option A: Unity Asset Store (Recommended)**
1. Open Unity Asset Store: https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676
2. Download and Import DOTween (Free)
3. Go to `Tools > Demigiant > DOTween Utility Panel`
4. Click `Setup DOTween`
5. Click `Apply`

**Option B: Unity Package Manager**
1. Go to `Window > Package Manager`
2. Search for "DOTween"
3. Install the package
4. Go to `Tools > Demigiant > DOTween Utility Panel`
5. Click `Setup DOTween` and Apply

**⚠️ IMPORTANT:** You MUST run the DOTween Setup Utility Panel or the project will have compilation errors.

### 3. Import TextMeshPro Essential Resources

1. Open Unity Editor
2. Go to `Window > TextMeshPro > Import TMP Essential Resources`
3. Click `Import`
4. Wait for import to complete

### 4. Verify All Packages Are Installed

Go to `Window > Package Manager` and verify these packages are installed:
- ✅ Input System (1.7.0)
- ✅ TextMeshPro (3.0.6)
- ✅ Unity UI (1.0.0)
- ✅ Addressables (1.19.19)
- ✅ Universal RP (14.0.7)
- ✅ Newtonsoft Json (3.2.1)
- ✅ NativeWebSocket (from git)
- ✅ DOTween (from Asset Store)

### 5. Build Settings Configuration

For Android builds, ensure:
1. Go to `File > Build Settings`
2. Select `Android` and click `Switch Platform`
3. Go to `Player Settings > Other Settings`
4. Set `Minimum API Level` to at least Android 5.0 (API Level 21)
5. Set `Target API Level` to latest

### 6. Project Structure

```
Assets/
├── Scripts/
│   ├── Auth/           - Authentication logic
│   ├── Game/           - Game controllers (Ship, Weapon, Ability)
│   ├── Input/          - Input handling
│   ├── Managers/       - Core managers (Game, GameState)
│   ├── Network/        - NetworkManager with NativeWebSocket
│   ├── UI/             - All UI controllers and scenes
│   └── Utils/          - Utility classes and AppGameState enum
└── ...
```

### 7. Key Changes from Previous Version

#### ✅ Socket.IO → NativeWebSocket Migration
- **OLD:** `using SocketIOClient;`
- **NEW:** `using NativeWebSocket;`
- NetworkManager completely rewritten to use raw WebSocket connections
- Message protocol: JSON with `{type: string, data: string}` format

#### ✅ GameState Naming Conflict Resolved
- **Network Data Class:** `NetworkManager.GameState` (game snapshot data)
- **App State Enum:** `AppGameState` (Boot, Lobby, Match, Result)
- Files should reference the correct type based on context

#### ✅ DOTween Integration
Files using DOTween animations:
- `AnimationController.cs`
- `TransitionManager.cs`
- `GlowEffect.cs`
- `ShakeEffect.cs`
- `BloomEffect.cs`
- `ResultSceneUI.cs`

All have `using DG.Tweening;` at the top.

### 8. Testing WebSocket Connection

When you run the project:
1. Start with the `LoginScene` (or create a Boot scene that loads Login)
2. Check Console for: `"Attempting WebSocket connection to: ws://localhost:3000/pvp?token=..."`
3. If server is running, you'll see: `"WebSocket connected to server"`
4. If server is NOT running, you'll see connection errors (EXPECTED in dev)

### 9. Backend Server Configuration

To connect to the backend:
1. Ensure backend is running on `localhost:3000` (or update `NetworkManager.ServerUrl`)
2. WebSocket endpoint: `ws://localhost:3000/pvp`
3. Authentication token is passed as query parameter: `?token=YOUR_JWT_TOKEN`

For production, update:
```csharp
// In NetworkManager.cs
public string ServerUrl = "ws://your-production-url.com";
```

### 10. Common Issues and Solutions

#### ❌ "The type or namespace name 'SocketIOClient' could not be found"
**Solution:** Package cleanup was successful. This should NOT appear anymore.

#### ❌ "The type or namespace name 'DG' could not be found"
**Solution:** Install DOTween from Asset Store and run Setup Utility Panel.

#### ❌ "The type or namespace name 'TMPro' could not be found"
**Solution:** Import TMP Essential Resources (Window > TextMeshPro > Import TMP Essential Resources).

#### ❌ "The type or namespace name 'NativeWebSocket' could not be found"
**Solution:** Add NativeWebSocket via Package Manager using git URL.

#### ❌ "WebSocket connection failed"
**Solution:** Normal if backend is not running. Start the NestJS backend server first.

### 11. Development Workflow

1. **Start Backend:** `cd backend && npm run start:dev`
2. **Open Unity:** The project should compile without errors
3. **Press Play:** Scene should load without red errors in Console
4. **Test Authentication:** Use Login scene to authenticate
5. **Test Matchmaking:** Use Lobby scene to join queue
6. **Test Game:** Play a match when matched

### 12. Files Checklist

Verify these files exist and have correct imports:

- ✅ `Assets/Scripts/Network/NetworkManager.cs` - Uses `NativeWebSocket`
- ✅ `Assets/Scripts/Utils/AppGameState.cs` - Enum for app states
- ✅ `Packages/manifest.json` - Contains all dependencies
- ✅ All UI files - Have `using TMPro;` and `using UnityEngine.UI;`
- ✅ All animation files - Have `using DG.Tweening;`

### 13. Next Steps

After completing this setup:
1. All compiler errors should be ZERO
2. Play mode should work without crashes
3. WebSocket connection attempts should log properly
4. UI should render correctly with TextMeshPro components

### 🎉 Success Criteria

✅ Unity opens the project without compile errors  
✅ No missing namespace errors for NativeWebSocket  
✅ No missing namespace errors for DG.Tweening  
✅ No missing namespace errors for TMPro  
✅ Press Play in any scene - Console is clean (no red errors)  
✅ WebSocket connection logs appear in Console  
✅ All UI elements render properly  

---

## Support

If you encounter issues:
1. Check Unity Console for specific error messages
2. Verify all packages are installed in Package Manager
3. Ensure DOTween Setup Utility Panel was run
4. Check that TMP Essential Resources were imported
5. Restart Unity Editor if needed

**Version:** Unity 2022.3+  
**Last Updated:** January 2025
