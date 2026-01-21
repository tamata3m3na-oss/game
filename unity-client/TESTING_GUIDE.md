# Unity Client Testing Guide

Complete testing guide for validating the Unity client with the backend.

## 🎯 Prerequisites

Before testing:

1. ✅ Backend server running on `http://localhost:3000`
2. ✅ PostgreSQL and Redis running (via Docker Compose)
3. ✅ All 4 Unity scenes created
4. ✅ Unity project opens without errors

## 🧪 Test Plan

### Phase 1: Backend Connectivity Test

#### Test 1.1: REST API Connection

**Objective:** Verify Unity can reach the backend REST API

**Steps:**
1. Start the backend server:
   ```bash
   cd backend
   npm run start:dev
   ```

2. Verify server is running:
   ```bash
   curl http://localhost:3000
   ```

3. In Unity, check Console for connection logs

**Expected Result:** No connection errors

---

#### Test 1.2: WebSocket Connection

**Objective:** Verify WebSocket connectivity

**Steps:**
1. In Unity Editor, open Login scene
2. Play the scene
3. Check Console for WebSocket connection logs

**Expected Result:** 
- "WebSocket connected" message in cyan
- No connection errors

---

### Phase 2: Authentication Flow Test

#### Test 2.1: User Registration

**Objective:** Create a new user account

**Steps:**
1. Play Login scene
2. Click "Register" button
3. Enter:
   - Email: `test1@example.com`
   - Username: `TestPlayer1`
   - Password: `password123`
4. Click "Create Account"

**Expected Result:**
- ✅ "Registration successful" in green
- ✅ Automatically transition to Lobby scene
- ✅ Token saved in PlayerPrefs
- ✅ User info displayed in Lobby

---

#### Test 2.2: User Login

**Objective:** Login with existing credentials

**Steps:**
1. In Unity, go to Edit → Clear All PlayerPrefs
2. Play Login scene
3. Enter credentials from Test 2.1
4. Click "Login"

**Expected Result:**
- ✅ "Login successful" message
- ✅ Transition to Lobby scene
- ✅ Correct username and stats shown

---

#### Test 2.3: Auto-Login

**Objective:** Verify token persistence

**Steps:**
1. Complete Test 2.2
2. Stop play mode
3. Start play mode again (don't clear PlayerPrefs)

**Expected Result:**
- ✅ "Auto-login successful" message
- ✅ Direct transition to Lobby (skip login screen)
- ✅ User data loaded correctly

---

#### Test 2.4: Logout

**Objective:** Verify logout clears session

**Steps:**
1. In Lobby scene, click "Logout" button

**Expected Result:**
- ✅ Return to Login scene
- ✅ Tokens cleared
- ✅ WebSocket disconnected
- ✅ Next login requires credentials

---

### Phase 3: Matchmaking Test

#### Test 3.1: Join Queue (Single Player)

**Objective:** Test queue join with one player

**Steps:**
1. Play Lobby scene
2. Click "Join Queue"
3. Observe queue status

**Expected Result:**
- ✅ Queue panel appears
- ✅ Position shows "1"
- ✅ Estimated wait time displayed
- ✅ "Leave Queue" button visible

---

#### Test 3.2: Leave Queue

**Objective:** Test queue exit

**Steps:**
1. While in queue, click "Leave Queue"

**Expected Result:**
- ✅ Queue panel hides
- ✅ "Join Queue" button reappears
- ✅ Backend receives queue:leave event

---

#### Test 3.3: Match Found (Two Players)

**Objective:** Test full matchmaking flow

**Setup:**
1. Register second user:
   - Email: `test2@example.com`
   - Username: `TestPlayer2`
   - Password: `password123`

**Steps:**
1. Open Unity project TWICE (two Unity Editor instances)
2. In Instance 1: Login as TestPlayer1, join queue
3. In Instance 2: Login as TestPlayer2, join queue
4. Wait for match

**Expected Result:**
- ✅ Both see "Match Found" panel
- ✅ Opponent info displayed
- ✅ Both auto-send "match:ready"
- ✅ Both transition to Game scene

**Alternative Setup (Without Second Unity Instance):**
1. Use browser-based Socket.IO client
2. Or use test script from backend

---

### Phase 4: Gameplay Test

#### Test 4.1: Match Initialization

**Objective:** Verify game scene loads correctly

**Steps:**
1. After match found (Test 3.3)
2. Observe Game scene

**Expected Result:**
- ✅ Both ships visible (blue player, red opponent)
- ✅ Health bars at 100%
- ✅ Match ID displayed
- ✅ Timer starts counting
- ✅ Ability UI shows "READY"

---

#### Test 4.2: Movement Input

**Objective:** Test keyboard movement

**Steps:**
1. In Game scene, press W/A/S/D keys
2. Observe player ship

**Expected Result:**
- ✅ Ship moves in correct direction
- ✅ Ship rotation updates
- ✅ Input sent to server at 60 FPS
- ✅ Smooth movement

---

#### Test 4.3: Fire Weapon

**Objective:** Test weapon input

**Steps:**
1. Press Space bar

**Expected Result:**
- ✅ Fire input sent to server
- ✅ Fire effect plays (if prefab assigned)
- ✅ Console shows "Weapon fired"

---

#### Test 4.4: Use Ability

**Objective:** Test ability with cooldown

**Steps:**
1. Press E key
2. Wait 5 seconds
3. Press E again

**Expected Result:**
- ✅ First use: Ability activates
- ✅ Cooldown UI shows countdown (5s → 0s)
- ✅ Second use fails during cooldown
- ✅ After 5s, ability ready again

---

#### Test 4.5: Server Snapshots

**Objective:** Verify snapshot processing

**Steps:**
1. During match, observe Console
2. Filter for "game:snapshot" messages

**Expected Result:**
- ✅ Snapshots received at ~20Hz (every 50ms)
- ✅ Player positions update
- ✅ Health values update
- ✅ No null reference exceptions

---

#### Test 4.6: Match End

**Objective:** Test match completion

**Setup:**
- This requires actual combat or server forcing match end

**Steps:**
1. Wait for health to reach 0
2. OR manually end match from backend

**Expected Result:**
- ✅ "Game ended" message
- ✅ Winner determined
- ✅ 2-second delay
- ✅ Transition to Result scene

---

### Phase 5: Result Screen Test

#### Test 5.1: Victory Screen

**Objective:** Test win condition display

**Steps:**
1. Complete a match as winner
2. Observe Result scene

**Expected Result:**
- ✅ "VICTORY!" text in green
- ✅ "+25 ELO" displayed
- ✅ Updated stats shown
- ✅ Rating increased by 25

---

#### Test 5.2: Defeat Screen

**Objective:** Test loss condition display

**Steps:**
1. Complete a match as loser
2. Observe Result scene

**Expected Result:**
- ✅ "DEFEAT" text in red
- ✅ "-25 ELO" displayed
- ✅ Updated stats shown
- ✅ Rating decreased by 25

---

#### Test 5.3: Back to Lobby

**Objective:** Test return flow

**Steps:**
1. In Result scene, click "Back to Lobby"

**Expected Result:**
- ✅ Return to Lobby scene
- ✅ Updated stats visible
- ✅ Can join queue again
- ✅ WebSocket still connected

---

### Phase 6: Touch Input Test (Android)

#### Test 6.1: Touch Movement

**Objective:** Test touch controls

**Steps:**
1. Build for Android
2. Install on device
3. In Game scene, drag finger on screen

**Expected Result:**
- ✅ Ship moves in drag direction
- ✅ Movement normalized correctly
- ✅ Input sent at 60 FPS

---

#### Test 6.2: Touch Fire

**Objective:** Test tap to fire

**Steps:**
1. Tap screen quickly

**Expected Result:**
- ✅ Fire input sent
- ✅ Same as Space key on keyboard

---

#### Test 6.3: Touch Ability

**Objective:** Test swipe for ability

**Steps:**
1. Swipe across screen (long drag > 100 pixels)

**Expected Result:**
- ✅ Ability activates
- ✅ Cooldown starts
- ✅ Same as E key

---

### Phase 7: Stress Tests

#### Test 7.1: Network Disconnection

**Objective:** Handle network loss gracefully

**Steps:**
1. During match, stop backend server
2. Observe client behavior

**Expected Result:**
- ✅ WebSocket disconnect detected
- ✅ Error logged but no crash
- ✅ User can return to Login

---

#### Test 7.2: Invalid Token

**Objective:** Handle expired tokens

**Steps:**
1. Edit PlayerPrefs, set invalid token
2. Try auto-login

**Expected Result:**
- ✅ Auto-login fails gracefully
- ✅ Show login screen
- ✅ User can login again

---

#### Test 7.3: Rapid Input

**Objective:** Stress test input system

**Steps:**
1. Spam all keys (W/A/S/D/Space/E) rapidly
2. Observe for 30 seconds

**Expected Result:**
- ✅ No exceptions
- ✅ Input queue handles overflow
- ✅ Performance stable

---

## 📊 Test Results Template

```
Test Date: _____________
Unity Version: 2022.3.62f3
Backend Version: _____________

Phase 1: Backend Connectivity
[ ] Test 1.1: REST API Connection
[ ] Test 1.2: WebSocket Connection

Phase 2: Authentication
[ ] Test 2.1: User Registration
[ ] Test 2.2: User Login
[ ] Test 2.3: Auto-Login
[ ] Test 2.4: Logout

Phase 3: Matchmaking
[ ] Test 3.1: Join Queue (Single Player)
[ ] Test 3.2: Leave Queue
[ ] Test 3.3: Match Found (Two Players)

Phase 4: Gameplay
[ ] Test 4.1: Match Initialization
[ ] Test 4.2: Movement Input
[ ] Test 4.3: Fire Weapon
[ ] Test 4.4: Use Ability
[ ] Test 4.5: Server Snapshots
[ ] Test 4.6: Match End

Phase 5: Result Screen
[ ] Test 5.1: Victory Screen
[ ] Test 5.2: Defeat Screen
[ ] Test 5.3: Back to Lobby

Phase 6: Touch Input (Android)
[ ] Test 6.1: Touch Movement
[ ] Test 6.2: Touch Fire
[ ] Test 6.3: Touch Ability

Phase 7: Stress Tests
[ ] Test 7.1: Network Disconnection
[ ] Test 7.2: Invalid Token
[ ] Test 7.3: Rapid Input

Overall Status: _____________
Issues Found: _____________
```

---

## 🐛 Known Issues & Workarounds

### Issue: WebSocket Closes Immediately
**Cause:** Invalid JWT token
**Fix:** Clear PlayerPrefs and login again

### Issue: Ships Don't Move
**Cause:** GameManager references not assigned
**Fix:** Assign PlayerShip and OpponentShip in Inspector

### Issue: UI Not Visible
**Cause:** Canvas Scaler misconfigured
**Fix:** Set Canvas to "Screen Space - Overlay"

### Issue: Input Not Sending
**Cause:** InputController not attached
**Fix:** Ensure InputController is on GameManager

---

## 🔧 Debugging Tips

### Enable Verbose Logging:
```csharp
// In Logger.cs, add timestamp:
Debug.Log($"[{Time.time:F2}] {message}");
```

### Monitor WebSocket Messages:
- Check Console for cyan "[NETWORK]" messages
- Filter Console by "NETWORK" keyword

### Inspect PlayerPrefs:
```csharp
// In Unity Console:
Debug.Log(PlayerPrefs.GetString("pvp_access_token"));
```

### Check Backend Logs:
```bash
# In backend terminal:
npm run start:dev
# Watch for socket connection logs
```

---

## ✅ Final Acceptance Criteria

Before considering the client "complete":

1. **Authentication:**
   - [x] Register works
   - [x] Login works
   - [x] Auto-login works
   - [x] Logout works

2. **Networking:**
   - [x] WebSocket connects
   - [x] Events send/receive
   - [x] Disconnect handled

3. **Matchmaking:**
   - [x] Join queue works
   - [x] Leave queue works
   - [x] Match found works
   - [x] Ready system works

4. **Gameplay:**
   - [x] Ships render
   - [x] Input works (keyboard + touch)
   - [x] Snapshots process
   - [x] Health updates
   - [x] Ability cooldown works
   - [x] Match ends properly

5. **UI:**
   - [x] All scenes load
   - [x] Transitions work
   - [x] Stats display correctly
   - [x] Results show correctly

6. **Code Quality:**
   - [x] No console errors
   - [x] No null references
   - [x] Clean architecture
   - [x] Well-commented

---

## 📞 Support

If tests fail:

1. Check Unity Console for errors
2. Check backend terminal for errors
3. Verify database is running
4. Verify Redis is running
5. Check network firewall settings
6. Review WebSocket connection logs

---

**Happy Testing!** 🎮✅
