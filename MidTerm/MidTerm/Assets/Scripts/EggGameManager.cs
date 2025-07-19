using UnityEngine;
using TMPro;
using System.Collections.Generic; // Added for List

public class EggGameManager : MonoBehaviour
{
    public static EggGameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private GameObject _eggPrefab;
    [SerializeField] private Transform[] _spawnPoints = new Transform[4]; // 4 spawn points for eggs
    [SerializeField] private float _spawnInterval = 2.0f;
    [SerializeField] private float _spawnIntervalDecrease = 0.05f; // Speed up over time
    [SerializeField] private float _minSpawnInterval = 0.5f;
    
    [Header("Special Egg Settings")]
    [SerializeField] private float _specialEggDelay = 5.0f; // Time before special eggs can spawn
    [SerializeField] private float _redEggChance = 0.08f; // 8% chance for red egg
    [SerializeField] private float _blueEggChance = 0.06f; // 6% chance for blue egg (slow motion)
    [SerializeField] private float _orangeEggChance = 0.06f; // 6% chance for orange egg (freeze pallet)
    [SerializeField] private float _greenEggChance = 0.05f; // 5% chance for green egg (heal)
    [SerializeField] private float _purpleEggChance = 0.05f; // 5% chance for purple egg (speed boost)
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _instructionsText;
    [SerializeField] private TextMeshProUGUI _pauseText;
    [SerializeField] private TextMeshProUGUI _effectText; // For showing active effects
    [SerializeField] private TextMeshProUGUI _gameOverText; // For showing final score when game over
    
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _gameStartClip;
    [SerializeField] private AudioClip _eggSpawnClip;
    [SerializeField] private AudioClip _explosionClip; // Optional explosion sound
    
    private long _score = 0;
    private float _currentSpawnInterval;
    private float _spawnTimer;
    private bool _gameActive = true;
    private bool _isPaused = false;
    private float _gameTime = 0.0f;
    private bool _exploded = false;
    private bool _gameOver = false; // Track game over state
    
    // Effect tracking
    private bool _slowMotionActive = false;
    private float _slowMotionTimer = 0.0f;
    private float _slowMotionDuration = 3.0f; // Blue egg slow motion for 3 seconds
    
    private bool _speedBoostActive = false;
    private float _speedBoostTimer = 0.0f;
    private float _speedBoostDuration = 5.0f; // Purple egg speed boost for 5 seconds
    private bool _purpleEggSurvivalBonus = false; // Track if player should get survival bonus
    
    private bool _palletFrozen = false;
    private float _palletFreezeTimer = 0.0f;
    private float _palletFreezeDuration = 0.5f; // Orange egg freezes pallet for 0.5 seconds
    
    // Original state storage for complete scene reset
    [System.Serializable]
    public class ObjectState
    {
        public GameObject gameObject;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;
        public bool hadRigidbody;
        public RigidbodyType2D originalBodyType;
        public bool hadCollider;
        public bool originalIsTrigger;
        public float originalGravityScale;
    }
    
    private List<ObjectState> _originalObjectStates = new List<ObjectState>();
    
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
        // Store original state of all scene objects
        StoreOriginalSceneState();
        
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
    
    private void StoreOriginalSceneState()
    {
        _originalObjectStates.Clear();
        
        // Find all GameObjects in the scene (excluding UI and prefabs)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Skip UI elements, cameras, GameManager, and inactive objects
            if (obj.GetComponent<Canvas>() != null || 
                obj.GetComponent<Camera>() != null ||
                obj.GetComponent<EggGameManager>() != null || // Skip GameManager
                obj.name.Contains("UI") || 
                obj.name.Contains("Canvas") ||
                obj.name.Contains("Text") ||
                obj.name.Contains("Button") ||
                obj.name.Contains("EventSystem") ||
                obj.name.Contains("GameManager") || // Skip by name too
                !obj.activeInHierarchy)
            {
                continue;
            }
            
            // Create state record
            ObjectState state = new ObjectState();
            state.gameObject = obj;
            state.originalPosition = obj.transform.position;
            state.originalRotation = obj.transform.rotation;
            state.originalScale = obj.transform.localScale;
            
            // Store physics state
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                state.hadRigidbody = true;
                state.originalBodyType = rb.bodyType;
                state.originalGravityScale = rb.gravityScale;
            }
            else
            {
                state.hadRigidbody = false;
            }
            
            // Store collider state
            Collider2D collider = obj.GetComponent<Collider2D>();
            if (collider != null)
            {
                state.hadCollider = true;
                state.originalIsTrigger = collider.isTrigger;
            }
            else
            {
                state.hadCollider = false;
            }
            
            _originalObjectStates.Add(state);
        }
        
        Debug.Log($"Stored original state for {_originalObjectStates.Count} objects");
    }
    
    void Update()
    {
        if (!_gameActive) 
        {
            Debug.Log("Game not active - not processing update");
            return;
        }
        
        // Handle restart input when game over
        if (_gameOver && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Restart key pressed - restarting game");
            RestartGame();
            return;
        }
        
        // Handle pause input (only if not game over)
        if (!_gameOver && Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        
        // Handle time-based effects (only if not game over)
        if (!_isPaused && !_exploded && !_gameOver)
        {
            UpdateEffects();
        }
        
        // Only spawn eggs if not paused, not exploded, and not game over
        if (!_isPaused && !_exploded && !_gameOver)
        {
            // Track game time
            _gameTime += Time.deltaTime;
            
            // Handle egg spawning
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0)
            {
                Debug.Log($"Spawning egg - gameTime: {_gameTime}, spawnTimer: {_spawnTimer}");
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
        else
        {
            // Debug why eggs aren't spawning
            if (_isPaused) Debug.Log("Not spawning eggs - game is paused");
            if (_exploded) Debug.Log("Not spawning eggs - game exploded");
            if (_gameOver) Debug.Log("Not spawning eggs - game over");
        }
    }
    
    private void UpdateEffects()
    {
        // Update slow motion effect
        if (_slowMotionActive)
        {
            _slowMotionTimer -= Time.unscaledDeltaTime; // Use unscaled time since we're modifying Time.timeScale
            if (_slowMotionTimer <= 0)
            {
                EndSlowMotion();
            }
        }
        
        // Update speed boost effect
        if (_speedBoostActive)
        {
            _speedBoostTimer -= Time.unscaledDeltaTime; // Use unscaled time since we're modifying Time.timeScale
            if (_speedBoostTimer <= 0)
            {
                EndSpeedBoost();
            }
        }
        
        // Update pallet freeze effect
        if (_palletFrozen)
        {
            _palletFreezeTimer -= Time.deltaTime;
            if (_palletFreezeTimer <= 0)
            {
                EndPalletFreeze();
            }
        }
        
        UpdateUI();
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
            
            // Check if we should make this a special egg (after delay and with random chance)
            if (_gameTime >= _specialEggDelay)
            {
                float randomValue = Random.Range(0f, 1f);
                
                if (randomValue < _redEggChance)
                {
                    ConvertToRedEgg(newEgg);
                }
                else if (randomValue < _redEggChance + _blueEggChance)
                {
                    ConvertToBlueEgg(newEgg);
                }
                else if (randomValue < _redEggChance + _blueEggChance + _orangeEggChance)
                {
                    ConvertToOrangeEgg(newEgg);
                }
                else if (randomValue < _redEggChance + _blueEggChance + _orangeEggChance + _greenEggChance)
                {
                    ConvertToGreenEgg(newEgg);
                }
                else if (randomValue < _redEggChance + _blueEggChance + _orangeEggChance + _greenEggChance + _purpleEggChance)
                {
                    ConvertToPurpleEgg(newEgg);
                }
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
    
    private void ConvertToBlueEgg(GameObject eggObject)
    {
        // Remove the regular Egg script
        Egg originalEgg = eggObject.GetComponent<Egg>();
        if (originalEgg != null)
        {
            DestroyImmediate(originalEgg);
        }
        
        // Add BlueEgg script
        eggObject.AddComponent<BlueEgg>();
        
        // Change color to blue
        SpriteRenderer spriteRenderer = eggObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }
        
        // Try to change material color as backup
        Renderer renderer = eggObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.blue;
        }
    }
    
    private void ConvertToOrangeEgg(GameObject eggObject)
    {
        // Remove the regular Egg script
        Egg originalEgg = eggObject.GetComponent<Egg>();
        if (originalEgg != null)
        {
            DestroyImmediate(originalEgg);
        }
        
        // Add OrangeEgg script
        eggObject.AddComponent<OrangeEgg>();
        
        // Change color to orange
        SpriteRenderer spriteRenderer = eggObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange color
        }
        
        // Try to change material color as backup
        Renderer renderer = eggObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.5f, 0f);
        }
    }
    
    private void ConvertToGreenEgg(GameObject eggObject)
    {
        // Remove the regular Egg script
        Egg originalEgg = eggObject.GetComponent<Egg>();
        if (originalEgg != null)
        {
            DestroyImmediate(originalEgg);
        }
        
        // Add GreenEgg script
        eggObject.AddComponent<GreenEgg>();
        
        // Change color to green
        SpriteRenderer spriteRenderer = eggObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }
        
        // Try to change material color as backup
        Renderer renderer = eggObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }
    
    private void ConvertToPurpleEgg(GameObject eggObject)
    {
        // Remove the regular Egg script
        Egg originalEgg = eggObject.GetComponent<Egg>();
        if (originalEgg != null)
        {
            DestroyImmediate(originalEgg);
        }
        
        // Add PurpleEgg script
        eggObject.AddComponent<PurpleEgg>();
        
        // Change color to purple/magenta
        SpriteRenderer spriteRenderer = eggObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.magenta;
        }
        
        // Try to change material color as backup
        Renderer renderer = eggObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.magenta;
        }
    }
    
    // Effect methods called by special eggs
    public void TriggerSlowMotion()
    {
        // End any current speed boost first
        if (_speedBoostActive)
        {
            EndSpeedBoost();
        }
        
        _slowMotionActive = true;
        _slowMotionTimer = _slowMotionDuration;
        Time.timeScale = 0.5f; // Slow down game to half speed
        
        ScorePoints(150); // Blue eggs give 150 points
    }
    
    public void TriggerSpeedBoost()
    {
        // End any current slow motion first
        if (_slowMotionActive)
        {
            EndSlowMotion();
        }
        
        _speedBoostActive = true;
        _speedBoostTimer = _speedBoostDuration;
        _purpleEggSurvivalBonus = true; // Player will get bonus if they survive
        Time.timeScale = 2.0f; // Speed up game to double speed
        
        ScorePoints(150); // Purple eggs give 150 points initially
    }
    
    public void TriggerPalletFreeze()
    {
        _palletFrozen = true;
        _palletFreezeTimer = _palletFreezeDuration;
        
        ScorePoints(150); // Orange eggs give 150 points (though they have negative effect)
    }
    
    public void TriggerHeal()
    {
        // Clear all negative effects
        if (_slowMotionActive)
        {
            EndSlowMotion();
        }
        if (_speedBoostActive)
        {
            EndSpeedBoost();
        }
        if (_palletFrozen)
        {
            EndPalletFreeze();
        }
        
        ScorePoints(150); // Green eggs give 150 points
    }
    
    private void EndSlowMotion()
    {
        _slowMotionActive = false;
        _slowMotionTimer = 0.0f;
        
        // Only reset time scale if no speed boost is active
        if (!_speedBoostActive)
        {
            Time.timeScale = 1.0f;
        }
    }
    
    private void EndSpeedBoost()
    {
        _speedBoostActive = false;
        _speedBoostTimer = 0.0f;
        
        // Give survival bonus if player survived the purple egg speed boost
        if (_purpleEggSurvivalBonus)
        {
            ScorePoints(499); // Bonus points for surviving purple egg speed boost
            _purpleEggSurvivalBonus = false;
        }
        
        // Only reset time scale if no slow motion is active
        if (!_slowMotionActive)
        {
            Time.timeScale = 1.0f;
        }
    }
    
    private void EndPalletFreeze()
    {
        _palletFrozen = false;
        _palletFreezeTimer = 0.0f;
    }
    
    public bool IsPalletFrozen()
    {
        return _palletFrozen;
    }
    
    public void ScorePoint()
    {
        ScorePoints(100); // Regular eggs give 100 points
    }
    
    public void ScorePoints(long points)
    {
        _score += points;
        
        // Check for game over condition (score below 0, but not from red egg)
        if (_score < 0 && points != -9999999999 && !_gameOver)
        {
            TriggerGameOver();
            return;
        }
        
        // Don't let score go below 0 unless it's the red egg penalty or game over
        if (_score < 0 && points != -9999999999 && !_gameOver) 
        {
            _score = 0;
        }
        UpdateUI();
    }
    
    public void LosePoint()
    {
        ScorePoints(-100); // Lose 100 points, but don't go below 0 unless it's red egg penalty
    }
    
    public void LoseBigPoints()
    {
        ScorePoints(-500); // Lose 500 points for missing eggs in score zones
    }
    
    public void TriggerGameOver()
    {
        if (_gameOver) return; // Prevent multiple game overs
        
        _gameOver = true;
        
        // Show final score
        if (_gameOverText != null)
        {
            _gameOverText.text = $"GAME OVER!\nFinal Score: {_score}\nPress R to Restart";
        }
        
        // Trigger explosion like red egg
        TriggerExplosion();
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
        
        // Update effects text
        if (_effectText != null)
        {
            string effectsDisplay = "";
            
            if (_slowMotionActive)
            {
                effectsDisplay += $"SLOW MOTION ({_slowMotionTimer:F1}s) ";
            }
            if (_speedBoostActive)
            {
                effectsDisplay += $"SPEED BOOST ({_speedBoostTimer:F1}s) ";
            }
            if (_palletFrozen)
            {
                effectsDisplay += $"FROZEN ({_palletFreezeTimer:F1}s) ";
            }
            
            _effectText.text = effectsDisplay;
        }
        
        // Show/hide game over text
        if (_gameOverText != null)
        {
            if (_gameOver)
            {
                _gameOverText.gameObject.SetActive(true);
            }
            else
            {
                _gameOverText.gameObject.SetActive(false);
            }
        }
    }
    
    public void StopGame()
    {
        _gameActive = false;
        
        // Reset time scale when stopping game
        Time.timeScale = 1.0f;
        _slowMotionActive = false;
        _speedBoostActive = false;
        _palletFrozen = false;
    }
    
    public void StartGame()
    {
        Debug.Log("Starting game - resetting all variables");
        
        _gameActive = true;
        _score = 0;
        _currentSpawnInterval = _spawnInterval;
        _spawnTimer = _currentSpawnInterval; // Reset spawn timer
        _isPaused = false;
        _gameTime = 0.0f;
        _exploded = false;
        _gameOver = false; // Reset game over state
        
        // Reset all effects
        Time.timeScale = 1.0f;
        _slowMotionActive = false;
        _speedBoostActive = false;
        _palletFrozen = false;
        _slowMotionTimer = 0.0f;
        _speedBoostTimer = 0.0f;
        _palletFreezeTimer = 0.0f;
        _purpleEggSurvivalBonus = false;
        
        Debug.Log($"Game started - gameActive: {_gameActive}, exploded: {_exploded}, gameOver: {_gameOver}, isPaused: {_isPaused}");
        Debug.Log($"Spawn settings - interval: {_currentSpawnInterval}, timer: {_spawnTimer}");
        
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
        
        // Only give massive negative points if it's from red egg (not game over)
        if (!_gameOver)
        {
            ScorePoints(-9999999999);
        }
        
        // Reset time scale during explosion
        Time.timeScale = 1.0f;
        _slowMotionActive = false;
        _speedBoostActive = false;
        _palletFrozen = false;
        _purpleEggSurvivalBonus = false;
        
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
        // First, destroy all eggs
        DestroyAllEggs();
        
        // Then restore all scene objects to their original state
        RestoreOriginalSceneState();
        
        // Finally, restart the game
        StartGame();
    }
    
    private void DestroyAllEggs()
    {
        // Destroy all eggs by finding them with components
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
        
        BlueEgg[] allBlueEggs = FindObjectsOfType<BlueEgg>();
        foreach (BlueEgg blueEgg in allBlueEggs)
        {
            if (blueEgg != null && blueEgg.gameObject != null)
            {
                Destroy(blueEgg.gameObject);
            }
        }
        
        OrangeEgg[] allOrangeEggs = FindObjectsOfType<OrangeEgg>();
        foreach (OrangeEgg orangeEgg in allOrangeEggs)
        {
            if (orangeEgg != null && orangeEgg.gameObject != null)
            {
                Destroy(orangeEgg.gameObject);
            }
        }
        
        GreenEgg[] allGreenEggs = FindObjectsOfType<GreenEgg>();
        foreach (GreenEgg greenEgg in allGreenEggs)
        {
            if (greenEgg != null && greenEgg.gameObject != null)
            {
                Destroy(greenEgg.gameObject);
            }
        }
        
        PurpleEgg[] allPurpleEggs = FindObjectsOfType<PurpleEgg>();
        foreach (PurpleEgg purpleEgg in allPurpleEggs)
        {
            if (purpleEgg != null && purpleEgg.gameObject != null)
            {
                Destroy(purpleEgg.gameObject);
            }
        }
    }
    
    private void RestoreOriginalSceneState()
    {
        foreach (ObjectState state in _originalObjectStates)
        {
            if (state.gameObject == null) continue; // Object was destroyed
            
            // Restore transform
            state.gameObject.transform.position = state.originalPosition;
            state.gameObject.transform.rotation = state.originalRotation;
            state.gameObject.transform.localScale = state.originalScale;
            
            // Restore physics
            Rigidbody2D rb = state.gameObject.GetComponent<Rigidbody2D>();
            if (state.hadRigidbody)
            {
                if (rb == null)
                {
                    // Re-add rigidbody if it was removed
                    rb = state.gameObject.AddComponent<Rigidbody2D>();
                }
                
                // Restore rigidbody properties
                rb.bodyType = state.originalBodyType;
                rb.gravityScale = state.originalGravityScale;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                
                // Reset other physics properties
                rb.linearDamping = 0f;
                rb.angularDamping = 0.05f;
            }
            else
            {
                // Remove rigidbody if it shouldn't have one
                if (rb != null)
                {
                    DestroyImmediate(rb);
                }
            }
            
            // Restore collider
            Collider2D collider = state.gameObject.GetComponent<Collider2D>();
            if (state.hadCollider && collider != null)
            {
                collider.isTrigger = state.originalIsTrigger;
            }
            
            // Ensure object is active
            if (!state.gameObject.activeInHierarchy)
            {
                state.gameObject.SetActive(true);
            }
        }
        
        Debug.Log("Restored all objects to original state");
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