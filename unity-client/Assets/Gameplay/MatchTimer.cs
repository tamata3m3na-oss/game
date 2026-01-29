using UnityEngine;
using TMPro;

namespace ShipBattle.Gameplay
{
    /// <summary>
    /// Displays match timer synchronized with server timestamps.
    /// Phase 2 - New implementation.
    /// </summary>
    public class MatchTimer : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Configuration")]
        [SerializeField] private float matchDuration = 300f; // 5 minutes default

        private float currentTime;
        private bool isRunning;

        private void Update()
        {
            if (isRunning)
            {
                currentTime += Time.deltaTime;
                UpdateDisplay();
            }
        }

        public void StartTimer()
        {
            currentTime = 0f;
            isRunning = true;
            Debug.Log("[MatchTimer] Timer started");
        }

        public void StopTimer()
        {
            isRunning = false;
            Debug.Log("[MatchTimer] Timer stopped");
        }

        public void SetTime(float seconds)
        {
            currentTime = seconds;
            UpdateDisplay();
        }

        public void SyncWithServerTimestamp(long serverTimestamp, long matchStartTimestamp)
        {
            // Calculate elapsed time from timestamps
            long elapsedMs = serverTimestamp - matchStartTimestamp;
            currentTime = elapsedMs / 1000f;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (timerText == null)
            {
                return;
            }

            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void OnValidate()
        {
            // Auto-assign in editor
            if (timerText == null)
            {
                timerText = GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
