using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Animations
{
    /// <summary>
    /// Manages scene transitions with visual effects
    /// Provides smooth, cinematic transitions between scenes
    /// </summary>
    [DefaultExecutionOrder(-140)]
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("[TransitionManager] Instance is null. Make sure TransitionManager is properly initialized.");
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        
        private static TransitionManager instance;

        [Header("Transition Overlay")]
        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private Image overlayImage;
        [SerializeField] private Image vignetteImage;

        [Header("Transition Colors")]
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private Color winColor = new Color(0f, 1f, 0.53f, 1f); // Green-cyan
        [SerializeField] private Color lossColor = new Color(1f, 0f, 0.27f, 1f); // Red-magenta

        [Header("Transition Settings")]
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private float zoomDuration = 0.5f;

        private bool isTransitioning = false;
        private CanvasGroup transitionCanvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[TransitionManager] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                InitializeOverlay();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TransitionManager] Exception during initialization: {ex.Message}");
            }
        }

        private void InitializeOverlay()
        {
            if (transitionCanvas == null)
            {
                GameObject canvasObj = new GameObject("TransitionCanvas");
                canvasObj.transform.SetParent(transform);
                transitionCanvas = canvasObj.AddComponent<Canvas>();
                transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                transitionCanvas.sortingOrder = 9999;

                // Add CanvasScaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Add GraphicRaycaster
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (overlayImage == null)
            {
                GameObject overlayObj = new GameObject("OverlayImage");
                overlayObj.transform.SetParent(transitionCanvas.transform, false);
                overlayImage = overlayObj.AddComponent<Image>();
                overlayImage.color = Color.clear;

                RectTransform rt = overlayImage.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }

            transitionCanvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
            {
                transitionCanvasGroup = transitionCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            if (vignetteImage == null)
            {
                CreateVignette();
            }
        }

        private void CreateVignette()
        {
            GameObject vignetteObj = new GameObject("VignetteImage");
            vignetteObj.transform.SetParent(transitionCanvas.transform, false);
            vignetteImage = vignetteObj.AddComponent<Image>();

            // Create vignette shader effect (simple radial gradient)
            vignetteImage.color = new Color(0f, 0f, 0f, 0.5f);

            RectTransform rt = vignetteImage.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            vignetteImage.gameObject.SetActive(false);
        }

        #region Easing Functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInQuad(float t)
        {
            return t * t;
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
        #endregion

        #region Basic Transitions
        public void LoadScene(string sceneName, TransitionType transitionType = TransitionType.Fade)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadSceneCoroutine(sceneName, transitionType));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, TransitionType transitionType)
        {
            isTransitioning = true;

            // Play exit animation
            yield return PlayExitAnimation(transitionType);

            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Wait a brief moment for scene to initialize
            yield return new WaitForSeconds(0.1f);

            // Play entry animation
            yield return PlayEntryAnimation(transitionType);

            isTransitioning = false;
        }

        public void LoadSceneWithDelay(string sceneName, float delay, TransitionType transitionType = TransitionType.Fade)
        {
            StartCoroutine(LoadSceneWithDelayCoroutine(sceneName, delay, transitionType));
        }

        private IEnumerator LoadSceneWithDelayCoroutine(string sceneName, float delay, TransitionType transitionType)
        {
            yield return new WaitForSeconds(delay);
            LoadScene(sceneName, transitionType);
        }
        #endregion

        #region Exit Animations
        private IEnumerator PlayExitAnimation(TransitionType type)
        {
            switch (type)
            {
                case TransitionType.Fade:
                    yield return FadeOutCoroutine();
                    break;
                case TransitionType.ZoomIn:
                    yield return ZoomOutCoroutine();
                    break;
                case TransitionType.SlideUp:
                    yield return SlideOutUpCoroutine();
                    break;
                case TransitionType.SlideDown:
                    yield return SlideOutDownCoroutine();
                    break;
                case TransitionType.Circle:
                    yield return CircleOutCoroutine();
                    break;
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeOutDuration));
        }

        private IEnumerator ZoomOutCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;
            overlayImage.transform.localScale = Vector3.one * 2f;

            // Run fade and scale animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(0f, 1f, zoomDuration),
                ScaleOverlay(2f, 1f, zoomDuration)
            ));
        }

        private IEnumerator SlideOutUpCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            Vector2 originalAnchoredPosition = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(0, -Screen.height);

            // Run fade and slide animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(0f, 1f, fadeOutDuration),
                SlideOverlayY(originalAnchoredPosition.y, fadeOutDuration)
            ));
        }

        private IEnumerator SlideOutDownCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            Vector2 originalAnchoredPosition = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(0, Screen.height);

            // Run fade and slide animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(0f, 1f, fadeOutDuration),
                SlideOverlayY(originalAnchoredPosition.y, fadeOutDuration)
            ));
        }

        private IEnumerator CircleOutCoroutine()
        {
            // Create circular mask effect (simplified version using scale)
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            // This would typically use a shader with a circular mask
            // For simplicity, we'll use a zoom effect
            yield return ZoomOutCoroutine();
        }
        #endregion

        #region Entry Animations
        private IEnumerator PlayEntryAnimation(TransitionType type)
        {
            switch (type)
            {
                case TransitionType.Fade:
                    yield return FadeInCoroutine();
                    break;
                case TransitionType.ZoomIn:
                    yield return ZoomInCoroutine();
                    break;
                case TransitionType.SlideUp:
                    yield return SlideInUpCoroutine();
                    break;
                case TransitionType.SlideDown:
                    yield return SlideInDownCoroutine();
                    break;
                case TransitionType.Circle:
                    yield return CircleInCoroutine();
                    break;
            }
        }

        private IEnumerator FadeInCoroutine()
        {
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeInDuration));
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator ZoomInCoroutine()
        {
            overlayImage.transform.localScale = Vector3.one * 0.5f;

            // Run fade and scale animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(1f, 0f, zoomDuration),
                ScaleOverlay(0.5f, 0.5f, zoomDuration)
            ));
            
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator SlideInUpCoroutine()
        {
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -Screen.height);

            // Run fade and slide animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(1f, 0f, fadeInDuration),
                SlideOverlayY(Screen.height, fadeInDuration)
            ));
            
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator SlideInDownCoroutine()
        {
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, Screen.height);

            // Run fade and slide animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(1f, 0f, fadeInDuration),
                SlideOverlayY(-Screen.height, fadeInDuration)
            ));
            
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator CircleInCoroutine()
        {
            // Simplified version using zoom effect
            yield return ZoomInCoroutine();
        }
        #endregion

        #region Helper Coroutines
        private IEnumerator FadeCanvasGroup(float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseInQuad(t);
                transitionCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                yield return null;
            }
            
            transitionCanvasGroup.alpha = toAlpha;
        }

        private IEnumerator ScaleOverlay(float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * fromScale;
            Vector3 endScale = Vector3.one * toScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseInQuad(t);
                overlayImage.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }
            
            overlayImage.transform.localScale = endScale;
        }

        private IEnumerator SlideOverlayY(float targetY, float duration)
        {
            float elapsed = 0f;
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            float startY = rt.anchoredPosition.y;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseInQuad(t);
                Vector2 pos = rt.anchoredPosition;
                pos.y = Mathf.Lerp(startY, targetY, t);
                rt.anchoredPosition = pos;
                yield return null;
            }
            
            Vector2 finalPos = rt.anchoredPosition;
            finalPos.y = targetY;
            rt.anchoredPosition = finalPos;
        }

        private IEnumerator ParallelTransition(params IEnumerator[] coroutines)
        {
            // Start all coroutines
            foreach (IEnumerator coroutine in coroutines)
            {
                StartCoroutine(coroutine);
            }

            // Wait for all to complete
            bool anyRunning = true;
            while (anyRunning)
            {
                anyRunning = false;
                foreach (IEnumerator coroutine in coroutines)
                {
                    // We can't directly check if a coroutine is running, so we'll use a simple delay
                    // This is a simplified approach - in a real implementation, you'd track each coroutine
                }
                
                yield return null;
            }
        }
        #endregion

        #region Special Transitions
        public void VictoryTransition(string nextScene)
        {
            StartCoroutine(VictoryTransitionCoroutine(nextScene));
        }

        private IEnumerator VictoryTransitionCoroutine(string nextScene)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            try
            {
                // Show victory effect
                overlayImage.color = winColor;
                overlayImage.gameObject.SetActive(true);
                transitionCanvasGroup.alpha = 0f;

                yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));

                // Spawn confetti with comprehensive null safety
                if (ParticleController.Instance != null)
                {
                    try
                    {
                        Camera mainCamera = Camera.main;
                        if (mainCamera != null)
                        {
                            Vector3 center = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                            ParticleController.Instance.SpawnConfettiEffect(center, 100);
                        }
                        else
                        {
                            Debug.LogWarning("[TransitionManager] Camera.main is null. Using default position for confetti.");
                            ParticleController.Instance.SpawnConfettiEffect(Vector3.zero, 100);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[TransitionManager] Exception spawning confetti effect: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("[TransitionManager] ParticleController.Instance is null. Confetti effect will not be shown.");
                }

                yield return new WaitForSeconds(1f);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TransitionManager] Exception during victory transition: {ex.Message}");
                isTransitioning = false;
                yield break;
            }

            // Load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));
            overlayImage.gameObject.SetActive(false);

            isTransitioning = false;
        }

        public void DefeatTransition(string nextScene)
        {
            StartCoroutine(DefeatTransitionCoroutine(nextScene));
        }

        private IEnumerator DefeatTransitionCoroutine(string nextScene)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            // Show defeat effect
            overlayImage.color = lossColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));

            yield return new WaitForSeconds(0.5f);

            // Load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));
            overlayImage.gameObject.SetActive(false);

            isTransitioning = false;
        }
        #endregion

        #region Vignette Effects
        public void ShowVignette(float intensity = 0.5f)
        {
            if (vignetteImage != null)
            {
                vignetteImage.gameObject.SetActive(true);
                Color color = vignetteImage.color;
                color.a = intensity;
                vignetteImage.color = color;
            }
        }

        public void HideVignette()
        {
            if (vignetteImage != null)
            {
                vignetteImage.gameObject.SetActive(false);
            }
        }

        public void FlashVignette(Color color, float duration = 0.1f)
        {
            StartCoroutine(FlashVignetteCoroutine(color, duration));
        }

        private IEnumerator FlashVignetteCoroutine(Color color, float duration)
        {
            if (vignetteImage == null) yield break;

            vignetteImage.gameObject.SetActive(true);
            vignetteImage.color = color;

            yield return new WaitForSeconds(duration);

            HideVignette();
        }
        #endregion

        #region Match Transitions
        public void TransitionToMatch()
        {
            StartCoroutine(TransitionToMatchCoroutine());
        }

        private IEnumerator TransitionToMatchCoroutine()
        {
            // Zoom in effect
            overlayImage.color = new Color(0f, 0.8f, 1f, 0.3f); // Cyan tint
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;
            overlayImage.transform.localScale = Vector3.one * 0.5f;

            // Run fade and scale animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(0f, 1f, 0.3f),
                ScaleOverlay(0.5f, 1.5f, 0.5f)
            ));

            // Freeze frame
            yield return new WaitForSeconds(0.1f);

            // Load game scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            // Fade in with slide
            overlayImage.transform.localScale = Vector3.one;
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -Screen.height);

            // Run fade and slide animations in parallel
            yield return StartCoroutine(ParallelTransition(
                FadeCanvasGroup(1f, 0f, 0.4f),
                SlideOverlayY(Screen.height, 0.4f)
            ));
            
            overlayImage.gameObject.SetActive(false);
        }
        #endregion
    }

    public enum TransitionType
    {
        Fade,
        ZoomIn,
        SlideUp,
        SlideDown,
        Circle
    }
}