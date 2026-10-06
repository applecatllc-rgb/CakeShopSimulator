using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        RefreshDisplay();
    }

    public void AddScore(int amount)
    {
        GameSession.AddScore(amount);
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {GameSession.Score}";
    }
}