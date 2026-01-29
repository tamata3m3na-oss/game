using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleStar.UI
{
    public class SplashController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float splashDuration = 2f;
        [SerializeField] private string nextScene = "Login";

        [Header("UI References")]
        [SerializeField] private Image logoImage;
        [SerializeField] private Text loadingText;

        private void Start()
        {
            StartCoroutine(ShowSplashThenLoadLogin());
        }

        private IEnumerator ShowSplashThenLoadLogin()
        {
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }

            yield return new WaitForSeconds(splashDuration);

            if (loadingText != null)
            {
                loadingText.text = "Connecting...";
            }

            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene(nextScene);
        }
    }
}