using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PvpGame.Game
{
    public class HealthDisplay : MonoBehaviour
    {
        [Header("References")]
        public Slider healthBar;
        public TextMeshProUGUI healthText;
        public Image fillImage;

        [Header("Colors")]
        public Color fullHealthColor = Color.green;
        public Color lowHealthColor = Color.red;

        public void SetHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"{current}/{max}";
            }

            if (fillImage != null)
            {
                float percentage = (float)current / max;
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, percentage);
            }
        }
    }
}
