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
    
    private void Start()
    {
        var result = GameManager.Instance.GetMatchResult();
        
        if (result == null)
        {
             // Fallback if testing scene directly
             Debug.LogWarning("No match result found.");
             return;
        }

        bool won = result.winnerId == AuthService.Instance.GetUserId().ToString();
        // Note: GetUserId returns int, winnerId is string. Ensure comparison works.
        // If IDs are consistent.
        
        if (resultText != null)
        {
            resultText.text = won ? "🎉 You Won! 🎉" : "😔 You Lost";
            resultText.color = won ? Color.green : Color.red;
        }
        
        int ratingChange = result.ratingChange;
        if (ratingText != null)
        {
            ratingText.text = ratingChange >= 0 ? 
                $"+{ratingChange} Rating" : 
                $"{ratingChange} Rating";
            ratingText.color = ratingChange >= 0 ? Color.green : Color.red;
        }
        
        playAgainButton.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("Lobby");
        });
        
        lobbyButton.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("Lobby");
        });
    }
}
