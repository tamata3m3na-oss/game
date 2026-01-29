# BattleStar Unity Client - Phase 1 Setup Guide

## Prerequisites
- Unity 2022.3 LTS or later
- Backend server running (NestJS API)

## Step 1: Create New Unity Project
1. Open Unity Hub
2. Click "New Project"
3. Select "3D" template
4. Name: "BattleStar-Client"
5. Location: `/home/engine/project/unity-client`
6. Click "Create"

## Step 2: Install Required Packages

### Via Unity Package Manager:
1. Open Window → Package Manager
2. Click "+" → "Add package by name..."

### Required Packages:
```
com.unity.textmeshpro (Latest stable)
com.unity.nuget.newtonsoft-json (Latest stable)
```

### DOTween Installation:
1. Open Asset Store (Window → Asset Store)
2. Search for "DOTween"
3. Import "DOTween (HOTween v2)"
4. Follow the DOTween setup wizard when prompted

## Step 3: Create Folder Structure

In Project window, create these folders:
```
Assets/
├── _Core/              # Core systems and managers
├── _Auth/              # Authentication system
├── _Network/           # HTTP/WebSocket networking
├── _UI/                # UI controllers and components
├── _Models/            # Data models and DTOs
├── Scenes/             # Unity scenes
└── Plugins/            # Third-party plugins
```

## Step 4: Configure Project Settings

### Player Settings:
1. Edit → Project Settings → Player
2. Set Company Name: "BattleStar"
3. Set Product Name: "BattleStar PvP"
4. Set Version: "1.0.0"

### Android Settings (if targeting Android):
1. Switch Platform to Android
2. Player Settings → Android:
   - Minimum API Level: API 23 (Android 6.0)
   - Target API Level: Latest installed
   - Scripting Backend: IL2CPP
   - API Compatibility Level: .NET Standard 2.1

## Step 5: Copy Implementation Files

Copy these files to their respective folders:

### Models (`_Models/`):
- `AuthModels.cs` - Authentication data structures

### Core (`_Core/`):
- `GameManager.cs` - Main game controller
- `SceneController.cs` - Scene management

### Auth (`_Auth/`):
- `AuthManager.cs` - Authentication system

### Network (`_Network/`):
- `HttpService.cs` - Centralized HTTP client

### UI (`_UI/`):
- `SplashController.cs` - Splash screen
- `LoginController.cs` - Login/Register UI
- `LobbyController.cs` - Lobby UI
- `UIManager.cs` - UI helper utilities

## Step 6: Create Scenes

### 1. Splash Scene
1. File → New Scene
2. Save as "Splash" in `Scenes/`
3. Add Canvas with:
   - Image (Logo)
   - Text (Loading message)
4. Attach `SplashController.cs` to Canvas
5. Set Next Scene to "Login"

### 2. Login Scene
1. File → New Scene
2. Save as "Login" in `Scenes/`
3. Create UI:
   - Tab buttons (Login/Register)
   - Input fields (Email, Password, Name)
   - Buttons (Submit)
   - Error text
   - Loading indicator
4. Attach `LoginController.cs` to Canvas/Panel

### 3. Lobby Scene
1. File → New Scene
2. Save as "Lobby" in `Scenes/`
3. Create UI:
   - Welcome text (player name)
   - Player stats display
   - Matchmaking button (future)
   - Logout button
4. Attach `LobbyController.cs`

## Step 7: Configure Backend URL

### For HttpService:
1. Create empty GameObject named "HttpService"
2. Attach `HttpService.cs` component
3. Set Base URL: `http://localhost:3000` (or your backend URL)

### For AuthManager:
1. Create empty GameObject named "AuthManager"
2. Attach `AuthManager.cs` component

## Step 8: Scene Build Settings

1. File → Build Settings
2. Add scenes in order:
   - Splash
   - Login
   - Lobby

## Step 9: Test Authentication Flow

1. Press Play in Unity
2. Should see Splash scene (2 seconds)
3. Login scene appears
4. Try to register/login
5. Should reach Lobby with player name displayed

## Common Issues & Solutions

### CORS Issues:
- Backend must be configured to allow Unity client origin
- Check browser console for CORS errors

### Network Errors:
- Verify backend URL is correct and server is running
- Check firewall settings
- Use `127.0.0.1` instead of `localhost` if needed

### JSON Serialization:
- Ensure model classes have proper [Serializable] attribute
- Check field names match exactly with backend DTOs

## Quality Gates Checklist

✅ Zero Compilation Errors
✅ Zero Runtime Errors in Lobby
✅ Token persists in Memory during session
✅ Profile data shows correctly
✅ Error messages are clear
✅ Loading indicators work
✅ Scene transitions smooth
✅ Android build compiles successfully

## Next Steps (Phase 2)

After Phase 1 is stable and approved:
- WebSocket connection setup
- Matchmaking system
- PvP gameplay implementation
- Real-time combat features