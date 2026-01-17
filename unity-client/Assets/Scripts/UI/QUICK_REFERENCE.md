# Quick Reference - UI Enhancement System

## Common Tasks

### Add Glow to a Button
```csharp
GlowEffect glow = button.GetComponent<GlowEffect>();
if (glow == null) glow = button.gameObject.AddComponent<GlowEffect>();
glow.SetGlowColor(UIColors.GlowCyan);
glow.SetGlowMode(GlowMode.Pulse);
```

### Add Shake for Errors
```csharp
ShakeEffect shake = inputField.GetComponent<ShakeEffect>();
if (shake == null) shake = inputField.gameObject.AddComponent<ShakeEffect>();
shake.ErrorShake();
```

### Fade In Canvas
```csharp
CanvasGroup cg = GetComponent<CanvasGroup>();
AnimationController.Instance.FadeIn(cg, 0.3f);
```

### Scale In Element
```csharp
AnimationController.Instance.ScaleIn(transform, 1f, 0.5f);
```

### Staggered Animation for List
```csharp
Transform[] items = new Transform[] { item1, item2, item3 };
AnimationController.Instance.StaggeredScaleIn(items, 0.1f, 0.5f);
```

### Spawn Particles
```csharp
ParticleController.Instance.SpawnImpactEffect(position);
ParticleController.Instance.SpawnConfettiEffect(position, 50);
```

### Transition to Scene
```csharp
TransitionManager.Instance.LoadScene("LobbyScene", TransitionType.Fade);
TransitionManager.Instance.VictoryTransition("LobbyScene");
```

### Color Helpers
```csharp
Color healthColor = UIColors.GetHealthColor(healthRatio); // 0-1
Color ratingColor = UIColors.GetRatingChangeColor(ratingChange);
Color gradient = UIColors.GetGradientColor(0.5f); // 0-1
```

## Animation Speed Settings

```csharp
// In AnimationController inspector or code:
defaultFadeDuration = 0.3f;      // Fade animations
defaultScaleDuration = 0.4f;     // Scale animations
defaultSlideDuration = 0.5f;     // Slide animations
defaultBounceDuration = 0.5f;    // Bounce animations
```

## Glow Mode Options

```csharp
GlowMode.None      // No glow
GlowMode.Static    // Constant glow
GlowMode.Pulse     // Pulsing glow
GlowMode.Breathe   // Breathe effect (scale + pulse)
GlowMode.OnHover   // Glow only when hovered
```

## Transition Types

```csharp
TransitionType.Fade      // Simple fade
TransitionType.ZoomIn    // Zoom in effect
TransitionType.SlideUp   // Slide up from bottom
TransitionType.SlideDown // Slide down from top
TransitionType.Circle    // Circular mask (simplified)
```

## Preset Shake Effects

```csharp
shake.ErrorShake();       // 8f strength, 0.4s duration
shake.DamageShake();      // 15f strength, 0.2s duration
shake.SubtleShake();     // 3f strength, 0.3s duration
shake.ExplosiveShake();   // 20f strength, 0.5s duration
```

## Camera Shake Presets

```csharp
ShakeEffect.ShakeCamera(0.5f, 0.3f);        // Custom
ShakeEffect.ShakeCameraImpact();              // 0.5f strength, 0.15s
ShakeEffect.ShakeCameraExplosion();           // 1.5f strength, 0.4s
```

## Color Presets

```csharp
UIColors.Primary        // #00D4FF (Cyan)
UIColors.Secondary      // #FF006E (Magenta)
UIColors.Success       // #00FF88 (Green)
UIColors.Danger        // #FF4444 (Red)
UIColors.Warning       // #FFB800 (Orange)

UIColors.HealthHigh    // Green
UIColors.HealthMedium  // Yellow
UIColors.HealthLow     // Red

GlowEffect.CyanGlow
GlowEffect.MagentaGlow
GlowEffect.GreenGlow
GlowEffect.RedGlow
GlowEffect.OrangeGlow
GlowEffect.YellowGlow
```

## Stopping Animations

```csharp
// Stop pulse
AnimationController.Instance.StopPulse(transform);

// Stop rotation
AnimationController.Instance.StopRotateAnimation(transform);

// Stop shake
ShakeEffect shake = GetComponent<ShakeEffect>();
shake.StopShake();
```

## Common Patterns

### Button with Press Animation
```csharp
button.onClick.AddListener(() => {
    AnimationController.Instance.ButtonPress(button.transform);
    // Your button logic here
});
```

### Loading State
```csharp
// Show loading
loadingSpinner.SetActive(true);
AnimationController.Instance.RotateAnimation(loadingSpinner.transform, 1f);
button.interactable = false;

// Hide loading
AnimationController.Instance.StopRotateAnimation(loadingSpinner.transform);
loadingSpinner.SetActive(false);
button.interactable = true;
```

### Counter Animation
```csharp
AnimationController.Instance.CounterAnimation(textMesh, 0, 100, 0.5f);
```

### Fade In Sequence
```csharp
CanvasGroup[] elements = new CanvasGroup[] { cg1, cg2, cg3 };
AnimationController.Instance.StaggeredFadeIn(elements, 0.1f, 0.3f);
```

### Damage Feedback
```csharp
public void TakeDamage(int damage)
{
    // Screen flash
    AnimationController.Instance.DamageFlash(overlayImage, 0.1f);
    
    // Screen shake
    ShakeEffect.ShakeCamera(0.5f, 0.15f);
    
    // Damage number
    ParticleController.Instance.SpawnDamageNumber(position, damage, isCritical);
}
```

## Health Bar Color Transition

```csharp
// Automatic color based on health ratio
float healthRatio = currentHealth / maxHealth;
healthBarFill.color = UIColors.GetHealthColor(healthRatio);
// Returns: Green (>60%), Yellow (25-60%), Red (<25%)
```

## UI Hover Detection

```csharp
// Add UIHoverDetector component for automatic hover effects
// Automatically calls OnHoverEnter/OnHoverExit on GlowEffect
UIHoverDetector hoverDetector = gameObject.AddComponent<UIHoverDetector>();
```

## Particle Types

```csharp
// Combat
SpawnImpactEffect(position)
SpawnShieldBreakEffect(position)
SpawnExplosionEffect(position, scale)
SpawnDamageNumber(position, damage, isCritical)

// Celebration
SpawnConfettiEffect(position, burstCount)

// Background
StartBackgroundParticles()
StopBackgroundParticles()

// Thruster
SpawnThrusterTrail(position, rotation, duration)
```

## Scene Transition Pattern

```csharp
// Load scene with transition
TransitionManager.Instance.LoadScene("SceneName", TransitionType.Fade);

// Victory/Defeat specific
if (isVictory)
{
    TransitionManager.Instance.VictoryTransition("LobbyScene");
}
else
{
    TransitionManager.Instance.DefeatTransition("LobbyScene");
}

// Match transition (with zoom)
TransitionManager.Instance.TransitionToMatch();
```

## Performance Tips

### Object Pooling
- Particle systems already use object pooling
- Pool size: 50 particles per type
- Automatic return to pool

### Stop Unused Animations
```csharp
// Stop continuous animations when not needed
AnimationController.Instance.StopPulse(transform);
AnimationController.Instance.StopRotateAnimation(transform);
```

### Limit Particles
- Max particles: 200-300
- Reduce pool sizes if needed
- Disable expensive effects on low-end devices

### Canvas Optimization
- Use Canvas Groups for batch operations
- Avoid too many separate Canvases
- Use appropriate Graphic Raycasters

## Debugging

### Check if Managers Exist
```csharp
if (AnimationController.Instance == null)
{
    Debug.LogWarning("AnimationController not initialized");
}
```

### Verify DOTween Installation
```
Tools > Demigiant > DOTween Utility Panel
Should show version and setup status
```

### Check Particle Prefabs
```csharp
// Verify particle prefabs are assigned in inspector
// Prefabs must have ParticleSystem component
```

### Common Issues

**Animations not playing:**
1. Check if AnimationController.Instance is null
2. Verify DOTween is installed and setup
3. Check console for errors

**Particles not appearing:**
1. Check particle prefab assignments
2. Verify particle layers
3. Check camera culling mask
4. Ensure particle system is enabled

**Performance issues:**
1. Reduce particle count
2. Disable expensive effects (bloom, glow)
3. Check FPS counter in GameSceneUI
4. Monitor memory usage

## Quick Code Snippets

### Delayed Action
```csharp
StartCoroutine(DelayedAction(1f, () => {
    // Your code here
}));

IEnumerator DelayedAction(float delay, System.Action action)
{
    yield return new WaitForSeconds(delay);
    action?.Invoke();
}
```

### Toggle Visibility with Animation
```csharp
public void ShowPanel(GameObject panel)
{
    panel.SetActive(true);
    AnimationController.Instance.ScaleIn(panel.transform, 1f, 0.3f);
}

public void HidePanel(GameObject panel)
{
    AnimationController.Instance.ScaleOut(panel.transform, 0.3f);
    StartCoroutine(HideAfterAnimation(panel, 0.3f));
}

IEnumerator HideAfterAnimation(GameObject obj, float delay)
{
    yield return new WaitForSeconds(delay);
    obj.SetActive(false);
}
```

### Update Text with Animation
```csharp
TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
AnimationController.Instance.FadeText(text, text.alpha, 1f, 0.3f);
```

## Full Example: Button with Effects

```csharp
public class PremiumButton : MonoBehaviour
{
    public Button button;
    public Image buttonImage;
    
    private void Start()
    {
        // Add glow effect
        GlowEffect glow = button.gameObject.AddComponent<GlowEffect>();
        glow.SetGlowColor(UIColors.GlowCyan);
        glow.SetGlowMode(GlowMode.Pulse);
        
        // Add hover detector
        button.gameObject.AddComponent<UIHoverDetector>();
        
        // Add button press animation
        button.onClick.AddListener(() => {
            AnimationController.Instance.ButtonPress(button.transform);
            OnButtonClicked();
        });
        
        // Add shake on error (example)
        button.gameObject.AddComponent<ShakeEffect>();
    }
    
    public void OnButtonClicked()
    {
        Debug.Log("Button clicked!");
    }
    
    public void ShowError()
    {
        ShakeEffect shake = GetComponent<ShakeEffect>();
        shake?.ErrorShake();
        
        GlowEffect glow = GetComponent<GlowEffect>();
        glow?.SetGlowColor(UIColors.GlowRed);
    }
}
```

## For More Information

See:
- `README.md` - Full documentation
- `SETUP_GUIDE.md` - Setup instructions
- `UI_UX_IMPLEMENTATION_SUMMARY.md` - Implementation overview
