# Unity Scenes Setup Guide

Complete step-by-step instructions for creating all 4 required scenes.

## 🎬 Scene 1: Login Scene

### Steps:

1. **Create Scene:**
   - File → New Scene → 2D
   - Save as `Assets/Scenes/Login.unity`

2. **Setup Canvas:**
   - Right-click Hierarchy → UI → Canvas
   - Canvas Scaler → Scale With Screen Size
   - Reference Resolution: 1920x1080

3. **Create Login Panel:**
   ```
   Canvas
   └── LoginPanel (Panel)
       ├── TitleText (TextMeshProUGUI) - "PvP Ship Battle"
       ├── EmailInput (TMP_InputField)
       │   └── Placeholder: "Email"
       ├── PasswordInput (TMP_InputField)
       │   └── Placeholder: "Password"
       │   └── Content Type: Password
       ├── LoginButton (Button)
       │   └── Text: "Login"
       └── RegisterButton (Button)
           └── Text: "Register"
   ```

4. **Create Register Panel (Initially Hidden):**
   ```
   Canvas
   └── RegisterPanel (Panel)
       └── UsernameInput (TMP_InputField)
           └── Placeholder: "Username"
   ```
   - Uncheck "Active" checkbox in Inspector

5. **Create Error Panel:**
   ```
   Canvas
   └── ErrorText (TextMeshProUGUI)
       └── Text: "" (empty)
       └── Color: Red
       └── Font Size: 24
   ```
   - Uncheck "Active"

6. **Create Loading Panel:**
   ```
   Canvas
   └── LoadingPanel (Panel)
       └── LoadingText (TextMeshProUGUI)
           └── Text: "Connecting..."
   ```
   - Uncheck "Active"

7. **Create Manager GameObject:**
   - Right-click Hierarchy → Create Empty
   - Name: "LoginManager"
   - Add Component → Scripts → Auth → LoginUI
   - Assign all references:
     - Email Input
     - Username Input
     - Password Input
     - Login Button
     - Register Button
     - Error Text
     - Loading Panel
     - Register Panel

8. **Create EventSystem** (if not exists):
   - Right-click Hierarchy → UI → Event System

9. **Save Scene**

---

## 🏛️ Scene 2: Lobby Scene

### Steps:

1. **Create Scene:**
   - File → New Scene → 2D
   - Save as `Assets/Scenes/Lobby.unity`

2. **Setup Canvas**

3. **Create Player Info Panel:**
   ```
   Canvas
   └── PlayerInfoPanel (Panel - top-left)
       ├── UsernameText (TextMeshProUGUI) - Font Size: 36
       ├── RatingText (TextMeshProUGUI) - "Rating: 1000"
       └── StatsText (TextMeshProUGUI) - "Wins: 0 | Losses: 0"
   ```

4. **Create Queue Controls:**
   ```
   Canvas
   └── QueueControlPanel (Panel - center)
       ├── JoinQueueButton (Button - Large)
       │   └── Text: "Join Queue"
       └── LeaveQueueButton (Button - Large)
           └── Text: "Leave Queue"
   ```
   - LeaveQueueButton: Uncheck "Active"

5. **Create Queue Status Panel:**
   ```
   Canvas
   └── QueuePanel (Panel - center)
       ├── QueueStatusText (TextMeshProUGUI)
       │   └── "Finding opponent..."
       └── QueuePositionText (TextMeshProUGUI)
           └── "Position: 1"
   ```
   - Uncheck "Active"

6. **Create Match Found Panel:**
   ```
   Canvas
   └── MatchFoundPanel (Panel - full screen overlay)
       ├── BackgroundImage (Image - semi-transparent)
       ├── TitleText (TextMeshProUGUI) - "MATCH FOUND!"
       ├── OpponentNameText (TextMeshProUGUI)
       └── OpponentRatingText (TextMeshProUGUI)
   ```
   - Uncheck "Active"

7. **Create Logout Button:**
   ```
   Canvas
   └── LogoutButton (Button - top-right)
       └── Text: "Logout"
   ```

8. **Create Loading Panel:**
   ```
   Canvas
   └── LoadingPanel (Panel)
       └── Text: "Connecting to server..."
   ```
   - Uncheck "Active"

9. **Create Manager GameObject:**
   - Create Empty → "LobbyManager"
   - Add Component → Scripts → UI → LobbyUI
   - Assign all references

10. **Add EventSystem**

11. **Save Scene**

---

## 🎮 Scene 3: Game Scene

### Steps:

1. **Create Scene:**
   - File → New Scene → 2D
   - Save as `Assets/Scenes/Game.unity`

2. **Setup Camera:**
   - Select Main Camera
   - Orthographic Size: 60 (to see 100x100 map)
   - Background: Dark blue/black

3. **Create Map Boundaries (Optional):**
   ```
   Hierarchy
   └── MapBoundaries (Empty GameObject)
       ├── TopWall (Sprite - line at y=50)
       ├── BottomWall (Sprite - line at y=-50)
       ├── LeftWall (Sprite - line at x=-50)
       └── RightWall (Sprite - line at x=50)
   ```

4. **Create Player Ship:**
   ```
   Hierarchy
   └── PlayerShip (GameObject)
       ├── SpriteRenderer (Blue square sprite, scale 2x2)
       ├── Scripts/Game/ShipController.cs
       └── HealthBarCanvas (Canvas - World Space)
           └── HealthBar (Slider)
               ├── Background (Image)
               ├── Fill Area
               │   └── Fill (Image - Green to Red)
               └── HealthText (TextMeshProUGUI)
   ```
   - Position: (20, 0, 0)
   - Add Component → HealthDisplay to HealthBar
   - Assign references in ShipController

5. **Create Opponent Ship:**
   - Duplicate PlayerShip
   - Rename to "OpponentShip"
   - Change sprite color to Red
   - Position: (80, 0, 0)

6. **Create Game UI Canvas:**
   ```
   Canvas
   └── GameUIPanel
       ├── TopBar (Panel)
       │   ├── PlayerNameText (TextMeshProUGUI - left)
       │   ├── MatchIdText (TextMeshProUGUI - center)
       │   ├── TimerText (TextMeshProUGUI - center)
       │   └── OpponentNameText (TextMeshProUGUI - right)
       ├── StatusText (TextMeshProUGUI - center, large)
       └── AbilityUI (Panel - bottom-right)
           ├── CooldownImage (Image - radial fill)
           └── CooldownText (TextMeshProUGUI)
   ```

7. **Create Game Manager:**
   ```
   Hierarchy
   └── GameManager (Empty GameObject)
       ├── Scripts/Game/GameManager.cs
       ├── Scripts/Input/InputController.cs
       ├── Scripts/Game/WeaponController.cs
       └── Scripts/Game/AbilityController.cs
   ```
   - Assign references:
     - Player Ship → PlayerShip
     - Opponent Ship → OpponentShip
     - Input Controller → self
     - Weapon Controller → self
     - Ability Controller → self
   - In AbilityController:
     - Assign Cooldown Image
     - Assign Cooldown Text

8. **Create Game UI Manager:**
   ```
   Hierarchy
   └── GameUIManager (Empty GameObject)
       └── Scripts/UI/GameUI.cs
   ```
   - Assign all UI text references
   - Assign GameManager reference

9. **Add EventSystem**

10. **Save Scene**

---

## 🏆 Scene 4: Result Scene

### Steps:

1. **Create Scene:**
   - File → New Scene → 2D
   - Save as `Assets/Scenes/Result.unity`

2. **Setup Canvas**

3. **Create Result Panel:**
   ```
   Canvas
   └── ResultPanel (Panel - full screen)
       ├── ResultBackground (Image - semi-transparent)
       ├── ResultText (TextMeshProUGUI - huge)
       │   └── Text: "VICTORY!" or "DEFEAT"
       │   └── Font Size: 120
       │   └── Alignment: Center
       ├── EloChangeText (TextMeshProUGUI)
       │   └── Text: "+25 ELO" or "-25 ELO"
       │   └── Font Size: 60
       └── Divider (Image - line)
   ```

4. **Create Stats Panel:**
   ```
   Canvas
   └── StatsPanel (Panel - center)
       ├── PlayerNameText (TextMeshProUGUI)
       ├── RatingText (TextMeshProUGUI)
       ├── WinsText (TextMeshProUGUI)
       └── LossesText (TextMeshProUGUI)
   ```

5. **Create Back Button:**
   ```
   Canvas
   └── BackToLobbyButton (Button - bottom-center)
       └── Text: "Back to Lobby"
   ```

6. **Create Manager:**
   ```
   Hierarchy
   └── ResultManager (Empty GameObject)
       └── Scripts/UI/ResultUI.cs
   ```
   - Assign all references

7. **Add EventSystem**

8. **Save Scene**

---

## 🎯 Build Settings

After creating all scenes:

1. **Open Build Settings:**
   - File → Build Settings

2. **Add Scenes:**
   - Click "Add Open Scenes" while each scene is open
   - OR drag scenes from Project panel
   - Order:
     1. Login
     2. Lobby
     3. Game
     4. Result

3. **Platform Settings:**
   - **PC/Mac/Linux Standalone:**
     - Target Platform: Windows
     - Architecture: x86_64
   
   - **Android:**
     - Minimum API Level: 24
     - Target API Level: 33
     - Install Location: Auto
     - Write Permission: External (SDCard)
     - Internet Access: Require
     - In Player Settings:
       - Package Name: com.yourcompany.pvpgame
       - Version: 1.0.0

4. **Player Settings:**
   - Company Name: Your Company
   - Product Name: PvP Ship Battle
   - Default Icon: (optional)

5. **Quality Settings:**
   - Edit → Project Settings → Quality
   - Set VSync Count: Don't Sync (for best performance)

---

## 📦 Creating Prefabs (Optional)

### Ship Prefab:
1. Drag PlayerShip from Hierarchy to Assets/Prefabs/
2. This creates reusable ship prefab

### Bullet Prefab (Optional):
1. Create → 2D Object → Sprite → Circle
2. Scale to 0.5
3. Add Trail Renderer (optional)
4. Save to Assets/Prefabs/Bullet.prefab

### Fire Effect Prefab (Optional):
1. Create → Effects → Particle System
2. Configure for muzzle flash
3. Save to Assets/Prefabs/FireEffect.prefab

---

## 🎨 Creating Simple Sprites

Since we don't have external art assets:

### Ship Sprite:
1. Create → 2D → Sprites → Square
2. Or use built-in sprite: "UISprite" or "Knob"

### Bullet Sprite:
1. Create → 2D → Sprites → Circle

### Custom Sprites:
1. Create 64x64 PNG in any image editor
2. Draw simple ship shape (triangle/arrow)
3. Import to Unity
4. Set Texture Type: Sprite (2D and UI)

---

## ✅ Validation Checklist

After setting up all scenes:

### Login Scene:
- [ ] All input fields present
- [ ] Buttons have click listeners
- [ ] LoginUI script attached and references assigned
- [ ] Error text is hidden by default
- [ ] Loading panel is hidden by default

### Lobby Scene:
- [ ] Player info displays correctly
- [ ] Queue buttons toggle visibility
- [ ] Match found panel is hidden
- [ ] LobbyUI script has all references
- [ ] Logout button works

### Game Scene:
- [ ] Both ships visible
- [ ] Health bars display
- [ ] Camera size = 60
- [ ] GameManager has all references
- [ ] Input controller attached
- [ ] Ability UI present

### Result Scene:
- [ ] Result text large and centered
- [ ] Stats panel shows all info
- [ ] Back button present
- [ ] ResultUI script configured

### Build Settings:
- [ ] All 4 scenes added in correct order
- [ ] Login is scene 0 (first)
- [ ] Platform selected
- [ ] Player settings configured

---

## 🐛 Common Issues & Fixes

### Issue: "TextMeshPro Not Found"
**Fix:** Window → TextMeshPro → Import TMP Essential Resources

### Issue: "InputSystem Not Found"
**Fix:** Window → Package Manager → Input System → Install

### Issue: "Scene Reference Missing"
**Fix:** File → Build Settings → Add Open Scenes

### Issue: "UI Not Visible"
**Fix:** 
- Check Canvas Render Mode = Screen Space - Overlay
- Check UI Layer is not hidden
- Check Canvas Scaler settings

### Issue: "Scripts Not Compiling"
**Fix:**
- Window → General → Console (check errors)
- Ensure all using statements are correct
- Ensure Unity version is 2022.3.62f3

---

## 🎓 Tips

1. **Use Prefabs:** Convert ships to prefabs after setting up
2. **Anchor UI:** Set anchors on UI elements for responsive design
3. **Color Code:** Use different colors for player vs opponent
4. **Test Frequently:** Test each scene individually
5. **Version Control:** Commit after each scene is complete

---

**Now you're ready to build the Unity client!** 🚀

Open Unity 2022.3.62f3 and follow these steps to create all 4 scenes.
