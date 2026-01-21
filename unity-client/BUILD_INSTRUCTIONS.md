# Build Instructions - Unity PvP Client

Complete guide for building the Unity client for different platforms.

## 📋 Prerequisites

Before building:

- ✅ Unity 2022.3.62f3 installed
- ✅ All 4 scenes created and added to Build Settings
- ✅ All scripts compile without errors
- ✅ Tested in Unity Editor successfully

---

## 🖥️ Windows Build (PC Standalone)

### Configuration:

1. **Open Build Settings:**
   - File → Build Settings
   - Platform: PC, Mac & Linux Standalone
   - Target Platform: Windows
   - Architecture: x86_64

2. **Player Settings:**
   - Company Name: `YourCompany`
   - Product Name: `PvP Ship Battle`
   - Version: `1.0.0`
   - Default Icon: (optional)

3. **Quality Settings:**
   - Edit → Project Settings → Quality
   - Default Quality: Medium or High
   - VSync: Don't Sync (for better performance)

4. **Backend Configuration:**
   - Update GameConfig URLs if not using localhost
   - REST API: `http://your-server.com:3000`
   - WebSocket: `ws://your-server.com:3000/pvp`

### Build Steps:

```
1. File → Build Settings
2. Verify all 4 scenes are listed
3. Click "Build"
4. Choose output folder (e.g., "Builds/Windows")
5. Wait for build to complete
6. Run .exe file to test
```

### Output:
- `PvP Ship Battle.exe`
- `PvP Ship Battle_Data/` folder
- `UnityPlayer.dll`
- `UnityCrashHandler64.exe`

### Distribution:
- Zip entire build folder
- Share with players
- Minimum: Windows 7 or later

---

## 📱 Android Build (APK)

### Prerequisites:

1. **Install Android Build Support:**
   - Unity Hub → Unity 2022.3.62f3 → Add Modules
   - Select: Android Build Support
   - Include: Android SDK & NDK Tools, OpenJDK

2. **Configure Android SDK:**
   - Edit → Preferences → External Tools
   - Set Android SDK path (or use Unity's)
   - Set Android NDK path (or use Unity's)
   - Set JDK path (or use Unity's)

### Player Settings:

1. **Open Player Settings:**
   - Edit → Project Settings → Player
   - Select Android tab (Android icon)

2. **Company & Product:**
   - Company Name: `YourCompany`
   - Product Name: `PvP Ship Battle`
   - Package Name: `com.yourcompany.pvpgame`
   - Version: `1.0.0`
   - Bundle Version Code: `1`

3. **Icon:**
   - Default Icon: (provide 1024x1024 PNG)
   - Adaptive Icon: (optional for modern Android)

4. **Other Settings:**
   - **Minimum API Level:** Android 7.0 'Nougat' (API level 24)
   - **Target API Level:** Android 13 (API level 33)
   - **Scripting Backend:** IL2CPP (for performance)
   - **ARM64:** ✅ Enabled (required for Play Store)

5. **Publishing Settings:**
   - Create new keystore (for release builds)
   - Project Keystore → Create New
   - Password: (save securely!)
   - Project Key Alias: `pvp-game-key`

6. **Configuration:**
   - Internet Access: **Require**
   - Write Permission: Auto
   - Install Location: Auto

7. **Backend URLs:**
   - Change from localhost to server IP/domain
   - Example: `http://192.168.1.100:3000` (local network)
   - Or: `https://your-server.com` (production)

### Build Steps:

```
1. File → Build Settings
2. Platform → Android
3. Click "Switch Platform" (first time only)
4. Build Type:
   - Development: "Export Project" (for Android Studio)
   - Release: "Build" (for APK)
5. Click "Build" or "Build and Run"
6. Choose output location
7. Wait for build (5-15 minutes)
```

### Output:
- `PvP Ship Battle.apk` (or .aab for Play Store)

### Installation:

**Via USB:**
```bash
adb install "PvP Ship Battle.apk"
```

**Via File Transfer:**
1. Copy APK to device
2. Open file manager
3. Tap APK
4. Allow "Install from Unknown Sources"
5. Install

### Testing on Android:

1. **Controls:**
   - Touch screen to move (drag)
   - Tap to fire
   - Swipe to use ability

2. **Network:**
   - Ensure device on same network as server
   - Or use public server URL

3. **Permissions:**
   - Internet permission is automatic

---

## 🌐 WebGL Build (Browser)

**Note:** WebSockets may have issues in WebGL builds. Not recommended for this project unless using WebRTC or Socket.IO adapters.

### Configuration:

1. **Build Settings:**
   - Platform: WebGL
   - Switch Platform

2. **Player Settings:**
   - Compression Format: Gzip (for smaller builds)
   - Memory Size: 256 MB minimum

3. **Backend:**
   - Must use WSS (secure WebSocket) for HTTPS sites
   - CORS must allow your domain

### Build Steps:

```
1. File → Build Settings
2. Platform → WebGL
3. Click "Build"
4. Choose output folder
5. Wait for build (10-30 minutes)
```

### Hosting:

```bash
# Simple HTTP server (Python)
cd build-folder
python -m http.server 8000

# Or use Node.js
npx http-server -p 8000
```

**Production Hosting:**
- Netlify
- Vercel
- GitHub Pages
- AWS S3 + CloudFront

---

## 🍎 iOS Build (iPhone/iPad)

**Requirements:**
- macOS with Xcode
- Apple Developer Account ($99/year)

### Build Settings:

1. **Platform:**
   - File → Build Settings → iOS
   - Switch Platform

2. **Player Settings:**
   - Bundle Identifier: `com.yourcompany.pvpgame`
   - Minimum iOS Version: 12.0
   - Architecture: ARM64
   - Camera Usage: None
   - Microphone Usage: None

3. **Build:**
   - Click "Build"
   - Output Xcode project

4. **Xcode:**
   - Open project in Xcode
   - Configure signing & capabilities
   - Add "App Transport Security" for HTTP (if not HTTPS)
   - Build and deploy to device

---

## 🔧 Build Optimization

### Reduce Build Size:

1. **Managed Stripping Level:**
   - Player Settings → Other Settings
   - Stripping Level: Medium or High

2. **Texture Compression:**
   - Override for platform-specific compression
   - Android: ASTC
   - iOS: ASTC

3. **Asset Bundles:**
   - Move large assets to Addressables
   - Download on demand

4. **Audio Compression:**
   - Compress audio files
   - Use Vorbis for music, ADPCM for SFX

### Improve Performance:

1. **Graphics:**
   - Use URP (Universal Render Pipeline) if needed
   - Lower quality settings for mobile

2. **Physics:**
   - Disable if not needed
   - Reduce fixed timestep

3. **Code:**
   - Enable IL2CPP (faster than Mono)
   - Enable managed code stripping

---

## 🚀 Deployment Checklist

### Before Building:

- [ ] All scenes created and working
- [ ] No console errors
- [ ] Backend URLs updated (not localhost)
- [ ] Tested in Unity Editor
- [ ] Version number incremented
- [ ] Icon set
- [ ] Product name configured

### For Android:

- [ ] Package name set (lowercase, no spaces)
- [ ] API levels configured (24 minimum, 33 target)
- [ ] Keystore created and password saved
- [ ] ARM64 enabled
- [ ] Internet permission enabled
- [ ] Tested on physical device
- [ ] Performance acceptable

### For Windows:

- [ ] Company name set
- [ ] Product name set
- [ ] Icon set
- [ ] Quality settings configured
- [ ] Tested build executable
- [ ] All DLLs included

### For Production:

- [ ] SSL certificate for backend (WSS not WS)
- [ ] CORS configured for production domain
- [ ] Server IP/domain updated in build
- [ ] Privacy policy URL (for stores)
- [ ] Terms of service
- [ ] Age rating determined

---

## 🐛 Common Build Issues

### "Build Failed" Error

**Check:**
- All scripts compile without errors
- All scenes are valid
- No missing references in Inspector
- Enough disk space

### "IL2CPP Error"

**Fix:**
- Update Unity to latest 2022.3.x patch
- Check C++ compiler is installed
- Restart Unity

### APK Won't Install

**Fix:**
- Check API level matches device
- Enable "Install from Unknown Sources"
- Ensure package name is unique
- Rebuild with different version code

### WebSocket Connection Fails

**Fix:**
- Change localhost to server IP
- Check firewall settings
- Verify backend is accessible from device
- Check CORS settings on backend

### Game Runs Slow on Android

**Fix:**
- Lower quality settings
- Reduce screen resolution
- Disable VSync
- Use IL2CPP scripting backend
- Optimize sprites (compress textures)

---

## 📊 Build Size Estimates

### Windows:
- Minimal: ~50-80 MB
- With assets: ~100-200 MB

### Android:
- Minimal APK: ~40-60 MB
- With assets: ~80-150 MB
- AAB (for Play Store): ~30-50 MB

### WebGL:
- Minimal: ~30-50 MB (compressed)
- With assets: ~60-100 MB

---

## 🎯 Platform-Specific Notes

### Windows:
- ✅ Easiest to build and test
- ✅ Best performance
- ✅ Keyboard controls ideal
- ✅ Quick iteration time

### Android:
- ⚠️ Longer build times (5-15 min)
- ⚠️ Need physical device for testing
- ✅ Touch controls required
- ⚠️ Performance varies by device
- ⚠️ Network setup more complex

### WebGL:
- ⚠️ Longest build times (10-30 min)
- ⚠️ WebSocket support limited
- ⚠️ Performance varies by browser
- ✅ Instant distribution (URL)
- ⚠️ Requires web hosting

### iOS:
- ⚠️ Requires macOS + Xcode
- ⚠️ Requires Apple Developer account ($99/year)
- ⚠️ Review process for App Store
- ✅ Best performance on devices
- ⚠️ Most restrictive platform

---

## 📱 Distribution Options

### Direct Distribution:
- ✅ Share APK/EXE directly
- ✅ No approval process
- ✅ Instant updates
- ⚠️ Manual installation
- ⚠️ No auto-updates

### Google Play Store:
- ✅ Wide reach
- ✅ Auto-updates
- ✅ Trusted source
- ⚠️ Review process (1-7 days)
- ⚠️ $25 one-time fee
- ⚠️ Requires AAB format
- ⚠️ Privacy policy required

### Steam (PC):
- ✅ Large PC gaming audience
- ✅ Auto-updates
- ✅ Community features
- ⚠️ $100 one-time fee per game
- ⚠️ Requires Steamworks integration
- ⚠️ Review process

### itch.io (PC/Android):
- ✅ Free distribution
- ✅ Indie-friendly
- ✅ Pay-what-you-want pricing
- ✅ No review process
- ⚠️ Smaller audience

---

## ✅ Final Build Checklist

**Before Submitting:**

- [ ] Game tested end-to-end
- [ ] All 4 scenes work correctly
- [ ] Authentication works
- [ ] Matchmaking works
- [ ] Gameplay smooth
- [ ] Match ends correctly
- [ ] Results display correctly
- [ ] No crashes
- [ ] No console errors
- [ ] Performance acceptable
- [ ] Backend stable
- [ ] URLs point to production server
- [ ] Privacy policy ready (if publishing)
- [ ] Screenshots/videos prepared
- [ ] App description written
- [ ] Version number correct

---

## 🎉 You're Ready to Build!

Choose your target platform and follow the steps above.

For most users, start with **Windows** for testing, then **Android** for mobile.

Good luck! 🚀

---

**Need Help?**
- Check Unity Console for errors
- Review Player Settings
- Verify Backend connectivity
- Test in Unity Editor first
