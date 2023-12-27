using UnityEngine;
using UnityEngine.UI;

public class PointsSystem : MonoBehaviour
{
    [SerializeField] private Text _currentScoreText;
    [SerializeField] private Text _bestScoreText;

    private int _currentScore = 0;
    private int _bestScore = 0;

    private void Start()
    {
        if (PlayerPrefs.HasKey("BestScore"))
            _bestScore = PlayerPrefs.GetInt("BestScore");

        _currentScoreText.text = _currentScore.ToString();
        _bestScoreText.text = _bestScore.ToString();
    }

    private void UpdateUI()
    {
        UpdateBestScore();

        _currentScoreText.text = _currentScore.ToString();
        _bestScoreText.text = _bestScore.ToString();
    }

    private void UpdateBestScore()
    {
        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetInt("BestScore", _bestScore);
        }
    }
    
    public void AddPoints(int count)
    {
        _currentScore += count;
        UpdateUI();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("BestScore", _bestScore);
    }
}
