using UnityEngine;
using UnityEngine.UI;

public class EggGameManager : MonoBehaviour
{
    public static EggGameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private GameObject _eggPrefab;
    [SerializeField] private Transform[] _spawnPoints = new Transform[4]; // 4 spawn points for eggs
    [SerializeField] private float _spawnInterval = 2.0f;
    [SerializeField] private float _spawnIntervalDecrease = 0.05f; // Speed up over time
    [SerializeField] private float _minSpawnInterval = 0.5f;
    
    [Header("UI")]
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _instructionsText;
    
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _gameStartClip;
    [SerializeField] private AudioClip _eggSpawnClip;
    
    private int _score = 0;
    private float _currentSpawnInterval;
    private float _spawnTimer;
    private bool _gameActive = true;
    
    void Awake()
    {
        // Singleton pattern like in brick breaker
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        _currentSpawnInterval = _spawnInterval;
        _spawnTimer = _currentSpawnInterval;
        
        UpdateUI();
        
        if (_audioSource && _gameStartClip)
        {
            _audioSource.PlayOneShot(_gameStartClip);
        }
        
        // Show instructions
        if (_instructionsText != null)
        {
            _instructionsText.text = "Use Q, W, A, D (or 1, 2, 3, 4) to move the wolf and catch eggs!";
        }
    }
    
    void Update()
    {
        if (!_gameActive) return;
        
        // Handle egg spawning
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0)
        {
            SpawnEgg();
            _spawnTimer = _currentSpawnInterval;
            
            // Gradually increase difficulty
            if (_currentSpawnInterval > _minSpawnInterval)
            {
                _currentSpawnInterval -= _spawnIntervalDecrease;
                _currentSpawnInterval = Mathf.Max(_currentSpawnInterval, _minSpawnInterval);
            }
        }
    }
    
    private void SpawnEgg()
    {
        if (_eggPrefab == null || _spawnPoints.Length == 0) return;
        
        // Choose random spawn point (1 of 4)
        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[randomIndex];
        
        if (spawnPoint != null)
        {
            Instantiate(_eggPrefab, spawnPoint.position, Quaternion.identity);
            
            // Play spawn sound
            if (_audioSource && _eggSpawnClip)
            {
                _audioSource.PlayOneShot(_eggSpawnClip);
            }
        }
    }
    
    public void ScorePoint()
    {
        _score++;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = "Score: " + _score;
        }
    }
    
    public void StopGame()
    {
        _gameActive = false;
    }
    
    public void StartGame()
    {
        _gameActive = true;
        _score = 0;
        _currentSpawnInterval = _spawnInterval;
        UpdateUI();
    }
    
    // Optional: visualize spawn points in scene view
    void OnDrawGizmos()
    {
        if (_spawnPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] != null)
                {
                    Gizmos.DrawWireCube(_spawnPoints[i].position, Vector3.one * 0.5f);
                }
            }
        }
    }
} 