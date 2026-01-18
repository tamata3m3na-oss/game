using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace UI.Animations
{
    /// <summary>
    /// Manages scene transitions with visual effects
    /// Provides smooth, cinematic transitions between scenes
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }

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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            InitializeOverlay();
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

            yield return transitionCanvasGroup.DOFade(1f, fadeOutDuration).SetEase(Ease.OutQuad).WaitForCompletion();
        }

        private IEnumerator ZoomOutCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;
            overlayImage.transform.localScale = Vector3.one * 2f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(1f, zoomDuration).SetEase(Ease.OutQuad));
            sequence.Join(overlayImage.transform.DOScale(1f, zoomDuration).SetEase(Ease.OutQuad));

            yield return sequence.WaitForCompletion();
        }

        private IEnumerator SlideOutUpCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            Vector2 originalAnchoredPosition = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(0, -Screen.height);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(1f, fadeOutDuration).SetEase(Ease.OutQuad));
            sequence.Join(rt.DOAnchorPosY(originalAnchoredPosition.y, fadeOutDuration).SetEase(Ease.OutQuad));

            yield return sequence.WaitForCompletion();
        }

        private IEnumerator SlideOutDownCoroutine()
        {
            overlayImage.color = fadeColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            Vector2 originalAnchoredPosition = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(0, Screen.height);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(1f, fadeOutDuration).SetEase(Ease.OutQuad));
            sequence.Join(rt.DOAnchorPosY(originalAnchoredPosition.y, fadeOutDuration).SetEase(Ease.OutQuad));

            yield return sequence.WaitForCompletion();
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
            yield return transitionCanvasGroup.DOFade(0f, fadeInDuration).SetEase(Ease.InQuad).WaitForCompletion();
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator ZoomInCoroutine()
        {
            overlayImage.transform.localScale = Vector3.one * 0.5f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(0f, zoomDuration).SetEase(Ease.InQuad));
            sequence.Join(overlayImage.transform.DOScale(0.5f, zoomDuration).SetEase(Ease.InQuad));

            yield return sequence.WaitForCompletion();
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator SlideInUpCoroutine()
        {
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -Screen.height);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(0f, fadeInDuration).SetEase(Ease.InQuad));
            sequence.Join(rt.DOAnchorPosY(Screen.height, fadeInDuration).SetEase(Ease.InQuad));

            yield return sequence.WaitForCompletion();
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator SlideInDownCoroutine()
        {
            RectTransform rt = overlayImage.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, Screen.height);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(0f, fadeInDuration).SetEase(Ease.InQuad));
            sequence.Join(rt.DOAnchorPosY(-Screen.height, fadeInDuration).SetEase(Ease.InQuad));

            yield return sequence.WaitForCompletion();
            overlayImage.gameObject.SetActive(false);
        }

        private IEnumerator CircleInCoroutine()
        {
            // Simplified version using zoom effect
            yield return ZoomInCoroutine();
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

            // Show victory effect
            overlayImage.color = winColor;
            overlayImage.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 0f;

            yield return transitionCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

            // Spawn confetti
            if (ParticleController.Instance != null)
            {
                Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                ParticleController.Instance.SpawnConfettiEffect(center, 100);
            }

            yield return new WaitForSeconds(1f);

            // Load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return transitionCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();
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

            yield return transitionCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();

            yield return new WaitForSeconds(0.5f);

            // Load next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return transitionCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();
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

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
            sequence.Join(overlayImage.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutQuad));

            yield return sequence.WaitForCompletion();

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

            sequence = DOTween.Sequence();
            sequence.Append(transitionCanvasGroup.DOFade(0f, 0.4f).SetEase(Ease.InQuad));
            sequence.Join(rt.DOAnchorPosY(Screen.height, 0.4f).SetEase(Ease.InQuad));

            yield return sequence.WaitForCompletion();
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
