# Quick Start Guide - Unity PvP Client

Get up and running in 10 minutes! ⚡

## 🚀 Step-by-Step Setup

### 1️⃣ Prerequisites (2 minutes)

Install:
- ✅ Unity Hub
- ✅ Unity 2022.3.62f3 LTS
- ✅ Node.js 18+
- ✅ Docker Desktop

### 2️⃣ Start Backend (2 minutes)

```bash
# Terminal 1: Start databases
cd backend
docker compose up -d

# Terminal 2: Start server
npm install
npm run start:dev
```

Verify: `curl http://localhost:3000` should return "Hello World" or similar

### 3️⃣ Open Unity Project (1 minute)

1. Open Unity Hub
2. Click "Add" → Select `unity-client` folder
3. Open with Unity 2022.3.62f3
4. Wait for packages to load (~1 minute)

### 4️⃣ Create GameConfig (1 minute)

1. In Unity Project panel, go to `Assets/Scripts/Config/`
2. Right-click → Create → PvP → Game Config
3. Name it "GameConfig"
4. Leave default values (localhost URLs)

### 5️⃣ Create 4 Scenes (4 minutes)

Follow `SCENE_SETUP_GUIDE.md` to create:

**Minimal Setup (1 minute each):**

#### Login Scene:
- Create new 2D scene
- Add Canvas
- Add: EmailInput, PasswordInput, LoginButton, RegisterButton
- Add LoginManager GameObject with LoginUI.cs
- Assign references
- Save as `Assets/Scenes/Login.unity`

#### Lobby Scene:
- Create new 2D scene
- Add Canvas
- Add: UsernameText, JoinQueueButton, LeaveQueueButton
- Add LobbyManager with LobbyUI.cs
- Assign references
- Save as `Assets/Scenes/Lobby.unity`

#### Game Scene:
- Create new 2D scene
- Camera size: 60
- Create PlayerShip (blue square sprite)
- Create OpponentShip (red square sprite)
- Add GameManager with GameManager.cs + InputController.cs
- Save as `Assets/Scenes/Game.unity`

#### Result Scene:
- Create new 2D scene
- Add Canvas
- Add: ResultText, BackToLobbyButton
- Add ResultManager with ResultUI.cs
- Save as `Assets/Scenes/Result.unity`

### 6️⃣ Configure Build Settings (30 seconds)

1. File → Build Settings
2. Add all 4 scenes (drag from Project panel)
3. Ensure Login is first

### 7️⃣ Test! (1 minute)

1. Play Login scene in Unity
2. Register new user
3. Should auto-transition to Lobby

---

## ✅ Verification Checklist

After setup, verify:

```
[ ] Backend running on http://localhost:3000
[ ] Unity project opens without errors
[ ] All 4 scenes exist in Assets/Scenes/
[ ] GameConfig exists in Assets/Scripts/Config/
[ ] Login scene plays without errors
[ ] Registration creates user successfully
[ ] Auto-transitions to Lobby scene
```

---

## 🎮 First Match Test

### Single Editor (Solo Test):

1. Play Login scene
2. Register as `player1@test.com`
3. Join queue
4. You'll wait (need 2nd player for match)

### Two Editors (Full Test):

1. **Editor 1:**
   - Login as `player1@test.com`
   - Join queue

2. **Editor 2:**
   - Open Unity project again (second instance)
   - Login as `player2@test.com`
   - Join queue

3. **Both editors:**
   - Match found!
   - Auto-transition to Game
   - See both ships
   - Test movement (WASD)
   - Test fire (Space)
   - Test ability (E)

---

## 🐛 Common Quick Fixes

### "Package Import Failed"
```
Window → Package Manager → Input System → Install
Window → TextMeshPro → Import TMP Essential Resources
```

### "Input System Not Working / Backend Not Enabled"
```
Edit → Project Settings → Player → Other Settings
→ Active Input Handling: Set to "Both" or "Input System Package (New)"
→ Restart Unity Editor after changing this setting
```

### "Scene Not Found"
```
File → Build Settings → Add Open Scenes (while scene is open)
```

### "Reference Not Assigned"
```
Select GameObject in Hierarchy
In Inspector, drag-and-drop UI elements to script references
```

### "Backend Not Responding"
```bash
# Check if running:
curl http://localhost:3000

# Restart:
cd backend
npm run start:dev
```

### "WebSocket Won't Connect"
```
Check Console for errors
Verify GameConfig URLs are correct:
  - http://localhost:3000 (REST)
  - ws://localhost:3000/pvp (WebSocket)
```

---

## 📝 Minimal Scene Templates

Too busy? Use these minimal setups:

### Login Scene (Absolute Minimum):
```
Canvas
├── EmailInput (TMP_InputField)
├── PasswordInput (TMP_InputField)
└── LoginButton (Button)

LoginManager (Empty GameObject)
└── LoginUI.cs (assign 3 references above)
```

### Lobby Scene (Absolute Minimum):
```
Canvas
├── UsernameText (TextMeshProUGUI)
└── JoinQueueButton (Button)

LobbyManager
└── LobbyUI.cs
```

### Game Scene (Absolute Minimum):
```
PlayerShip (Sprite - blue)
OpponentShip (Sprite - red)

GameManager
├── GameManager.cs
└── InputController.cs
```

### Result Scene (Absolute Minimum):
```
Canvas
├── ResultText (TextMeshProUGUI)
└── BackButton (Button)

ResultManager
└── ResultUI.cs
```

---

## 🎯 Next Steps

After Quick Start:

1. **Polish UI:** Add better graphics, animations
2. **Add Effects:** Particle systems for fire/abilities
3. **Add Audio:** Sound effects, music
4. **Test on Android:** Build APK and test on device
5. **Multiplayer Test:** Play with friends!

---

## 📚 Full Documentation

- `README.md` - Complete feature overview
- `SCENE_SETUP_GUIDE.md` - Detailed scene creation
- `TESTING_GUIDE.md` - Comprehensive testing
- Backend docs - See `../backend/MATCHMAKING.md`

---

## 🆘 Help

**Project Health Check:**
- In Unity: PvP → Project Health Check

**Console Errors:**
- Check for colored logs (green = success, red = error)
- Auth events = Magenta
- Network events = Cyan
- Game events = Blue

**Still Stuck?**
1. Check Unity Console
2. Check backend terminal
3. Verify Docker containers running: `docker ps`
4. Review SCENE_SETUP_GUIDE.md

---

## 🎉 Success!

If you can:
- ✅ Register a user
- ✅ See Lobby scene
- ✅ Join queue
- ✅ (With 2nd player) Start a match

**You're all set!** 🚀

Now build amazing PvP experiences! 🎮

---

**Estimated Total Time:** ~10 minutes  
**Skill Level:** Beginner to Intermediate Unity
