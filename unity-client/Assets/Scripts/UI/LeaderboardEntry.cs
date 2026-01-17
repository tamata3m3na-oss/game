using UnityEngine;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI winsText;
    public TextMeshProUGUI lossesText;
    
    public void SetData(int rank, string username, int rating, int wins, int losses)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (usernameText != null) usernameText.text = username;
        if (ratingText != null) ratingText.text = rating.ToString();
        if (winsText != null) winsText.text = wins.ToString();
        if (lossesText != null) lossesText.text = losses.ToString();
    }
}