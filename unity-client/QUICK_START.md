# Unity Client - Quick Start Checklist

## ✅ Pre-Flight Checklist (Do This First!)

### Step 1: Open Unity Project
- [ ] Open Unity Hub
- [ ] Add project from folder
- [ ] Open with Unity 2022.3+
- [ ] Wait for package resolution (may take a few minutes)

### Step 2: Install DOTween (REQUIRED)
- [ ] Open Unity Asset Store window
- [ ] Search for "DOTween" (Free)
- [ ] Download and Import
- [ ] Go to `Tools > Demigiant > DOTween Utility Panel`
- [ ] Click "Setup DOTween"
- [ ] Click "Apply"

**⚠️ If you skip this:** You'll see errors like "The type or namespace name 'DG' could not be found"

### Step 3: Import TextMeshPro Resources (REQUIRED)
- [ ] Go to `Window > TextMeshPro > Import TMP Essential Resources`
- [ ] Click "Import"
- [ ] Wait for import to complete

**⚠️ If you skip this:** You'll see errors like "The type or namespace name 'TMPro' could not be found"

### Step 4: Verify Packages
- [ ] Open `Window > Package Manager`
- [ ] Check these packages are installed:
  - ✅ Input System (1.7.0)
  - ✅ TextMeshPro (3.0.6)
  - ✅ Unity UI (1.0.0)
  - ✅ Addressables (1.19.19)
  - ✅ Universal RP (14.0.7)
  - ✅ Newtonsoft Json (3.2.1)
  - ✅ NativeWebSocket (custom)

**If NativeWebSocket is missing:**
- Click `+` in Package Manager
- Select "Add package from git URL"
- Enter: `https://github.com/endel/NativeWebSocket.git#upm`
- Click "Add"

### Step 5: Verify Compilation
- [ ] Check Unity Console (no red errors should appear)
- [ ] If errors appear, read them and refer to UNITY_SETUP_INSTRUCTIONS.md

### Step 6: Test Run
- [ ] Press Play in any scene
- [ ] Console should show: "Attempting WebSocket connection to: ws://localhost:3000/pvp?token=..."
- [ ] If no backend running, connection will fail (expected)

---

## 🚀 Running with Backend

### Start Backend Server
```bash
cd backend
npm install
npm run start:dev
```

Server should start on `http://localhost:3000`

### Test in Unity
1. Press Play in Login Scene
2. Register a new user
3. Login with credentials
4. Join matchmaking queue
5. (Need 2 clients to get matched)

---

## 🐛 Troubleshooting

### Error: "DG namespace not found"
➡️ Install DOTween from Asset Store and run Setup Utility Panel

### Error: "TMPro namespace not found"
➡️ Import TMP Essential Resources

### Error: "NativeWebSocket namespace not found"
➡️ Add NativeWebSocket package from git URL in Package Manager

### Warning: "WebSocket connection failed"
➡️ Normal if backend is not running. Start backend first.

---

## 📚 Full Documentation

- [UNITY_SETUP_INSTRUCTIONS.md](UNITY_SETUP_INSTRUCTIONS.md) - Complete setup guide
- [MIGRATION_COMPLETE.md](MIGRATION_COMPLETE.md) - What changed and why
- [README.md](README.md) - Project overview and architecture

---

## ✨ You're Ready When...

✅ Unity Console is clean (no red errors)  
✅ Press Play works without crashes  
✅ WebSocket connection attempt logged  
✅ Login scene loads properly  
✅ UI renders correctly  

**🎉 Enjoy your production-ready Unity client!**
