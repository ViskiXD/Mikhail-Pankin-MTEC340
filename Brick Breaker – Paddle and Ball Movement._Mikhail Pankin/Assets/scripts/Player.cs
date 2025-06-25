using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    private int _score;

    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            _scoreGUI.text = Score.ToString();
        }
    }

    [SerializeField] private TextMeshProUGUI _scoreGUI;


    private void Start()
    {
        Score = 0;
    }

}
