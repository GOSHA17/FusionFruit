using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game instance;

    [SerializeField] private int _levelNum;
    [SerializeField] private Level[] _levels;
    [SerializeField] private Match3 _match3;
    private int _countCompletedLevels = 0;

    [Header("Time")]
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _winPanelText;
    [SerializeField] private int _timer;
    [SerializeField] private int timeFor3Stars;
    [SerializeField] private int timeFor2Stars;
    [SerializeField] private int timeFor1Stars;

    [Header("UI Elements")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private Text _textFor3Stars;
    [SerializeField] private Text _textFor2Stars;
    [SerializeField] private Text _textFor1Stars;

    [Header("Stars")]
    [SerializeField] private Image[] starsImage;
    [SerializeField] private Sprite starCollected;
    [SerializeField] private Sprite starEmpty;
    [SerializeField] private AudioSource _audioStars;


    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        System.Random random = new System.Random();
        for (int i = 0; i < _levels.Length; i++)
        {
            _levels[i].completed += GameOver;
            if (i == 0)
            {
                _levels[i].valuePiece = random.Next(1, _match3.GetPieces().Length);
            }
            else
            {
                do
                {
                    _levels[i].valuePiece = random.Next(1, _match3.GetPieces().Length);
                }
                while (_levels[i - 1].valuePiece == _levels[i].valuePiece);
            }
            UpdateUI(i);
        }
        _timerText.text = string.Format("{0:00}:{1:00}", _timer / 60, _timer % 60);
        _textFor3Stars.text = string.Format("{0:00}:{1:00}", timeFor3Stars / 60, timeFor3Stars % 60);
        _textFor2Stars.text = string.Format("{0:00}:{1:00}", timeFor2Stars / 60, timeFor2Stars % 60);
        _textFor1Stars.text = string.Format("{0:00}:{1:00}", timeFor1Stars / 60, timeFor1Stars % 60);
        StartCoroutine(TimerTick());
    }

    private void UpdateUI(int index)
    {
        _levels[index].countText.text = $"{_levels[index].GetCurrentCount()}/{ _levels[index].needCount}";
        _levels[index].pieceImage.sprite = _match3.GetPieces()[_levels[index].valuePiece - 1];
    }

    public void AddPieceToCount(int value)
    {
        for (int i = 0; i < _levels.Length; i++)
        {
            if (_levels[i].valuePiece == value && !_levels[i].IsCompleted())
            {
                _levels[i].AddCount();
                UpdateUI(i);
            }
        }
    }

    private void GameOver()
    {
        _countCompletedLevels++;
        if (_countCompletedLevels == _levels.Length)
        {
            Win();
        }
    }

    private void Lose()
    {
        _gameOverPanel.SetActive(true);
        StopAllCoroutines();
    }

    private void Win()
    {
        _winPanel.SetActive(true);
        _winPanelText.text = string.Format("{0:00}:{1:00}", _timer / 60, _timer % 60);
        StopAllCoroutines();
        StartCoroutine(SetCollectedStars());
    }

    IEnumerator TimerTick()
    {
        yield return new WaitForSeconds(1f);
        _timer -= 1;
        _timerText.text = string.Format("{0:00}:{1:00}", _timer / 60, _timer % 60);
        if (_timer <= 0)
        {
            Lose();
            StopAllCoroutines();
        }
        else
        {
            StartCoroutine(TimerTick());
        }
    }

    private int CheckStarsCompleted()
    {
        if (_timer >= timeFor3Stars)
        {
            return 3;
        }
        else if (_timer >= timeFor2Stars)
        {
            return 2;
        }
        else if (_timer >= timeFor1Stars)
        {
            return 1;
        }   
        else
        {
            return 0;
        }
    }

    IEnumerator SetCollectedStars()
    {
        int collectedStars = CheckStarsCompleted();

        for (int i = 0; i < starsImage.Length; i++)
        {
            yield return new WaitForSeconds(0.5f);
            if (i < collectedStars)
            {
                starsImage[i].sprite = starCollected;
                _audioStars.Play();
            }
            else
            {
                starsImage[i].sprite = starEmpty;
            }
        }

        if (PlayerPrefs.HasKey($"Level{_levelNum}"))
        {
            int previousStars = PlayerPrefs.GetInt($"Level{_levelNum}");
            if (previousStars < collectedStars)
            {
                PlayerPrefs.SetInt($"Level{_levelNum}", collectedStars);
            }
        }
        else
        {
            PlayerPrefs.SetInt($"Level{_levelNum}", collectedStars);
        }
    }
}

[System.Serializable]
public class Level
{
    public Text countText;
    public Image pieceImage;
    [HideInInspector] public int valuePiece;
    public int needCount = 30;
    public event UnityAction completed;

    private bool _isCompleted = false;
    private int _currentCount = 0;

    public void AddCount()
    {
        _currentCount = Mathf.Clamp(_currentCount + 1, 0, needCount);
        if (_currentCount == needCount)
        {
            completed?.Invoke();
            _isCompleted = true;
        }
    }

    public bool IsCompleted()
    {
        return _isCompleted;
    }

    public int GetCurrentCount()
    {
        return _currentCount;
    }
}
