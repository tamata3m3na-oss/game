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
    
    private NetworkManager.GameEndData gameEndData;
    
    private void Start()
    {
        if (nextMatchButton != null)
        {
            nextMatchButton.onClick.AddListener(OnNextMatchClicked);
        }
        
        // Subscribe to game end event
        GameStateManager.Instance.OnGameEnded.AddListener(ShowResultScreen);
    }
    
    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnGameEnded.RemoveListener(ShowResultScreen);
        }
    }
    
    public void ShowResultScreen(int winnerId)
    {
        gameObject.SetActive(true);
        
        // Get game end data from GameStateManager
        var gameState = GameStateManager.Instance.GetCurrentGameState();
        
        if (gameState == null) return;
        
        // Determine if local player won
        int localPlayerId = AuthManager.Instance.GetUserId();
        bool isWinner = winnerId == localPlayerId;
        
        // Set winner text
        if (winnerText != null)
        {
            winnerText.text = isWinner ? "YOU WIN!" : "YOU LOSE";
            winnerText.color = isWinner ? Color.green : Color.red;
        }
        
        // Set result title
        if (resultTitle != null)
        {
            resultTitle.text = isWinner ? "VICTORY" : "DEFEAT";
        }
        
        // Calculate stats
        int yourHealth = gameState.player1.id == localPlayerId ? gameState.player1.health : gameState.player2.health;
        int opponentHealth = gameState.player1.id == localPlayerId ? gameState.player2.health : gameState.player1.health;
        int yourDamage = gameState.player1.id == localPlayerId ? gameState.player1.damageDealt : gameState.player2.damageDealt;
        int opponentDamage = gameState.player1.id == localPlayerId ? gameState.player2.damageDealt : gameState.player1.damageDealt;
        
        // Update stats display
        if (yourHealthText != null) yourHealthText.text = yourHealth.ToString();
        if (opponentHealthText != null) opponentHealthText.text = opponentHealth.ToString();
        if (yourDamageText != null) yourDamageText.text = yourDamage.ToString();
        if (opponentDamageText != null) opponentDamageText.text = opponentDamage.ToString();
        
        // Calculate duration (assuming 20Hz tick rate, each tick is 50ms)
        float durationSeconds = gameState.tick * 0.05f;
        if (durationText != null)
        {
            TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
            durationText.text = string.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);
        }
        
        // Rating change would come from server - this is a placeholder
        if (ratingChangeText != null)
        {
            int ratingChange = isWinner ? 15 : -10; // Example values
            ratingChangeText.text = (ratingChange >= 0 ? "+" : "") + ratingChange;
            ratingChangeText.color = ratingChange >= 0 ? Color.green : Color.red;
        }
    }
    
    private void OnNextMatchClicked()
    {
        // Return to lobby
        SceneManager.LoadScene("LobbyScene");
    }
    
    public void HideResultScreen()
    {
        gameObject.SetActive(false);
    }
}