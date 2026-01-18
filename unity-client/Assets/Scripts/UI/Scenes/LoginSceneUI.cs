using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UI.Animations;
using UI.Effects;

namespace UI.Scenes
{
    /// <summary>
    /// Enhanced Login Scene with premium visual effects
    /// Includes animated background, glassmorphism, and smooth transitions
    /// </summary>
    public class LoginSceneUI : MonoBehaviour
    {
        [Header("Login Panel")]
        public TMP_InputField emailInputField;
        public TMP_InputField passwordInputField;
        public Button loginButton;
        public TextMeshProUGUI loginErrorText;
        public Transform loginPanel;
        public Transform logoTransform;
        
        [Header("Register Panel")]
        public TMP_InputField registerUsernameInput;
        public TMP_InputField registerEmailInput;
        public TMP_InputField registerPasswordInput;
        public Button registerButton;
        public TextMeshProUGUI registerErrorText;
        public TextMeshProUGUI registerSuccessText;
        public Transform registerPanel;
        
        [Header("UI Effects")]
        public Image backgroundImage;
        public CanvasGroup loginCanvasGroup;
        public CanvasGroup registerCanvasGroup;
        public GameObject loadingSpinner;
        
        [Header("Visual Settings")]
        [SerializeField] private Color primaryGlowColor = new Color(0f, 0.83f, 1f, 1f); // Cyan
        [SerializeField] private Color errorColor = new Color(1f, 0.27f, 0.27f, 1f); // Red
        
        private bool isLoading = false;

        private void Start()
        {
            // Initialize animation system
            if (AnimationController.Instance == null)
            {
                GameObject animControllerObj = new GameObject("AnimationController");
                animControllerObj.AddComponent<AnimationController>();
            }

            // Initialize particle system
            if (ParticleController.Instance == null)
            {
                GameObject particleControllerObj = new GameObject("ParticleController");
                particleControllerObj.AddComponent<ParticleController>();
            }

            // Set up button listeners
            SetupButtons();
            
            // Subscribe to auth events
            SubscribeToAuthEvents();
            
            // Check if already logged in
            if (AuthManager.Instance.IsLoggedIn())
            {
                TransitionToLobby();
                return;
            }

            // Start background particles
            ParticleController.Instance?.StartBackgroundParticles();

            // Play entrance animations
            PlayEntranceAnimations();
            
            // Apply visual effects
            ApplyVisualEffects();
        }

        private void SetupButtons()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
                
                // Add button press animation
                Button btnAnim = loginButton.GetComponent<Button>();
                if (btnAnim != null)
                {
                    btnAnim.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(loginButton.transform));
                }
            }
            
            if (registerButton != null)
            {
                registerButton.onClick.AddListener(OnRegisterButtonClicked);
                
                // Add button press animation
                Button btnAnim = registerButton.GetComponent<Button>();
                if (btnAnim != null)
                {
                    btnAnim.onClick.AddListener(() => AnimationController.Instance?.ButtonPress(registerButton.transform));
                }
            }
        }

        private void PlayEntranceAnimations()
        {
            // Fade in background
            if (loginCanvasGroup != null)
            {
                AnimationController.Instance.FadeIn(loginCanvasGroup, 0.3f);
            }

            // Logo animation: Scale in + rotate
            if (logoTransform != null)
            {
                AnimationController.Instance.ScaleIn(logoTransform, 1f, 0.5f);
                
                // Add continuous float animation to logo
                AnimationController.Instance.FloatAnimation(logoTransform, 10f, 2f);
                
                // Add subtle rotation
                AnimationController.Instance.RotateAnimation(logoTransform, 6f);
            }

            // Input fields slide up
            if (emailInputField != null)
            {
                AnimationController.Instance.SlideUp(emailInputField.transform, 50f, 0.5f);
            }

            if (passwordInputField != null)
            {
                AnimationController.Instance.SlideUp(passwordInputField.transform, 50f, 0.5f);
            }

            // Button slide up with bounce
            if (loginButton != null)
            {
                AnimationController.Instance.SlideUp(loginButton.transform, 50f, 0.5f);
                
                // Add continuous pulse animation
                AnimationController.Instance.ContinuousPulse(loginButton.transform, 1.05f, 2f);
            }
        }

        private void ApplyVisualEffects()
        {
            // Apply glow effects to buttons
            ApplyGlowEffect(loginButton, primaryGlowColor, GlowMode.Pulse);
            ApplyGlowEffect(registerButton, primaryGlowColor, GlowMode.Pulse);
            
            // Apply bloom effect to logo
            BloomEffect logoBloom = logoTransform?.GetComponent<BloomEffect>();
            if (logoBloom == null && logoTransform != null)
            {
                logoBloom = logoTransform.gameObject.AddComponent<BloomEffect>();
                logoBloom.SetCyanBloom();
                logoBloom.StartPulse();
            }
            
            // Apply glassmorphism to input fields (visual only)
            ApplyGlassmorphismEffect(emailInputField);
            ApplyGlassmorphismEffect(passwordInputField);
            ApplyGlassmorphismEffect(registerUsernameInput);
            ApplyGlassmorphismEffect(registerEmailInput);
            ApplyGlassmorphismEffect(registerPasswordInput);
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

        private void ApplyGlassmorphismEffect(TMP_InputField inputField)
        {
            if (inputField == null) return;
            
            // Add glow effect on focus
            GlowEffect glow = inputField.GetComponent<GlowEffect>();
            if (glow == null)
            {
                glow = inputField.gameObject.AddComponent<GlowEffect>();
            }
            glow.SetGlowColor(primaryGlowColor);
            glow.SetGlowMode(GlowMode.OnHover);
        }

        private void SubscribeToAuthEvents()
        {
            AuthManager.Instance.OnLoginSuccess.AddListener(HandleLoginSuccess);
            AuthManager.Instance.OnLoginFailed.AddListener(HandleLoginFailed);
            AuthManager.Instance.OnRegisterSuccess.AddListener(HandleRegisterSuccess);
            AuthManager.Instance.OnRegisterFailed.AddListener(HandleRegisterFailed);
        }

        private void OnLoginButtonClicked()
        {
            if (emailInputField == null || passwordInputField == null) return;
            
            string email = emailInputField.text;
            string password = passwordInputField.text;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Please enter email and password");
                ShakeElement(emailInputField?.transform);
                return;
            }
            
            ClearLoginError();
            SetLoading(true);
            AuthManager.Instance.Login(email, password);
        }

        private void OnRegisterButtonClicked()
        {
            if (registerUsernameInput == null || registerEmailInput == null || registerPasswordInput == null) return;
            
            string username = registerUsernameInput.text;
            string email = registerEmailInput.text;
            string password = registerPasswordInput.text;
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowRegisterError("Please fill all fields");
                ShakeElement(registerUsernameInput?.transform);
                return;
            }
            
            if (password.Length < 6)
            {
                ShowRegisterError("Password must be at least 6 characters");
                ShakeElement(registerPasswordInput?.transform);
                return;
            }
            
            ClearRegisterError();
            SetLoading(true);
            AuthManager.Instance.Register(username, email, password);
        }

        private void HandleLoginSuccess(string message)
        {
            SetLoading(false);
            Debug.Log("Login successful: " + message);
            
            // Stop particles and transition
            ParticleController.Instance?.StopBackgroundParticles();
            TransitionToLobby();
        }

        private void HandleLoginFailed(string error)
        {
            SetLoading(false);
            ShowLoginError(error);
            Debug.LogError("Login failed: " + error);
            
            // Shake effect on error
            ShakeElement(loginButton?.transform);
        }

        private void HandleRegisterSuccess(string message)
        {
            SetLoading(false);
            ShowRegisterSuccess(message);
            Debug.Log("Registration successful: " + message);
            
            // Switch to login panel with animation
            SwitchToLoginPanel();
        }

        private void HandleRegisterFailed(string error)
        {
            SetLoading(false);
            ShowRegisterError(error);
            Debug.LogError("Registration failed: " + error);
            
            // Shake effect on error
            ShakeElement(registerButton?.transform);
        }

        private void ShakeElement(Transform element)
        {
            if (element != null)
            {
                ShakeEffect shake = element.GetComponent<ShakeEffect>();
                if (shake == null)
                {
                    shake = element.gameObject.AddComponent<ShakeEffect>();
                }
                shake.ErrorShake();
            }
        }

        private void SetLoading(bool loading)
        {
            isLoading = loading;
            
            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(loading);
                
                if (loading)
                {
                    AnimationController.Instance.RotateAnimation(loadingSpinner.transform, 1f);
                }
            }
            
            if (loginButton != null)
            {
                loginButton.interactable = !loading;
            }
            
            if (registerButton != null)
            {
                registerButton.interactable = !loading;
            }
        }

        #region Display Methods
        private void ShowLoginError(string error)
        {
            if (loginErrorText != null)
            {
                loginErrorText.text = error;
                loginErrorText.color = errorColor;
                loginErrorText.gameObject.SetActive(true);
                
                // Add shake effect
                ShakeEffect shake = loginErrorText.GetComponent<ShakeEffect>();
                if (shake == null)
                {
                    shake = loginErrorText.gameObject.AddComponent<ShakeEffect>();
                }
                shake.ErrorShake();
            }
        }

        private void ClearLoginError()
        {
            if (loginErrorText != null)
            {
                loginErrorText.gameObject.SetActive(false);
            }
        }

        private void ShowRegisterError(string error)
        {
            if (registerErrorText != null)
            {
                registerErrorText.text = error;
                registerErrorText.color = errorColor;
                registerErrorText.gameObject.SetActive(true);
                
                // Add shake effect
                ShakeEffect shake = registerErrorText.GetComponent<ShakeEffect>();
                if (shake == null)
                {
                    shake = registerErrorText.gameObject.AddComponent<ShakeEffect>();
                }
                shake.ErrorShake();
            }
        }

        private void ShowRegisterSuccess(string message)
        {
            if (registerSuccessText != null)
            {
                registerSuccessText.text = message;
                registerSuccessText.color = GlowEffect.GreenGlow;
                registerSuccessText.gameObject.SetActive(true);
                
                // Add pulse animation
                AnimationController.Instance.Pulse(registerSuccessText.transform, 1.1f, 0.5f);
            }
        }

        private void ClearRegisterError()
        {
            if (registerErrorText != null)
            {
                registerErrorText.gameObject.SetActive(false);
            }
            
            if (registerSuccessText != null)
            {
                registerSuccessText.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Panel Switching
        public void SwitchToRegisterPanel()
        {
            if (loginPanel != null)
            {
                AnimationController.Instance.ScaleOut(loginPanel, 0.3f);
                StartCoroutine(HideAfterAnimation(loginPanel.gameObject, 0.3f));
            }
            
            if (registerPanel != null)
            {
                registerPanel.gameObject.SetActive(true);
                registerPanel.localScale = Vector3.zero;
                AnimationController.Instance.ScaleIn(registerPanel, 1f, 0.4f);
            }
            
            ClearLoginError();
            ClearRegisterError();
        }

        public void SwitchToLoginPanel()
        {
            if (registerPanel != null)
            {
                AnimationController.Instance.ScaleOut(registerPanel, 0.3f);
                StartCoroutine(HideAfterAnimation(registerPanel.gameObject, 0.3f));
            }
            
            if (loginPanel != null)
            {
                loginPanel.gameObject.SetActive(true);
                loginPanel.localScale = Vector3.zero;
                AnimationController.Instance.ScaleIn(loginPanel, 1f, 0.4f);
            }
            
            ClearLoginError();
            ClearRegisterError();
        }

        private System.Collections.IEnumerator HideAfterAnimation(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        #endregion

        private void TransitionToLobby()
        {
            // Use transition manager for smooth scene change
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadScene("LobbyScene", TransitionType.SlideUp);
            }
            else
            {
                SceneManager.LoadScene("LobbyScene");
            }
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess.RemoveListener(HandleLoginSuccess);
                AuthManager.Instance.OnLoginFailed.RemoveListener(HandleLoginFailed);
                AuthManager.Instance.OnRegisterSuccess.RemoveListener(HandleRegisterSuccess);
                AuthManager.Instance.OnRegisterFailed.RemoveListener(HandleRegisterFailed);
            }

            // Stop particles
            ParticleController.Instance?.StopBackgroundParticles();
        }
    }
}
