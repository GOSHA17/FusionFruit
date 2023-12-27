using UnityEngine;

public class ChooseLevel : MonoBehaviour
{
    [SerializeField] private LevelInfo[] _levelsInfo;
    [SerializeField] private Sprite _collectedStarSprite;
    [SerializeField] private Sprite _emptyStarSprite;

    private void Start()
    {
        for (int i = 0; i < _levelsInfo.Length; i++)
        {
            if (_levelsInfo[i].CheckLevelCompleted() || i == 0 || (i != 0 && _levelsInfo[i - 1].CheckLevelCompleted()))
                _levelsInfo[i].SetInteractionButton();
            else
                break;

            if (_levelsInfo[i].CheckLevelCompleted())
            {
                _levelsInfo[i].SetStarsInfo(_collectedStarSprite, _emptyStarSprite);
            }
        }
    }
}
