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
public class GameBehavior : MonoBehaviour
{
    // singleton instance
    private static GameBehavior _instance;

    public Utilities.GameState CurrentState;

    [SerializeField] private TMP_Text _messagesGUI;

    // singleton instance property
    public static GameBehavior Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameBehavior>();
            }
            return _instance;
        }
    }

    public float paddleSpeed = 10.0f;
    public float ballSpeed = 10.0f;
    public float ballSpeedIncrease = 0.1f;
    public float ballSpeedIncreaseInterval = 10.0f;
    public float paddleInfluence = 0.5f;
    public float ballSize = 0.25f;
    
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
                // Try to find a TextMeshPro component as fallback
                if (_scoreTextUI == null)
                {
                    _scoreTextUI = FindObjectOfType<TextMeshProUGUI>();
                    if (_scoreTextUI != null)
                    {
                        Debug.Log("Found TextMeshPro UI component automatically!");
                        _scoreTextUI.text = _score.ToString();
                    }
                }
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
        CurrentState = Utilities.GameState.Play;
        
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

    private void Update()
    {
        // toggle pause/play with space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (CurrentState)
            {
                case Utilities.GameState.Play:
                    CurrentState = Utilities.GameState.Pause;
                    if (_messagesGUI != null) _messagesGUI.enabled = true;
                    break;
                case Utilities.GameState.Pause:
                    CurrentState = Utilities.GameState.Play;
                    if (_messagesGUI != null) _messagesGUI.enabled = false;
                    break;
                case Utilities.GameState.GameOver:
                    CurrentState = Utilities.GameState.Play;
                    if (_messagesGUI != null) _messagesGUI.enabled = false;
                    break;
            }
        }
        // test scoring system with B key (changed from space to avoid conflict)
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B pressed - testing score increment");
            ScorePoint(0);
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

    // manual method to test scoring (can be called from other scripts or inspector)
    [ContextMenu("Test Score")]
    public void TestScore()
    {
        Debug.Log("Manual score test called");
        ScorePoint(0);
    }
}