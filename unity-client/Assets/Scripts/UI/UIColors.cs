using UnityEngine;

namespace UI
{
    /// <summary>
    /// Central color palette for the game's UI
    /// Following the premium design system specifications
    /// </summary>
    public static class UIColors
    {
        #region Primary Colors
        /// <summary>Cyan Glow - Active/Primary CTA</summary>
        public static readonly Color Primary = new Color(0f, 0.83f, 1f, 1f); // #00D4FF
        
        /// <summary>Magenta - Accent/Ability</summary>
        public static readonly Color Secondary = new Color(1f, 0f, 0.43f, 1f); // #FF006E
        
        /// <summary>Deep Blue - Main BG</summary>
        public static readonly Color Background = new Color(0.04f, 0.05f, 0.15f, 1f); // #0A0E27
        
        /// <summary>Surface - Cards/Panels</summary>
        public static readonly Color Surface = new Color(0.1f, 0.12f, 0.23f, 1f); // #1A1F3A
        #endregion

        #region Status Colors
        /// <summary>Green - Win/Victory</summary>
        public static readonly Color Success = new Color(0f, 1f, 0.53f, 1f); // #00FF88
        
        /// <summary>Red - Damage/Loss</summary>
        public static readonly Color Danger = new Color(1f, 0.27f, 0.27f, 1f); // #FF4444
        
        /// <summary>Orange - Ability Ready</summary>
        public static readonly Color Warning = new Color(1f, 0.72f, 0f, 1f); // #FFB800
        #endregion

        #region Text Colors
        /// <summary>Primary text color</summary>
        public static readonly Color TextPrimary = new Color(1f, 1f, 1f, 1f); // #FFFFFF
        
        /// <summary>Secondary text color</summary>
        public static readonly Color TextSecondary = new Color(0.69f, 0.72f, 0.78f, 1f); // #B0B8C8
        #endregion

        #region Gradient Colors
        /// <summary>Cyan gradient start</summary>
        public static readonly Color GradientStart = new Color(0.06f, 0.2f, 0.38f, 1f); // #0F3460
        
        /// <summary>Gradient middle</summary>
        public static readonly Color GradientMiddle = new Color(0.09f, 0.13f, 0.24f, 1f); // #16213E
        
        /// <summary>Gradient end</summary>
        public static readonly Color GradientEnd = new Color(0.05f, 0.01f, 0.13f, 1f); // #0D0221
        #endregion

        #region Health Colors
        /// <summary>High health (green)</summary>
        public static readonly Color HealthHigh = new Color(0f, 1f, 0.53f, 1f);
        
        /// <summary>Medium health (yellow)</summary>
        public static readonly Color HealthMedium = new Color(1f, 0.92f, 0f, 1f);
        
        /// <summary>Low health (red)</summary>
        public static readonly Color HealthLow = new Color(1f, 0.27f, 0.27f, 1f);
        #endregion

        #region Shield Colors
        /// <summary>Shield color (cyan)</summary>
        public static readonly Color Shield = new Color(0f, 0.83f, 1f, 1f);
        
        /// <summary>Shield breaking color</summary>
        public static readonly Color ShieldBreaking = new Color(0f, 0.5f, 0.8f, 1f);
        #endregion

        #region Glow Presets
        /// <summary>Cyan glow effect</summary>
        public static readonly Color GlowCyan = new Color(0f, 0.83f, 1f, 1f);
        
        /// <summary>Magenta glow effect</summary>
        public static readonly Color GlowMagenta = new Color(1f, 0f, 0.43f, 1f);
        
        /// <summary>Green glow effect</summary>
        public static readonly Color GlowGreen = new Color(0f, 1f, 0.53f, 1f);
        
        /// <summary>Red glow effect</summary>
        public static readonly Color GlowRed = new Color(1f, 0.27f, 0.27f, 1f);
        
        /// <summary>Orange glow effect</summary>
        public static readonly Color GlowOrange = new Color(1f, 0.72f, 0f, 1f);
        
        /// <summary>Yellow glow effect</summary>
        public static readonly Color GlowYellow = new Color(1f, 0.92f, 0f, 1f);
        #endregion

        #region Victory/Defeat Colors
        /// <summary>Victory color (green-cyan gradient)</summary>
        public static readonly Color Victory = new Color(0f, 1f, 0.33f, 1f);
        
        /// <summary>Defeat color (red-magenta gradient)</summary>
        public static readonly Color Defeat = new Color(1f, 0f, 0.27f, 1f);
        #endregion

        #region UI Element Colors
        /// <summary>Button hover color</summary>
        public static readonly Color ButtonHover = new Color(1f, 1f, 1f, 0.1f);
        
        /// <summary>Button pressed color</summary>
        public static readonly Color ButtonPressed = new Color(1f, 1f, 1f, 0.2f);
        
        /// <summary>Input field focus color</summary>
        public static readonly Color InputFocus = new Color(0f, 0.83f, 1f, 0.5f);
        
        /// <summary>Input field error color</summary>
        public static readonly Color InputError = new Color(1f, 0.27f, 0.27f, 0.5f);
        
        /// <summary>Card background (glassmorphism)</summary>
        public static readonly Color CardBackground = new Color(0.1f, 0.12f, 0.23f, 0.8f);
        
        /// <summary>Card border glow</summary>
        public static readonly Color CardBorder = new Color(0f, 0.83f, 1f, 0.3f);
        #endregion

        #region Particle Colors
        /// <summary>Cyan particle</summary>
        public static readonly Color ParticleCyan = new Color(0f, 0.83f, 1f, 0.6f);
        
        /// <summary>Magenta particle</summary>
        public static readonly Color ParticleMagenta = new Color(1f, 0f, 0.43f, 0.6f);
        
        /// <summary>Impact particle (orange)</summary>
        public static readonly Color ParticleImpact = new Color(1f, 0.5f, 0f, 1f);
        
        /// <summary>Confetti colors</summary>
        public static readonly Color[] ConfettiColors = new Color[]
        {
            new Color(1f, 0f, 0.43f, 1f),  // Magenta
            new Color(0f, 0.83f, 1f, 1f),   // Cyan
            new Color(0f, 1f, 0.53f, 1f),   // Green
            new Color(1f, 0.92f, 0f, 1f),   // Yellow
            new Color(1f, 0.27f, 0.27f, 1f) // Red
        };
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get health color based on health ratio (0-1)
        /// </summary>
        public static Color GetHealthColor(float healthRatio)
        {
            if (healthRatio > 0.6f)
            {
                return HealthHigh;
            }
            else if (healthRatio > 0.25f)
            {
                return HealthMedium;
            }
            else
            {
                return HealthLow;
            }
        }

        /// <summary>
        /// Get rating change color (green for positive, red for negative)
        /// </summary>
        public static Color GetRatingChangeColor(int change)
        {
            return change >= 0 ? Success : Danger;
        }

        /// <summary>
        /// Create a gradient color at the specified position (0-1)
        /// </summary>
        public static Color GetGradientColor(float position)
        {
            position = Mathf.Clamp01(position);
            
            if (position < 0.5f)
            {
                return Color.Lerp(GradientStart, GradientMiddle, position * 2f);
            }
            else
            {
                return Color.Lerp(GradientMiddle, GradientEnd, (position - 0.5f) * 2f);
            }
        }

        /// <summary>
        /// Get random confetti color
        /// </summary>
        public static Color GetRandomConfettiColor()
        {
            return ConfettiColors[Random.Range(0, ConfettiColors.Length)];
        }
        #endregion
    }
}
