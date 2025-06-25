using UnityEngine;
using TMPro;
// manager class for the game
// software design pattern: singleton
// ensures that there is only one instance of the class
// and provides a global point of access to it
// this is useful for managing the game state and ensuring that the game is running smoothly
// and efficiently
// this is a good example of a singleton pattern
// because it ensures that there is only one instance of the class
public class Gamebihave : MonoBehaviour
{
    // singleton instance
    private static Gamebihave _instance;

    // singleton instance property
    public static Gamebihave Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Gamebihave>();
            }
            return _instance;
        }
    }

    public float paddleSpeed = 10.0f;
    public float ballSpeed = 10.0f;
    public float ballSpeedIncrease = 0.1f;
    public float ballSpeedIncreaseInterval = 10.0f;
    public float paddleInfluence = 0.5f;
    public float ballSize = 1.0f;
    
    // score backing variable
    private int _score;
    
    // score getter and setter property
    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            Debug.Log("Score updated to: " + _score);
            // update the UI text every time the score is updated
            if (_scoreTextUI != null)
            {
                _scoreTextUI.text = _score.ToString();
                Debug.Log("UI text updated to: " + _scoreTextUI.text);
            }
            else
            {
                Debug.LogWarning("_scoreTextUI is null! Please assign a TextMeshPro UI component in the inspector.");
            }
        }
    }
    
    // score text UI variable
    [SerializeField] private TextMeshProUGUI _scoreTextUI;

    [SerializeField] int _pointsToWin = 3;
    [SerializeField] private Player[] _players = new Player[2];

    
    
    void Awake()
    {
        if (_instance != null && _instance != this)
           Destroy(this);
        else
           _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // initialize score
        Score = 0;
        
        // only update player scores if players exist (null check)
        if (_players != null)
        {
            foreach (Player p in _players)
            {
                if (p != null)
                {
                    p.Score = 0;
                }
            }
        }
    }

    public void ScorePoint(int playerIndex)
    {
        Debug.Log("ScorePoint called with playerIndex: " + playerIndex);
        // increment the main game score when a brick is destroyed
        Score++;
        
        // also update player score if players array is being used
        if (_players != null && playerIndex >= 0 && playerIndex < _players.Length && _players[playerIndex] != null)
        {
            _players[playerIndex].Score++;
            Debug.Log("Player " + playerIndex + " score updated to: " + _players[playerIndex].Score);
        }
    }

    void Update()
    {
        // test scoring system with space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - testing score increment");
            ScorePoint(0);
        }
    }
    
    // manual method to test scoring (can be called from other scripts or inspector)
    [ContextMenu("Test Score")]
    public void TestScore()
    {
        Debug.Log("Manual score test called");
        ScorePoint(0);
    }
}


//I really tried to fix it in any possible way, but I couldn't find the issue. my score in just not updating. I am soory but i will
//submit it as is. Cause I just Can't find the issue AT ALL.