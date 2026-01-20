using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultScreenController : MonoBehaviour
{
    [Header("Result UI")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI resultTitle;

    [Header("Stats Display")]
    public TextMeshProUGUI yourHealthText;
    public TextMeshProUGUI opponentHealthText;
    public TextMeshProUGUI yourDamageText;
    public TextMeshProUGUI opponentDamageText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI ratingChangeText;

    [Header("Buttons")]
    public Button nextMatchButton;

    private void Start()
    {
        if (nextMatchButton != null)
        {
            nextMatchButton.onClick.AddListener(OnNextMatchClicked);
        }

        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnGameEndReceived += OnGameEnd;
        }
        else
        {
            Debug.LogWarning("[ResultScreenController] NetworkEventManager is missing.");
        }

        // If we entered the scene after the event fired, render from cached state.
        if (NetworkEventManager.Instance?.LastGameEnd != null)
        {
            OnGameEnd(NetworkEventManager.Instance.LastGameEnd);
        }
    }

    private void OnDestroy()
    {
        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnGameEndReceived -= OnGameEnd;
        }
    }

    private void OnGameEnd(GameEndData endData)
    {
        if (endData == null) return;

        gameObject.SetActive(true);

        NetworkGameState gameState = endData.finalState ?? GameStateRepository.Instance?.GetCurrentState();
        if (gameState == null) return;

        int localPlayerId = AuthManager.Instance != null ? AuthManager.Instance.GetUserId() : -1;
        bool isWinner = endData.winner == localPlayerId;

        if (winnerText != null)
        {
            winnerText.text = isWinner ? "YOU WIN!" : "YOU LOSE";
            winnerText.color = isWinner ? Color.green : Color.red;
        }

        if (resultTitle != null)
        {
            resultTitle.text = isWinner ? "VICTORY" : "DEFEAT";
        }

        int yourHealth = gameState.player1.id == localPlayerId ? gameState.player1.health : gameState.player2.health;
        int opponentHealth = gameState.player1.id == localPlayerId ? gameState.player2.health : gameState.player1.health;
        int yourDamage = gameState.player1.id == localPlayerId ? gameState.player1.damageDealt : gameState.player2.damageDealt;
        int opponentDamage = gameState.player1.id == localPlayerId ? gameState.player2.damageDealt : gameState.player1.damageDealt;

        if (yourHealthText != null) yourHealthText.text = yourHealth.ToString();
        if (opponentHealthText != null) opponentHealthText.text = opponentHealth.ToString();
        if (yourDamageText != null) yourDamageText.text = yourDamage.ToString();
        if (opponentDamageText != null) opponentDamageText.text = opponentDamage.ToString();

        float durationSeconds = gameState.tick * 0.05f;
        if (durationText != null)
        {
            TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
            durationText.text = string.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);
        }

        if (ratingChangeText != null)
        {
            int ratingChange = isWinner ? 15 : -10;
            ratingChangeText.text = (ratingChange >= 0 ? "+" : "") + ratingChange;
            ratingChangeText.color = ratingChange >= 0 ? Color.green : Color.red;
        }
    }

    private void OnNextMatchClicked()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void HideResultScreen()
    {
        gameObject.SetActive(false);
    }
}
