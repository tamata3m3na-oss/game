using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UI.Animations;
using UI.Effects;

namespace UI.Scenes
{
    /// <summary>
    /// Enhanced Lobby Scene with premium visual effects
    /// Includes animated stats, leaderboard, and smooth transitions
    /// </summary>
    public class LobbySceneUI : MonoBehaviour
    {
        [Header("Player Stats Card")]
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI ratingText;
        public TextMeshProUGUI winsText;
        public TextMeshProUGUI lossesText;
        public TextMeshProUGUI winRateText;
        public TextMeshProUGUI rankText;
        public Transform statsCard;
        public Image rankBadge;
        
        [Header("Queue System")]
        public Button queueButton;
        public Button leaveQueueButton;
        public TextMeshProUGUI queueStatusText;
        public GameObject queuePanel;
        public Image queueButtonFill;
        
        [Header("Leaderboard")]
        public Transform leaderboardContent;
        public GameObject leaderboardEntryPrefab;
        public Transform leaderboardPanel;
        
        [Header("Buttons")]
        public Button disconnectButton;
        public Button settingsButton;
        public Button leaderboardButton;
        
        [Header("UI Elements")]
        public Transform titleTransform;
        public CanvasGroup mainCanvasGroup;
        
        [Header("Visual Settings")]
        [SerializeField] private Color primaryGlowColor = new Color(0f, 0.83f, 1f, 1f); // Cyan
        [SerializeField] private Color successColor = new Color(0f, 1f, 0.53f, 1f); // Green
        [SerializeField] private Color dangerColor = new Color(1f, 0.27f, 0.27f, 1f); // Red
        
        private bool isInQueue = false;
        private int currentRating = 1000;

        private void Start()
        {
            // CRITICAL: Wait for managers to be ready
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            // Wait max 5 seconds for managers
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                if (AnimationController.Instance != null && ParticleController.Instance != null && TransitionManager.Instance != null)
                {
                    // Managers ready - proceed
                    InitializeUI();
                    break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (AnimationController.Instance == null)
            {
                Debug.LogError("[LobbySceneUI] AnimationController not initialized after timeout");
            }
            
            if (ParticleController.Instance == null)
            {
                Debug.LogError("[LobbySceneUI] ParticleController not initialized after timeout");
            }
            
            if (TransitionManager.Instance == null)
            {
                Debug.LogError("[LobbySceneUI] TransitionManager not initialized after timeout");
            }
        }

        private void InitializeUI()
        {
            // Initialize UI
            UpdatePlayerStats();
            
            // Set up buttons
            SetupButtons();
            
            // Subscribe to network events
            SubscribeToEvents();
            
            // Play entrance animations
            PlayEntranceAnimations();
            
            // Apply visual effects
            ApplyVisualEffects();
            
            // Update queue button state
            UpdateQueueButtonState();
            
            // Hide queue panel initially
            if (queuePanel != null)
            {
                queuePanel.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (queueButton != null)
            {
                queueButton.onClick.AddListener(OnQueueButtonClicked);
                queueButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(queueButton.transform));
            }
            
            if (leaveQueueButton != null)
            {
                leaveQueueButton.onClick.AddListener(OnLeaveQueueButtonClicked);
            }
            
            if (disconnectButton != null)
            {
                disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
                disconnectButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(disconnectButton.transform));
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
                settingsButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(settingsButton.transform));
            }
            
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.AddListener(OnLeaderboardButtonClicked);
                leaderboardButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(leaderboardButton.transform));
            }
        }

        private void PlayEntranceAnimations()
        {
            // Fade in main canvas
            if (mainCanvasGroup != null)
            {
                AnimationController.Instance.FadeIn(mainCanvasGroup, 0.3f);
            }

            // Title glitch effect
            if (titleTransform != null)
            {
                StartCoroutine(TitleGlitchEffect(titleTransform));
            }

            // Stats card scale in
            if (statsCard != null)
            {
                AnimationController.Instance.ScaleIn(statsCard, 1f, 0.5f);
                
                // Apply glassmorphism effect
                ApplyGlassmorphism(statsCard);
            }

            // Rating counter animation
            StartCoroutine(AnimateRatingCounter());

            // Buttons slide up
            if (queueButton != null)
            {
                AnimationController.Instance.SlideUp(queueButton.transform, 50f, 0.5f);
            }

            if (disconnectButton != null)
            {
                AnimationController.Instance.SlideUp(disconnectButton.transform, 50f, 0.5f);
            }

            // Start background particles
            ParticleController.Instance?.StartBackgroundParticles();
        }

        private IEnumerator TitleGlitchEffect(Transform title)
        {
            if (title == null) yield break;
            
            TextMeshProUGUI titleText = title.GetComponent<TextMeshProUGUI>();
            
            // Glitch effect: 3 frames
            for (int i = 0; i < 3; i++)
            {
                titleText.color = new Color(Random.value, Random.value, Random.value);
                title.localPosition += new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
                yield return new WaitForSeconds(0.15f);
            }
            
            titleText.color = Color.white;
            title.localPosition = Vector3.zero;
            
            // Cyan underline slide in effect
            AnimationController.Instance.SlideInFromLeft(titleTransform.Find("Underline"), 200f, 0.6f);
        }

        private IEnumerator AnimateRatingCounter()
        {
            if (ratingText == null) yield break;
            
            int startRating = 0;
            int endRating = currentRating;
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                int current = Mathf.RoundToInt(Mathf.Lerp(startRating, endRating, elapsed / duration));
                ratingText.text = current.ToString();
                yield return null;
            }
            
            ratingText.text = endRating.ToString();
        }

        private void ApplyVisualEffects()
        {
            // Apply glow effects to buttons
            ApplyGlowEffect(queueButton, primaryGlowColor, GlowMode.Pulse);
            ApplyGlowEffect(leaderboardButton, primaryGlowColor, GlowMode.OnHover);
            ApplyGlowEffect(settingsButton, primaryGlowColor, GlowMode.OnHover);
            ApplyGlowEffect(disconnectButton, dangerColor, GlowMode.OnHover);
            
            // Apply bloom to stats text
            BloomEffect ratingBloom = ratingText?.GetComponent<BloomEffect>();
            if (ratingBloom == null && ratingText != null)
            {
                ratingBloom = ratingText.gameObject.AddComponent<BloomEffect>();
                ratingBloom.SetCyanBloom();
            }
            
            // Rotate settings button icon
            if (settingsButton != null)
            {
                Transform icon = settingsButton.transform.Find("Icon");
                if (icon != null)
                {
                    AnimationController.Instance.RotateAnimation(icon, 10f);
                }
            }
            
            // Pulse rank badge
            if (rankBadge != null)
            {
                AnimationController.Instance.ContinuousPulse(rankBadge.transform, 1.1f, 2f);
            }
        }

        private void ApplyGlassmorphism(Transform element)
        {
            if (element == null) return;
            
            Image image = element.GetComponent<Image>();
            if (image != null)
            {
                // Semi-transparent background
                Color glassColor = image.color;
                glassColor.a = 0.8f;
                image.color = glassColor;
            }
        }

        private void ApplyGlowEffect(Button button, Color color, GlowMode mode)
        {
            if (button == null) return;
            
            GlowEffect glow = button.GetComponent<GlowEffect>();
            if (glow == null)
            {
                glow = button.gameObject.AddComponent<GlowEffect>();
            }
            glow.SetGlowColor(color);
            glow.SetGlowMode(mode);
        }

        private void UpdatePlayerStats()
        {
            if (usernameText != null)
            {
                usernameText.text = AuthManager.Instance.GetUsername();
                
                // Fade in animation for username
                AnimationController.Instance.FadeText(usernameText, 0f, 1f, 0.3f);
            }
            
            // Rating, wins, losses would come from player stats API
            // Placeholder data for demo
            if (ratingText != null) 
            {
                currentRating = 1000;
                ratingText.text = currentRating.ToString();
            }
            if (winsText != null) 
            {
                winsText.text = "10";
                AnimateStatText(winsText, 0, 10, successColor);
            }
            if (lossesText != null) 
            {
                lossesText.text = "5";
                AnimateStatText(lossesText, 0, 5, dangerColor);
            }
            
            // Calculate and display win rate
            if (winRateText != null)
            {
                int wins = int.Parse(winsText.text);
                int losses = int.Parse(lossesText.text);
                float winRate = wins + losses > 0 ? (float)wins / (wins + losses) * 100f : 0f;
                winRateText.text = winRate.ToString("F1") + "%";
                
                // Color code win rate
                winRateText.color = winRate >= 60f ? successColor : 
                                   winRate >= 40f ? primaryGlowColor : 
                                   dangerColor;
            }
            
            if (rankText != null)
            {
                rankText.text = "#42";
            }
        }

        private void AnimateStatText(TextMeshProUGUI text, int from, int to, Color color)
        {
            if (text == null) return;
            
            text.color = color;
            
            // Pulse animation
            AnimationController.Instance.Pulse(text.transform, 1.2f, 0.3f);
        }

        private void OnQueueButtonClicked()
        {
            if (!isInQueue)
            {
                NetworkManager.Instance.JoinQueue();
                isInQueue = true;
                UpdateQueueButtonState();
                
                if (queuePanel != null)
                {
                    queuePanel.SetActive(true);
                    AnimationController.Instance.ScaleIn(queuePanel.transform, 1f, 0.3f);
                }
                
                // Animate queue button
                if (queueButton != null)
                {
                    AnimationController.Instance.StopPulse(queueButton.transform);
                    
                    // Show loading state
                    Transform spinner = queueButton.transform.Find("Spinner");
                    if (spinner != null)
                    {
                        spinner.gameObject.SetActive(true);
                        AnimationController.Instance.RotateAnimation(spinner, 1f);
                    }
                    
                    // Change button text
                    TextMeshProUGUI buttonText = queueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "Searching...";
                    }
                }
            }
        }

        private void OnLeaveQueueButtonClicked()
        {
            if (isInQueue)
            {
                NetworkManager.Instance.LeaveQueue();
                isInQueue = false;
                UpdateQueueButtonState();
                
                if (queuePanel != null)
                {
                    AnimationController.Instance.ScaleOut(queuePanel.transform, 0.3f);
                    StartCoroutine(HideAfterAnimation(queuePanel, 0.3f));
                }
                
                // Reset queue button
                if (queueButton != null)
                {
                    AnimationController.Instance.ContinuousPulse(queueButton.transform, 1.05f, 2f);
                    
                    Transform spinner = queueButton.transform.Find("Spinner");
                    if (spinner != null)
                    {
                        AnimationController.Instance.StopRotateAnimation(spinner);
                        spinner.gameObject.SetActive(false);
                    }
                    
                    TextMeshProUGUI buttonText = queueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "FIND MATCH";
                    }
                }
            }
        }

        private void OnDisconnectButtonClicked()
        {
            // Confirm dialog
            ShowConfirmDialog("Are you sure you want to disconnect?", () =>
            {
                AuthManager.Instance.Logout();
                TransitionToLogin();
            });
        }

        private void OnSettingsButtonClicked()
        {
            // Show settings panel
            Debug.Log("Settings button clicked");
        }

        private void OnLeaderboardButtonClicked()
        {
            // Show/hide leaderboard
            if (leaderboardPanel != null)
            {
                bool isActive = leaderboardPanel.gameObject.activeSelf;
                
                if (isActive)
                {
                    AnimationController.Instance.ScaleOut(leaderboardPanel, 0.3f);
                    StartCoroutine(HideAfterAnimation(leaderboardPanel.gameObject, 0.3f));
                }
                else
                {
                    leaderboardPanel.gameObject.SetActive(true);
                    leaderboardPanel.localScale = Vector3.zero;
                    AnimationController.Instance.ScaleIn(leaderboardPanel, 1f, 0.4f);
                    LoadLeaderboard();
                }
            }
        }

        private IEnumerator HideAfterAnimation(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        private void ShowConfirmDialog(string message, System.Action onConfirm)
        {
            // Create simple confirm dialog
            GameObject dialogObj = new GameObject("ConfirmDialog");
            dialogObj.transform.SetParent(mainCanvasGroup.transform, false);
            
            RectTransform rt = dialogObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.3f, 0.4f);
            rt.anchorMax = new Vector2(0.7f, 0.6f);
            
            Image bg = dialogObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.06f, 0.15f, 0.95f);
            
            // Add glow effect
            GlowEffect glow = dialogObj.AddComponent<GlowEffect>();
            glow.SetGlowColor(dangerColor);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(dialogObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.white;
            
            // Add buttons
            GameObject yesButton = CreateDialogButton("Yes", dialogObj, new Vector2(-80f, -50f));
            GameObject noButton = CreateDialogButton("No", dialogObj, new Vector2(80f, -50f));
            
            yesButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                Destroy(dialogObj);
            });
            
            noButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Destroy(dialogObj);
            });
        }

        private GameObject CreateDialogButton(string text, GameObject parent, Vector2 position)
        {
            GameObject buttonObj = new GameObject(text + "Button");
            buttonObj.transform.SetParent(parent.transform, false);
            
            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(100f, 40f);
            
            Image bg = buttonObj.AddComponent<Image>();
            bg.color = primaryGlowColor;
            
            Button button = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.black;
            
            return buttonObj;
        }

        private void UpdateQueueButtonState()
        {
            if (queueButton != null)
            {
                queueButton.gameObject.SetActive(!isInQueue);
            }
            
            if (leaveQueueButton != null)
            {
                leaveQueueButton.gameObject.SetActive(isInQueue);
            }
        }

        private void HandleQueueStatus(QueueStatusData status)
        {
            if (queueStatusText != null && status != null)
            {
                if (status.position > 0)
                {
                    queueStatusText.text = $"Position: {status.position} | Wait: {status.estimatedWait}s";
                }
                else
                {
                    queueStatusText.text = "Searching for match...";
                }
            }
        }

        private void HandleMatchFound(MatchFoundData data)
        {
            Debug.Log($"Match found! Match ID: {data.matchId} Opponent: {data.opponent.username}");
            
            // Confetti effect
            if (ParticleController.Instance != null)
            {
                Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                ParticleController.Instance.SpawnConfettiEffect(center, 50);
            }
            
            // Mark player as ready for the match
            NetworkManager.Instance.MarkMatchReady(data.matchId);
            
            // Stop particles
            ParticleController.Instance?.StopBackgroundParticles();
            
            // Transition to game
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.TransitionToMatch();
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        private void LoadLeaderboard()
        {
            if (leaderboardContent == null || leaderboardEntryPrefab == null) return;
            
            // Clear existing entries
            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }
            
            // Add placeholder entries with staggered animation
            for (int i = 0; i < 10; i++)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                LeaderboardEntry entry = entryObj.GetComponent<LeaderboardEntry>();
                if (entry != null)
                {
                    entry.SetData(i + 1, "Player" + (i + 1), 1000 + (10 - i) * 50, 20 - i, i);
                    
                    // Staggered fade in
                    CanvasGroup canvasGroup = entryObj.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = entryObj.AddComponent<CanvasGroup>();
                    }
                    
                    canvasGroup.alpha = 0f;
                    StartCoroutine(DelayedFadeIn(canvasGroup, i * 0.05f));
                }
            }
        }

        private IEnumerator DelayedFadeIn(CanvasGroup canvasGroup, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }

        private void TransitionToLogin()
        {
            ParticleController.Instance?.StopBackgroundParticles();
            
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadScene("LoginScene", TransitionType.Fade);
            }
            else
            {
                SceneManager.LoadScene("LoginScene");
            }
        }

        private void SubscribeToEvents()
        {
            var nem = NetworkEventManager.GetInstance(false);
            if (nem != null)
            {
                nem.OnQueueStatusReceived += HandleQueueStatus;
                nem.OnMatchFoundReceived += HandleMatchFound;
            }
        }

        #region Easing Functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        #endregion

        private void OnDestroy()
        {
            var nem = NetworkEventManager.GetInstance(false);
            if (nem != null)
            {
                nem.OnQueueStatusReceived -= HandleQueueStatus;
                nem.OnMatchFoundReceived -= HandleMatchFound;
            }

            ParticleController.Instance?.StopBackgroundParticles();
        }
    }
}
