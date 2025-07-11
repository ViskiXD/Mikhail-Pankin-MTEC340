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
    
    [Header("Red Egg Settings")]
    [SerializeField] private float _redEggDelay = 10.0f; // Time before red eggs can spawn
    [SerializeField] private float _redEggChance = 0.1f; // 10% chance for red egg
    
    [Header("UI")]
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _instructionsText;
    [SerializeField] private Text _pauseText;
    
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _gameStartClip;
    [SerializeField] private AudioClip _eggSpawnClip;
    [SerializeField] private AudioClip _explosionClip; // Optional explosion sound
    
    private int _score = 0;
    private float _currentSpawnInterval;
    private float _spawnTimer;
    private bool _gameActive = true;
    private bool _isPaused = false;
    private float _gameTime = 0.0f;
    private bool _exploded = false;
    
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
            _instructionsText.text = "Use Q, R, A, D (or 1, 2, 3, 4) to move the wolf and catch eggs! Press SPACE to pause.";
        }
    }
    
    void Update()
    {
        if (!_gameActive) return;
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        
        // Only spawn eggs if not paused and not exploded
        if (!_isPaused && !_exploded)
        {
            // Track game time
            _gameTime += Time.deltaTime;
            
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
    }
    
    private void SpawnEgg()
    {
        if (_eggPrefab == null || _spawnPoints.Length == 0) return;
        
        // Choose random spawn point (1 of 4)
        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[randomIndex];
        
        if (spawnPoint != null)
        {
            GameObject newEgg = Instantiate(_eggPrefab, spawnPoint.position, Quaternion.identity);
            
            // Check if we should make this a red egg (after delay and with random chance)
            if (_gameTime >= _redEggDelay && Random.Range(0f, 1f) < _redEggChance)
            {
                ConvertToRedEgg(newEgg);
            }
            
            // Play spawn sound
            if (_audioSource && _eggSpawnClip)
            {
                _audioSource.PlayOneShot(_eggSpawnClip);
            }
        }
    }
    
    private void ConvertToRedEgg(GameObject eggObject)
    {
        // Remove the regular Egg script
        Egg originalEgg = eggObject.GetComponent<Egg>();
        if (originalEgg != null)
        {
            DestroyImmediate(originalEgg);
        }
        
        // Add RedEgg script
        eggObject.AddComponent<RedEgg>();
        
        // Change color to red
        SpriteRenderer spriteRenderer = eggObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        // Try to change material color as backup
        Renderer renderer = eggObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }
    
    public void ScorePoint()
    {
        _score++;
        UpdateUI();
    }
    
    public void LosePoint()
    {
        _score--;
        // Don't let score go below 0
        if (_score < 0) _score = 0;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = "Score: " + _score;
        }
        
        if (_pauseText != null)
        {
            _pauseText.text = _isPaused ? "PAUSED - Press SPACE to Resume" : "";
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
        _isPaused = false;
        _gameTime = 0.0f;
        _exploded = false;
        UpdateUI();
    }
    
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        UpdateUI();
    }
    
    public bool IsPaused()
    {
        return _isPaused;
    }
    
    public bool IsExploded()
    {
        return _exploded;
    }
    
    public void TriggerExplosion()
    {
        if (_exploded) return; // Prevent multiple explosions
        
        _exploded = true;
        
        // Play explosion sound if available
        if (_audioSource && _explosionClip)
        {
            _audioSource.PlayOneShot(_explosionClip);
        }
        
        // Make everything fall down with explosion forces
        StartCoroutine(ExplosionEffect());
    }
    
    private System.Collections.IEnumerator ExplosionEffect()
    {
        // Find ALL game objects and make them explode!
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                // Skip UI elements and cameras
                if (obj.GetComponent<Canvas>() != null || 
                    obj.GetComponent<Camera>() != null ||
                    obj.name.Contains("UI") || 
                    obj.name.Contains("Canvas") ||
                    obj.name.Contains("Text") ||
                    obj.name.Contains("Button"))
                {
                    continue;
                }
                
                // Get or add Rigidbody2D to make everything explodable
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = obj.AddComponent<Rigidbody2D>();
                }
                
                // Make sure it's not kinematic so it can move
                rb.bodyType = RigidbodyType2D.Dynamic;
                
                // Apply massive random explosion forces
                Vector2 explosionForce = new Vector2(
                    Random.Range(-25f, 25f), // Even stronger horizontal force
                    Random.Range(10f, 30f)   // Stronger upward force
                );
                
                rb.AddForce(explosionForce, ForceMode2D.Impulse);
                
                // Add crazy random spin
                rb.angularVelocity = Random.Range(-30f, 30f);
                
                // Increase gravity to make everything fall faster
                rb.gravityScale = 4.0f;
                
                // Make colliders into triggers temporarily so things don't get stuck
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null && !collider.isTrigger)
                {
                    collider.isTrigger = true;
                }
            }
        }
        
        // Also specifically target known game objects
        ExplodeSpecificObjects();
        
        // Wait a few seconds, then restart the game
        yield return new WaitForSeconds(3.0f);
        
        // Restart the game
        RestartGame();
    }
    
    private void ExplodeSpecificObjects()
    {
        // Find and explode pallet specifically
        Pallet pallet = FindObjectOfType<Pallet>();
        if (pallet != null)
        {
            ExplodeObject(pallet.gameObject, new Vector2(Random.Range(-20f, 20f), Random.Range(15f, 25f)));
        }
        
        // Find and explode all slopes
        Slopes[] slopes = FindObjectsOfType<Slopes>();
        foreach (Slopes slope in slopes)
        {
            if (slope != null)
            {
                ExplodeObject(slope.gameObject, new Vector2(Random.Range(-15f, 15f), Random.Range(10f, 20f)));
            }
        }
        
        // Find and explode score zones
        ScoreZone[] scoreZones = FindObjectsOfType<ScoreZone>();
        foreach (ScoreZone zone in scoreZones)
        {
            if (zone != null)
            {
                ExplodeObject(zone.gameObject, new Vector2(Random.Range(-10f, 10f), Random.Range(5f, 15f)));
            }
        }
        
        // Find any objects with "Ground", "Wall", "Platform" etc in the name
        GameObject[] namedObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in namedObjects)
        {
            if (obj.name.ToLower().Contains("ground") || 
                obj.name.ToLower().Contains("wall") || 
                obj.name.ToLower().Contains("platform") ||
                obj.name.ToLower().Contains("slope"))
            {
                ExplodeObject(obj, new Vector2(Random.Range(-12f, 12f), Random.Range(8f, 18f)));
            }
        }
    }
    
    private void ExplodeObject(GameObject obj, Vector2 force)
    {
        if (obj == null) return;
        
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.angularVelocity = Random.Range(-25f, 25f);
        rb.gravityScale = 3.5f;
    }
    
    private void RestartGame()
    {
        // Destroy all eggs by finding them with components (more reliable)
        Egg[] allEggs = FindObjectsOfType<Egg>();
        foreach (Egg egg in allEggs)
        {
            if (egg != null && egg.gameObject != null)
            {
                Destroy(egg.gameObject);
            }
        }
        
        RedEgg[] allRedEggs = FindObjectsOfType<RedEgg>();
        foreach (RedEgg redEgg in allRedEggs)
        {
            if (redEgg != null && redEgg.gameObject != null)
            {
                Destroy(redEgg.gameObject);
            }
        }
        
        // Reset ALL objects back to their original state
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                // Skip UI elements and cameras
                if (obj.GetComponent<Canvas>() != null || 
                    obj.GetComponent<Camera>() != null ||
                    obj.name.Contains("UI") || 
                    obj.name.Contains("Canvas") ||
                    obj.name.Contains("Text") ||
                    obj.name.Contains("Button"))
                {
                    continue;
                }
                
                // Reset rigidbodies
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Don't reset eggs since they're being destroyed
                    if (rb.GetComponent<Egg>() == null && rb.GetComponent<RedEgg>() == null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                        rb.gravityScale = 1.0f;
                        
                        // Remove rigidbodies that were added during explosion (except for eggs and pallet)
                        if (rb.GetComponent<Pallet>() == null && 
                            rb.GetComponent<Egg>() == null && 
                            rb.GetComponent<RedEgg>() == null)
                        {
                            // Check if this object originally had physics
                            if (!ShouldHaveRigidbody(obj))
                            {
                                DestroyImmediate(rb);
                            }
                            else
                            {
                                // Reset to kinematic if it should be static
                                if (obj.GetComponent<Slopes>() != null || 
                                    obj.name.ToLower().Contains("ground") ||
                                    obj.name.ToLower().Contains("wall") ||
                                    obj.name.ToLower().Contains("platform"))
                                {
                                    rb.bodyType = RigidbodyType2D.Kinematic;
                                }
                            }
                        }
                    }
                }
                
                // Reset colliders
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    // Reset trigger state (most things shouldn't be triggers)
                    if (obj.GetComponent<Egg>() == null && 
                        obj.GetComponent<RedEgg>() == null && 
                        obj.GetComponent<ScoreZone>() == null &&
                        !obj.name.ToLower().Contains("trigger"))
                    {
                        collider.isTrigger = false;
                    }
                }
                
                // Reset positions for specific objects
                ResetObjectPosition(obj);
            }
        }
        
        // Restart game
        StartGame();
    }
    
    private bool ShouldHaveRigidbody(GameObject obj)
    {
        // Objects that should naturally have rigidbodies
        return obj.GetComponent<Egg>() != null || 
               obj.GetComponent<RedEgg>() != null || 
               obj.GetComponent<Pallet>() != null;
    }
    
    private void ResetObjectPosition(GameObject obj)
    {
        // Reset pallet to its starting position if it has position references
        Pallet pallet = obj.GetComponent<Pallet>();
        if (pallet != null)
        {
            // The pallet script will handle its own positioning
            return;
        }
        
        // For other objects, we don't move them since we don't know their original positions
        // They will settle naturally with physics
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