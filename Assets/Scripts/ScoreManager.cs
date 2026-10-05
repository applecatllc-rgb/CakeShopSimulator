using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    private int score;

    private void Start()
    {
        RefreshDisplay();
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}
