# Quick Start Guide - Phase 2 Unity Client

## 🚀 Get Started in 10 Minutes

This guide will get you up and running with the Phase 2 Unity client.

---

## Prerequisites

- Unity Hub installed
- Unity 2022.3.62f3 LTS
- Backend server running (see `/backend/README.md`)
- Code editor (VS Code, Rider, or Visual Studio)

---

## Step 1: Open Project

1. Open Unity Hub
2. Click "Add" → Select `unity-client` folder
3. Select Unity version: 2022.3.62f3
4. Click on project to open

**Expected:** Unity Editor opens with project loaded

---

## Step 2: Create Ship Prefab

1. **Create GameObject:**
   - Right-click in Hierarchy → 2D Object → Sprite → Square
   - Rename to "Ship"

2. **Configure Sprite:**
   - Inspector → Sprite Renderer → Color: Cyan
   - Transform → Scale: (1, 1, 1)

3. **Add ShipView Component:**
   - Inspector → Add Component → Search "ShipView"
   - Add component

4. **Create Health/Shield Bars:**
   - Right-click Ship → UI → Slider (Health)
   - Right-click Ship → UI → Slider (Shield)
   - Configure sliders:
     - Max Value: 100
     - Whole Numbers: checked
     - Value: 100

5. **Assign References:**
   - ShipView component:
     - Ship Sprite: Drag SpriteRenderer
     - Health Bar Fill: Drag health slider fill
     - Shield Bar Fill: Drag shield slider fill

6. **Create Prefab:**
   - Drag Ship from Hierarchy to `Assets/Prefabs/`
   - Delete from Hierarchy

---

## Step 3: Create Bullet Prefab

1. **Create GameObject:**
   - Right-click in Hierarchy → 2D Object → Sprite → Circle
   - Rename to "Bullet"
   - Transform → Scale: (0.2, 0.2, 0.2)

2. **Configure Sprite:**
   - Inspector → Sprite Renderer → Color: Yellow

3. **Add TrailRenderer:**
   - Inspector → Add Component → Trail Renderer
   - Configure:
     - Time: 0.3
     - Width: 0.1
     - Color: Yellow (fade to transparent)

4. **Add BulletView Component:**
   - Inspector → Add Component → Search "BulletView"
   - Add component

5. **Assign References:**
   - BulletView component:
     - Bullet Sprite: Drag SpriteRenderer
     - Trail: Drag TrailRenderer

6. **Create Prefab:**
   - Drag Bullet from Hierarchy to `Assets/Prefabs/`
   - Delete from Hierarchy

---

## Step 4: Setup Game Scene

1. **Open Game Scene:**
   - Project window → Assets/Scenes/Game.unity
   - Double-click to open

2. **Create GameController:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "GameController"
   - Add Component → Search "GameController"
   - Assign prefabs:
     - Ship Prefab: Drag Ship from Prefabs folder
     - Bullet Prefab: Drag Bullet from Prefabs folder

3. **Create InputSender:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "InputSender"
   - Add Component → Search "InputSender"

4. **Create Canvas (HUD):**
   - Right-click in Hierarchy → UI → Canvas
   - Rename to "MatchHUD"
   - Add Component → Search "MatchHUD"

5. **Create HUD Elements:**
   - **Timer:**
     - Right-click Canvas → UI → Text - TextMeshPro
     - Rename to "TimerText"
     - Position: Top center
     - Text: "00:00"
     - Font Size: 36

   - **Player Health Bar:**
     - Right-click Canvas → UI → Slider
     - Rename to "PlayerHealthBar"
     - Position: Bottom left
     - Max Value: 100

   - **Opponent Health Bar:**
     - Right-click Canvas → UI → Slider
     - Rename to "OpponentHealthBar"
     - Position: Top left
     - Max Value: 100

   - **Connection Indicator:**
     - Right-click Canvas → UI → Image
     - Rename to "ConnectionIndicator"
     - Position: Top right
     - Color: Green
     - Size: (30, 30)

6. **Assign MatchHUD References:**
   - Select MatchHUD GameObject
   - In Inspector, MatchHUD component:
     - Player Health Bar: Drag PlayerHealthBar
     - Opponent Health Bar: Drag OpponentHealthBar
     - Match Timer: Drag TimerText
     - Connection Status Image: Drag ConnectionIndicator

7. **Save Scene:**
   - File → Save Scene (Ctrl+S)

---

## Step 5: Configure Backend URL

1. **Open SocketClient.cs:**
   - Project window → Assets/Network/SocketClient.cs
   - Double-click to open in code editor

2. **Check Server URL (Line 11):**
   ```csharp
   private string serverUrl = "ws://localhost:3000/pvp";
   ```

3. **For production, change to:**
   ```csharp
   private string serverUrl = "wss://your-server.com/pvp";
   ```

---

## Step 6: Test in Play Mode

1. **Ensure Backend is Running:**
   ```bash
   cd backend
   npm run start:dev
   ```

2. **Press Play in Unity:**
   - Click Play button (or F5)

3. **Test Flow:**
   - Login/Register
   - Join Queue
   - Match starts → Game scene loads
   - Ships should appear
   - Press WASD to move
   - Press Space to fire

4. **Expected Results:**
   - ✅ Ships visible on screen
   - ✅ Movement works
   - ✅ Bullets fire
   - ✅ Health bars update
   - ✅ Timer counts
   - ✅ Connection indicator is green

---

## Step 7: Build for Windows

1. **Open Build Settings:**
   - File → Build Settings

2. **Select Platform:**
   - PC, Mac & Linux Standalone
   - Target Platform: Windows
   - Architecture: x86_64

3. **Player Settings:**
   - Company Name: Your Company
   - Product Name: PvP Ship Battle
   - Version: 1.0.0

4. **Build:**
   - Click "Build"
   - Choose output folder
   - Wait for build to complete

5. **Test Executable:**
   - Run the .exe file
   - Test full gameplay flow

---

## Step 8: Build for Android (Optional)

1. **Install Android Build Support:**
   - Unity Hub → Installs → Add Modules
   - Check "Android Build Support"
   - Install

2. **Open Build Settings:**
   - File → Build Settings
   - Select Android
   - Click "Switch Platform"

3. **Player Settings:**
   - Company Name: Your Company
   - Package Name: com.yourcompany.shipbattle
   - Minimum API Level: Android 7.0 (API 24)

4. **Build:**
   - Click "Build And Run"
   - Choose output folder
   - Wait for build and install

5. **Test on Device:**
   - Game should launch on Android device
   - Test touch controls
   - Verify joystick works

---

## 🐛 Troubleshooting

### Issue: Prefabs not showing
**Solution:** Verify prefabs are assigned in GameController component

### Issue: No movement
**Solution:** Check InputSender component is in scene and active

### Issue: Connection failed
**Solution:** Verify backend is running on localhost:3000

### Issue: No snapshots
**Solution:** Check Unity console for errors, verify WebSocket connection

### Issue: Health bars not updating
**Solution:** Check MatchHUD component references are assigned

---

## 📝 Next Steps

After completing quick start:

1. **Add Visual Polish:**
   - Replace placeholder sprites with actual ship art
   - Add particle effects
   - Add animated backgrounds

2. **Add Audio:**
   - Background music
   - Weapon fire sounds
   - Hit/damage sounds

3. **Optimize:**
   - Profile with Unity Profiler
   - Optimize draw calls
   - Test on various devices

4. **Deploy:**
   - Deploy backend to cloud
   - Update WebSocket URL
   - Build and distribute

---

## 📚 Additional Resources

- **Full Documentation:** [README.md](README.md)
- **Integration Guide:** [/INTEGRATION_GUIDE.md](../INTEGRATION_GUIDE.md)
- **Completion Report:** [/PHASE_2_COMPLETION_REPORT.md](../PHASE_2_COMPLETION_REPORT.md)
- **Backend Docs:** [/backend/README.md](../backend/README.md)

---

## ✅ Success Checklist

- [ ] Unity project opens without errors
- [ ] Ship prefab created and assigned
- [ ] Bullet prefab created and assigned
- [ ] Game scene configured
- [ ] HUD elements created
- [ ] Play mode works
- [ ] Can move with WASD
- [ ] Can fire with Space
- [ ] Health bars visible
- [ ] Timer counts
- [ ] Windows build succeeds
- [ ] Game is playable!

---

**Happy Coding! 🎮**
