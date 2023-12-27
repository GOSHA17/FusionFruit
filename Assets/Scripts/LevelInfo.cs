using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private int _levelNum;
    [SerializeField] private Image[] _stars;

    private int _collectedStars;
    private Text _levelText;
    private Button _levelButton;

    private void Awake()
    {
        _levelText = GetComponentInChildren<Text>();
        _levelButton = GetComponent<Button>();
    }

    private void Start()
    {
        _levelButton.onClick.AddListener(LoadLevel);
        _levelText.text = _levelNum.ToString();
    }

    public void SetInteractionButton()
    {
        _levelButton.interactable = true;
    }

    public bool CheckLevelCompleted()
    {
        return PlayerPrefs.HasKey($"Level{_levelNum}");
    }

    public void SetStarsInfo(Sprite collectedStar, Sprite starEmpty)
    {
        _collectedStars = PlayerPrefs.GetInt($"Level{_levelNum}");
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].gameObject.SetActive(true);
            if (i < _collectedStars)
            {
                _stars[i].sprite = collectedStar;
            }
            else
            {
                _stars[i].sprite = starEmpty;
            }
        }
    }

    public int GetLevelNum()
    {
        return _levelNum;
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene($"Level{_levelNum}");
    }
}
