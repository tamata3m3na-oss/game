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
    /// Enhanced Result Scene with premium victory/defeat presentation
    /// Includes confetti, stats cards, rating animations, and smooth transitions
    /// </summary>
    public class ResultSceneUI : MonoBehaviour
    {
        [Header("Result Display")]
        public TextMeshProUGUI resultTitle;
        public TextMeshProUGUI winnerText;
        public Transform resultPanel;
        public Image backgroundGradient;
        
        [Header("Stats Cards")]
        public Transform statsContainer;
        public GameObject statsCardPrefab;
        public TextMeshProUGUI yourHealthText;
        public TextMeshProUGUI opponentHealthText;
        public TextMeshProUGUI yourDamageText;
        public TextMeshProUGUI opponentDamageText;
        public TextMeshProUGUI durationText;
        
        [Header("Rating Display")]
        public TextMeshProUGUI ratingBeforeText;
        public TextMeshProUGUI ratingAfterText;
        public Image ratingArrow;
        public Transform ratingDisplay;
        
        [Header("Rank Display")]
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI rankChangeText;
        public Transform rankBadge;
        
        [Header("Opponent Info")]
        public TextMeshProUGUI opponentNameText;
        public Image opponentAvatar;
        
        [Header("Buttons")]
        public Button nextMatchButton;
        public Button leaderboardButton;
        public Button lobbyButton;
        public Transform buttonsContainer;
        
        [Header("Visual Settings")]
        [SerializeField] private Color winColor = new Color(0f, 1f, 0.53f, 1f); // Green-cyan
        [SerializeField] private Color lossColor = new Color(1f, 0f, 0.27f, 1f); // Red-magenta
        [SerializeField] private Color primaryColor = new Color(0f, 0.83f, 1f, 1f); // Cyan
        
        private bool isVictory = false;
        private int ratingBefore = 1000;
        private int ratingAfter = 1000;

        private void Start()
        {
            // Initialize managers
            InitializeManagers();
            
            // Setup buttons
            SetupButtons();
            
            // Subscribe to events
            SubscribeToEvents();
            
            // Hide initially
            gameObject.SetActive(false);
        }

        private void InitializeManagers()
        {
            if (AnimationController.Instance == null)
            {
                GameObject animControllerObj = new GameObject("AnimationController");
                animControllerObj.AddComponent<AnimationController>();
            }

            if (ParticleController.Instance == null)
            {
                GameObject particleControllerObj = new GameObject("ParticleController");
                particleControllerObj.AddComponent<ParticleController>();
            }

            if (TransitionManager.Instance == null)
            {
                GameObject transitionManagerObj = new GameObject("TransitionManager");
                transitionManagerObj.AddComponent<TransitionManager>();
            }
        }

        private void SetupButtons()
        {
            if (nextMatchButton != null)
            {
                nextMatchButton.onClick.AddListener(OnNextMatchClicked);
                nextMatchButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(nextMatchButton.transform));
                
                ApplyGlowEffect(nextMatchButton, primaryColor, GlowMode.Pulse);
            }
            
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
                leaderboardButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(leaderboardButton.transform));
                
                ApplyGlowEffect(leaderboardButton, primaryColor, GlowMode.OnHover);
            }
            
            if (lobbyButton != null)
            {
                lobbyButton.onClick.AddListener(OnLobbyClicked);
                lobbyButton.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(lobbyButton.transform));
                
                ApplyGlowEffect(lobbyButton, primaryColor, GlowMode.OnHover);
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

        private void SubscribeToEvents()
        {
            GameStateManager.Instance.OnGameEnded.AddListener(ShowResultScreen);
        }

        public void ShowResultScreen(int winnerId)
        {
            gameObject.SetActive(true);
            
            // Get game end data from GameStateManager
            var gameState = GameStateManager.Instance.GetCurrentGameState();
            
            if (gameState == null) return;
            
            // Determine if local player won
            int localPlayerId = AuthManager.Instance.GetUserId();
            isVictory = winnerId == localPlayerId;
            
            // Apply victory/defeat theme
            ApplyTheme(isVictory);
            
            // Play entrance animations
            StartCoroutine(PlayEntranceAnimations());
            
            // Calculate and display stats
            DisplayStats(gameState, localPlayerId);
        }

        private void ApplyTheme(bool victory)
        {
            Color themeColor = victory ? winColor : lossColor;
            
            // Set background gradient
            if (backgroundGradient != null)
            {
                backgroundGradient.color = themeColor * 0.3f;
            }
            
            // Set result title
            if (resultTitle != null)
            {
                resultTitle.text = victory ? "VICTORY" : "DEFEAT";
                resultTitle.color = themeColor;
            }
            
            // Set winner text
            if (winnerText != null)
            {
                winnerText.text = victory ? "YOU WIN!" : "YOU LOSE";
                winnerText.color = themeColor;
            }
            
            // Apply bloom effect
            if (resultTitle != null)
            {
                BloomEffect bloom = resultTitle.GetComponent<BloomEffect>();
                if (bloom == null)
                {
                    bloom = resultTitle.gameObject.AddComponent<BloomEffect>();
                }
                bloom.SetBloomColor(themeColor);
                bloom.StartPulse();
            }
        }

        private IEnumerator PlayEntranceAnimations()
        {
            // Fade in background
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            AnimationController.Instance.FadeIn(canvasGroup, 0.3f);
            
            // Spawn confetti or dark particles based on result
            if (isVictory)
            {
                yield return new WaitForSeconds(0.1f);
                
                // Confetti burst
                if (ParticleController.Instance != null)
                {
                    Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                    ParticleController.Instance.SpawnConfettiEffect(center, 100);
                }
            }
            else
            {
                // Dark particles falling
                if (ParticleController.Instance != null)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Vector3 randomPos = new Vector3(
                            Random.Range(-5f, 5f),
                            Random.Range(3f, 6f),
                            0f
                        );
                        ParticleController.Instance.SpawnParticle("impact", randomPos, 1f);
                    }
                }
            }
            
            // Result title scale in
            if (resultTitle != null)
            {
                AnimationController.Instance.ScaleIn(resultTitle.transform, 1.2f, 0.5f);
                
                // Shimmer effect
                StartCoroutine(ShimmerEffect(resultTitle));
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // Stats cards scale in (staggered)
            Transform[] statsElements = new Transform[]
            {
                yourHealthText?.transform,
                opponentHealthText?.transform,
                yourDamageText?.transform,
                opponentDamageText?.transform,
                durationText?.transform
            };
            
            AnimationController.Instance.StaggeredScaleIn(statsElements, 0.1f, 0.4f);
            
            yield return new WaitForSeconds(0.5f);
            
            // Rating counter animation
            yield return StartCoroutine(AnimateRatingChange());
            
            // Rank animation
            if (rankBadge != null)
            {
                AnimationController.Instance.ScaleIn(rankBadge, 1f, 0.5f);
                
                if (isVictory)
                {
                    // Celebration effect on rank up
                    AnimationController.Instance.Pulse(rankBadge, 1.2f, 0.5f);
                }
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // Buttons slide up
            if (buttonsContainer != null)
            {
                AnimationController.Instance.SlideUp(buttonsContainer, 50f, 0.5f);
            }
        }

        private IEnumerator ShimmerEffect(TextMeshProUGUI text)
        {
            if (text == null) yield break;
            
            Color originalColor = text.color;
            
            // Shimmer across text (simplified as color pulse)
            float shimmerSpeed = 0.8f;
            float elapsed = 0f;
            
            while (elapsed < shimmerSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / shimmerSpeed;
                
                // Vary brightness
                float brightness = 1f + 0.2f * Mathf.Sin(t * Mathf.PI * 2);
                text.color = originalColor * brightness;
                
                yield return null;
            }
            
            text.color = originalColor;
        }

        private void DisplayStats(GameState gameState, int localPlayerId)
        {
            // Calculate stats
            int yourHealth = gameState.player1.id == localPlayerId ? gameState.player1.health : gameState.player2.health;
            int opponentHealth = gameState.player1.id == localPlayerId ? gameState.player2.health : gameState.player1.health;
            int yourDamage = gameState.player1.id == localPlayerId ? gameState.player1.damageDealt : gameState.player2.damageDealt;
            int opponentDamage = gameState.player1.id == localPlayerId ? gameState.player2.damageDealt : gameState.player1.damageDealt;
            
            // Update stats display
            if (yourHealthText != null)
            {
                yourHealthText.text = yourHealth.ToString();
                yourHealthText.color = healthLowColor; // Using red for health display
            }
            
            if (opponentHealthText != null)
            {
                opponentHealthText.text = opponentHealth.ToString();
                opponentHealthText.color = healthLowColor;
            }
            
            if (yourDamageText != null)
            {
                yourDamageText.text = yourDamage.ToString();
                yourDamageText.color = primaryColor;
            }
            
            if (opponentDamageText != null)
            {
                opponentDamageText.text = opponentDamage.ToString();
                opponentDamageText.color = primaryColor;
            }
            
            // Calculate duration (assuming 20Hz tick rate, each tick is 50ms)
            float durationSeconds = gameState.tick * 0.05f;
            if (durationText != null)
            {
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                durationText.text = string.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);
            }
            
            // Rating change (placeholder)
            int ratingChange = isVictory ? 15 : -10;
            ratingAfter = ratingBefore + ratingChange;
            
            if (ratingBeforeText != null)
            {
                ratingBeforeText.text = ratingBefore.ToString();
                ratingBeforeText.alpha = 0.5f;
            }
            
            if (ratingAfterText != null)
            {
                ratingAfterText.text = ratingAfter.ToString();
                ratingAfterText.color = ratingChange >= 0 ? winColor : lossColor;
            }
            
            if (ratingArrow != null)
            {
                // Rotate arrow up or down
                float rotation = ratingChange >= 0 ? 0f : 180f;
                ratingArrow.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
                ratingArrow.color = ratingChange >= 0 ? winColor : lossColor;
            }
        }

        private Color healthLowColor => new Color(1f, 0.27f, 0.27f, 1f);

        private IEnumerator AnimateRatingChange()
        {
            if (ratingAfterText == null) yield break;
            
            int currentValue = ratingBefore;
            int targetValue = ratingAfter;
            float duration = 1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                currentValue = Mathf.RoundToInt(Mathf.Lerp(ratingBefore, ratingAfter, EaseOutQuad(t)));
                ratingAfterText.text = currentValue.ToString();
                
                // Scale animation during count
                float scale = 1f + 0.1f * Mathf.Sin(t * Mathf.PI);
                ratingAfterText.transform.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            ratingAfterText.text = ratingAfter.ToString();
            ratingAfterText.transform.localScale = Vector3.one;
            
            // Pulse at end
            AnimationController.Instance.Pulse(ratingDisplay, 1.1f, 0.3f);
            
            // Arrow animation
            if (ratingArrow != null)
            {
                AnimationController.Instance.Pulse(ratingArrow.transform, 1.2f, 0.3f);
            }
        }

        #region Easing Functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        #endregion

        private void OnNextMatchClicked()
        {
            // Transition to lobby to queue again
            TransitionToLobby();
        }

        private void OnLeaderboardClicked()
        {
            // Go to leaderboard
            TransitionToLobby();
            
            // Note: In a full implementation, this would show the leaderboard panel
        }

        private void OnLobbyClicked()
        {
            TransitionToLobby();
        }

        private void TransitionToLobby()
        {
            if (TransitionManager.Instance != null)
            {
                if (isVictory)
                {
                    TransitionManager.Instance.VictoryTransition("LobbyScene");
                }
                else
                {
                    TransitionManager.Instance.DefeatTransition("LobbyScene");
                }
            }
            else
            {
                SceneManager.LoadScene("LobbyScene");
            }
        }

        public void HideResultScreen()
        {
            // Play exit animation
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                AnimationController.Instance.FadeOut(canvasGroup, 0.3f);
            }
            
            StartCoroutine(HideAfterFade());
        }

        private IEnumerator HideAfterFade()
        {
            yield return new WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameEnded.RemoveListener(ShowResultScreen);
            }
        }
    }
}