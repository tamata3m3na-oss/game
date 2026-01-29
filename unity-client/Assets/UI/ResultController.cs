using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ShipBattle.Core;
using ShipBattle.Network;

public class ResultController : MonoBehaviour
{
    [SerializeField] private Text resultText;
    [SerializeField] private Text ratingText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button lobbyButton;
    
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
        
        var result = GameManager.Instance.GetMatchResult();
        
        if (result == null)
        {
            Debug.LogWarning("[RESULT] No match result found. Testing scene directly?");
            if (resultText != null) resultText.text = "Match Result";
            if (ratingText != null) ratingText.text = "";
            return;
        }

        Debug.Log($"[RESULT] Match result - Winner: {result.winnerId}, Rating change: {result.ratingChange}");

        int myId = AuthService.Instance.GetUserId();
        bool won = result.winnerId == myId.ToString();
        
        Debug.Log($"[RESULT] My ID: {myId}, Won: {won}");
        
        if (resultText != null)
        {
            resultText.text = won ? "🎉 You Won! 🎉" : "😔 You Lost";
            resultText.color = won ? Color.green : Color.red;
            Debug.Log($"[RESULT] Result text set: {resultText.text}");
        }
        else
        {
            Debug.LogError("[RESULT] Result text is not assigned!");
        }
        
        int ratingChange = result.ratingChange;
        if (ratingText != null)
        {
            ratingText.text = ratingChange >= 0 ? 
                $"+{ratingChange} Rating" : 
                $"{ratingChange} Rating";
            ratingText.color = ratingChange >= 0 ? Color.green : Color.red;
            Debug.Log($"[RESULT] Rating text set: {ratingText.text}");
        }
        else
        {
            Debug.LogError("[RESULT] Rating text is not assigned!");
        }
        
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() => 
            {
                Debug.Log("[RESULT] Play Again button clicked - Loading Lobby");
                SceneManager.LoadScene("Lobby");
            });
            Debug.Log("[RESULT] Play Again button listener added");
        }
        else
        {
            Debug.LogError("[RESULT] Play Again button is not assigned!");
        }
        
        if (lobbyButton != null)
        {
            lobbyButton.onClick.AddListener(() => 
            {
                Debug.Log("[RESULT] Lobby button clicked - Loading Lobby");
                SceneManager.LoadScene("Lobby");
            });
            Debug.Log("[RESULT] Lobby button listener added");
        }
        else
        {
            Debug.LogError("[RESULT] Lobby button is not assigned!");
        }
    }
    
    private void OnDestroy()
    {
        Debug.Log("[RESULT] ResultController OnDestroy - Removing listeners");
        if (playAgainButton != null)
            playAgainButton.onClick.RemoveAllListeners();
        if (lobbyButton != null)
            lobbyButton.onClick.RemoveAllListeners();
    }
}
