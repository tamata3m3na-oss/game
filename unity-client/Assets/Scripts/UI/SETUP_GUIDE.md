# Quick Setup Guide - Premium UI/UX Enhancement

## Prerequisites

### Required Package: DOTween

This UI enhancement system requires **DOTween** (by Demigiant) for animations.

#### Installation Options:

**Option 1: Unity Asset Store (Recommended)**
1. Open Unity
2. Go to Window > General > Asset Store
3. Search for "DOTween"
4. Import the package
5. Run the DOTween Setup utility: Tools > Demigiant > DOTween Utility Panel > Setup DOTween

**Option 2: Package Manager (Unity 2019.4+)**
1. Open Window > Package Manager
2. Click the "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/Demigiant/dotween.git`
5. Click Add

## Quick Start

### 1. Initialize Controllers (Automatic)

The enhanced scene controllers automatically initialize required managers:

```csharp
private void InitializeManagers()
{
    if (AnimationController.Instance == null)
    {
        GameObject obj = new GameObject("AnimationController");
        obj.AddComponent<AnimationController>();
    }
    // ParticleController and TransitionManager also initialized
}
```

### 2. Replace UI Controllers

For each scene, replace the old controller with the enhanced version:

**LoginScene:**
- Remove: `LoginUIController`
- Add: `LoginSceneUI`

**LobbyScene:**
- Remove: `LobbyUIController`
- Add: `LobbySceneUI`

**GameScene:**
- Remove: `MatchUIController`
- Add: `GameSceneUI`

**ResultScene:**
- Remove: `ResultScreenController`
- Add: `ResultSceneUI`

### 3. Reassign UI References

Since the enhanced controllers use the same field names, most references will reconnect automatically. However, you may need to add missing elements for new features.

**Example - LoginScene:**
```
Old fields (already exist):
- emailInputField
- passwordInputField
- loginButton
- loginErrorText
- registerUsernameInput
- registerEmailInput
- registerPasswordInput
- registerButton
- registerErrorText
- registerSuccessText
- loginPanel
- registerPanel

New fields (optional, add for enhanced effects):
- logoTransform (Transform)
- backgroundImage (Image)
- loginCanvasGroup (CanvasGroup)
- registerCanvasGroup (CanvasGroup)
- loadingSpinner (GameObject)
```

### 4. Test

1. Play each scene
2. Verify animations play
3. Check for console errors
4. Test all button interactions

## Common Setup Tasks

### Adding a Glow Effect to a Button

```csharp
// In inspector
GlowEffect glow = myButton.AddComponent<GlowEffect>();
glow.SetGlowColor(UIColors.GlowCyan);
glow.SetGlowMode(GlowMode.Pulse);
```

### Adding a Shake Effect for Errors

```csharp
// In inspector
ShakeEffect shake = myInputField.AddComponent<ShakeEffect>();
shake.ErrorShake(); // Call when error occurs
```

### Using the Animation Controller

```csharp
// Fade in
AnimationController.Instance.FadeIn(canvasGroup, 0.3f);

// Scale in with bounce
AnimationController.Instance.ScaleIn(transform, 1f, 0.5f);

// Slide up
AnimationController.Instance.SlideUp(transform, 100f, 0.5f);

// Pulse animation
AnimationController.Instance.Pulse(transform, 1.1f, 0.5f);
```

### Using the Particle Controller

```csharp
// Spawn impact effect
ParticleController.Instance.SpawnImpactEffect(position);

// Spawn confetti
ParticleController.Instance.SpawnConfettiEffect(position, 50);

// Show floating damage number
ParticleController.Instance.SpawnDamageNumber(position, 25, true);
```

### Using the Transition Manager

```csharp
// Load scene with fade transition
TransitionManager.Instance.LoadScene("LobbyScene", TransitionType.Fade);

// Victory transition
TransitionManager.Instance.VictoryTransition("LobbyScene");

// Match transition
TransitionManager.Instance.TransitionToMatch();
```

## Troubleshooting

### "DOTween not found" Error

**Solution:** Install DOTween
1. Open Asset Store (Window > General > Asset Store)
2. Search for "DOTween"
3. Import and run setup utility

### Animations Not Playing

**Possible Causes:**
1. `AnimationController.Instance` is null
2. DOTween not installed
3. Managers not initialized

**Solution:**
- Verify DOTween is installed
- Check console for initialization errors
- Ensure `InitializeManagers()` is called in `Start()`

### Particles Not Appearing

**Possible Causes:**
1. Particle prefabs not assigned in inspector
2. Particle system disabled
3. Camera doesn't see particles

**Solution:**
1. Assign particle prefabs to `ParticleController` in inspector
2. Check particle system settings
3. Verify particle layers and camera culling mask

### Performance Issues

**Solutions:**
1. Reduce particle pool sizes in `ParticleController`
2. Limit active particles
3. Disable expensive effects (bloom, glow)
4. Use object pooling consistently
5. Monitor FPS in Game Scene UI

## UI Element Checklist

### LoginScene Required Elements:
- [ ] emailInputField (TMP_InputField)
- [ ] passwordInputField (TMP_InputField)
- [ ] loginButton (Button)
- [ ] loginErrorText (TextMeshProUGUI)
- [ ] registerUsernameInput (TMP_InputField)
- [ ] registerEmailInput (TMP_InputField)
- [ ] registerPasswordInput (TMP_InputField)
- [ ] registerButton (Button)
- [ ] registerErrorText (TextMeshProUGUI)
- [ ] registerSuccessText (TextMeshProUGUI)
- [ ] loginPanel (Transform)
- [ ] registerPanel (Transform)

### LoginScene Optional Elements (for enhanced effects):
- [ ] logoTransform (Transform)
- [ ] backgroundImage (Image)
- [ ] loginCanvasGroup (CanvasGroup)
- [ ] registerCanvasGroup (CanvasGroup)
- [ ] loadingSpinner (GameObject)

### LobbyScene Required Elements:
- [ ] usernameText (TextMeshProUGUI)
- [ ] ratingText (TextMeshProUGUI)
- [ ] winsText (TextMeshProUGUI)
- [ ] lossesText (TextMeshProUGUI)
- [ ] queueButton (Button)
- [ ] leaveQueueButton (Button)
- [ ] queueStatusText (TextMeshProUGUI)
- [ ] queuePanel (GameObject)
- [ ] leaderboardContent (Transform)
- [ ] leaderboardEntryPrefab (GameObject)
- [ ] disconnectButton (Button)

### LobbyScene Optional Elements:
- [ ] winRateText (TextMeshProUGUI)
- [ ] rankText (TextMeshProUGUI)
- [ ] statsCard (Transform)
- [ ] rankBadge (Image)
- [ ] queueButtonFill (Image)
- [ ] leaderboardPanel (Transform)
- [ ] settingsButton (Button)
- [ ] leaderboardButton (Button)
- [ ] titleTransform (Transform)
- [ ] mainCanvasGroup (CanvasGroup)

### GameScene Required Elements:
- [ ] playerHealthBar (Image)
- [ ] opponentHealthBar (Image)
- [ ] playerShieldBar (Image)
- [ ] opponentShieldBar (Image)
- [ ] fireCooldownIndicator (Image)
- [ ] abilityCooldownIndicator (Image)
- [ ] fireCooldownText (TextMeshProUGUI)
- [ ] abilityCooldownText (TextMeshProUGUI)
- [ ] playerNameText (TextMeshProUGUI)
- [ ] opponentNameText (TextMeshProUGUI)
- [ ] timerText (TextMeshProUGUI)
- [ ] fpsText (TextMeshProUGUI)
- [ ] pingText (TextMeshProUGUI)

### GameScene Optional Elements:
- [ ] playerHealthBarFill (Image)
- [ ] opponentHealthBarFill (Image)
- [ ] playerShieldBarFill (Image)
- [ ] opponentShieldBarFill (Image)
- [ ] fireCooldownTransform (Transform)
- [ ] abilityCooldownTransform (Transform)
- [ ] opponentInfoPanel (Transform)
- [ ] shieldStatusText (TextMeshProUGUI)
- [ ] abilityStatusText (TextMeshProUGUI)
- [ ] damageFlashOverlay (Image)
- [ ] vignetteOverlay (Image)
- [ ] hitMarker (Transform)

### ResultScene Required Elements:
- [ ] winnerText (TextMeshProUGUI)
- [ ] resultTitle (TextMeshProUGUI)
- [ ] yourHealthText (TextMeshProUGUI)
- [ ] opponentHealthText (TextMeshProUGUI)
- [ ] yourDamageText (TextMeshProUGUI)
- [ ] opponentDamageText (TextMeshProUGUI)
- [ ] durationText (TextMeshProUGUI)
- [ ] ratingChangeText (TextMeshProUGUI)
- [ ] nextMatchButton (Button)

### ResultScene Optional Elements:
- [ ] resultPanel (Transform)
- [ ] backgroundGradient (Image)
- [ ] statsContainer (Transform)
- [ ] statsCardPrefab (GameObject)
- [ ] ratingBeforeText (TextMeshProUGUI)
- [ ] ratingAfterText (TextMeshProUGUI)
- [ ] ratingArrow (Image)
- [ ] ratingDisplay (Transform)
- [ ] rankText (TextMeshProUGUI)
- [ ] rankChangeText (TextMeshProUGUI)
- [ ] rankBadge (Transform)
- [ ] opponentNameText (TextMeshProUGUI)
- [ ] opponentAvatar (Image)
- [ ] leaderboardButton (Button)
- [ ] lobbyButton (Button)
- [ ] buttonsContainer (Transform)

## Next Steps

After completing setup:

1. **Test thoroughly**: Play through all scenes
2. **Monitor performance**: Check FPS and memory usage
3. **Adjust as needed**: Modify animation speeds, colors, etc.
4. **Polish**: Add additional effects as needed

For detailed documentation, see [README.md](README.md)
