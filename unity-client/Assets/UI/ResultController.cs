using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ShipBattle.Core;
using ShipBattle.Network;
using TMPro;

public class ResultController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text ratingChangeText;
    [SerializeField] private Button lobbyButton;

    [Header("Statistics")]
    [SerializeField] private TMP_Text playerStatsText;
    [SerializeField] private TMP_Text enemyStatsText;
    
    private void Awake()
    {
        Debug.Log("[RESULT] ResultController Awake");
    }
    
    private void Start()
    {
        Debug.Log("[RESULT] ResultController Start");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("[RESULT] GameManager instance is null!");
            return;
        }
        
        if (lobbyButton != null)
        {
            lobbyButton.onClick.AddListener(ReturnToLobby);
        }
        
        DisplayResults();
    }
    
    private void DisplayResults()
    {
        var result = GameManager.Instance.GetMatchResult();
        if (result == null)
        {
            if (resultText != null) resultText.text = "No Match Data";
            return;
        }

        int myId = AuthService.Instance.GetUserId();
        bool won = result.winnerId == myId.ToString();
        
        if (resultText != null)
        {
            resultText.text = won ? "VICTORY" : "DEFEAT";
            resultText.color = won ? Color.green : Color.red;
        }
        
        if (ratingChangeText != null)
        {
            ratingChangeText.text = result.ratingChange >= 0 ? 
                $"+{result.ratingChange} Rank" : 
                $"{result.ratingChange} Rank";
        }

        // Mock stats for now as they are not in MatchEndEvent yet
        if (playerStatsText != null) playerStatsText.text = "Kills: 0\nDamage: 0";
        if (enemyStatsText != null) enemyStatsText.text = "Kills: 0\nDamage: 0";
        
        Debug.Log($"[RESULT] Result displayed: {resultText?.text}");
    }

    public void ReturnToLobby()
    {
        Debug.Log("[RESULT] Returning to lobby...");
        SceneManager.LoadScene("Lobby");
    }
    
    private void OnDestroy()
    {
        if (lobbyButton != null)
            lobbyButton.onClick.RemoveListener(ReturnToLobby);
    }
}
