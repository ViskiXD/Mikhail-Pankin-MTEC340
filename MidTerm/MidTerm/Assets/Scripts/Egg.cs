using UnityEngine;

public class Egg : MonoBehaviour
{
    [SerializeField] private float _fallSpeed = 2.0f;
    
    private Rigidbody2D _rb;
    private AudioSource _audioSource;
    
    [SerializeField] private AudioClip _catchClip;
    [SerializeField] private AudioClip _missClip;
    
    // Pause system variables
    private Vector2 _pausedVelocity;
    private float _pausedAngularVelocity;
    private bool _wasPaused = false;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        
        // Set up physics similar to ball behavior
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.1f;
        _rb.gravityScale = 1.0f;
        
        // Start falling
        _rb.linearVelocity = Vector2.down * _fallSpeed;
    }
    
    void Update()
    {
        // Handle pause/unpause for eggs
        if (EggGameManager.Instance != null)
        {
            bool isPaused = EggGameManager.Instance.IsPaused();
            
            // Just became paused
            if (isPaused && !_wasPaused)
            {
                // Store current velocities
                _pausedVelocity = _rb.linearVelocity;
                _pausedAngularVelocity = _rb.angularVelocity;
                
                // Freeze the egg
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                
                _wasPaused = true;
            }
            // Just became unpaused
            else if (!isPaused && _wasPaused)
            {
                // Restore physics
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.linearVelocity = _pausedVelocity;
                _rb.angularVelocity = _pausedAngularVelocity;
                
                _wasPaused = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) // Pallet catches egg
        {
            // Increment score
            if (EggGameManager.Instance != null)
            {
                EggGameManager.Instance.ScorePoint();
            }
            
            if (_audioSource && _catchClip)
            {
                _audioSource.PlayOneShot(_catchClip);
            }
            
            // Destroy the egg
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Ground")) // Egg hits ground (missed)
        {
            if (_audioSource && _missClip)
            {
                _audioSource.PlayOneShot(_missClip);
            }
            
            // Destroy the egg
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle slope collisions - eggs should bounce/roll naturally
        if (collision.gameObject.CompareTag("Slope"))
        {
            // Let physics handle the slope interaction naturally
        }
    }
}
