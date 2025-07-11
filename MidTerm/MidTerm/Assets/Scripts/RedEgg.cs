using UnityEngine;

public class RedEgg : MonoBehaviour
{
    private float _fallSpeed = 2.0f;
    private Rigidbody2D _rb;
    private AudioSource _audioSource;
    
    // Pause system variables
    private Vector2 _pausedVelocity;
    private float _pausedAngularVelocity;
    private bool _wasPaused = false;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        
        // Set up physics similar to regular egg
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.1f;
        _rb.gravityScale = 1.0f;
        
        // Start falling
        _rb.linearVelocity = Vector2.down * _fallSpeed;
    }
    
    void Update()
    {
        // Handle pause/unpause for red eggs
        if (EggGameManager.Instance != null)
        {
            bool isPaused = EggGameManager.Instance.IsPaused();
            
            // Just became paused
            if (isPaused && !_wasPaused)
            {
                // Store current velocities
                _pausedVelocity = _rb.linearVelocity;
                _pausedAngularVelocity = _rb.angularVelocity;
                
                // Freeze the red egg
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
        if (other.gameObject.CompareTag("Player")) // Pallet catches red egg
        {
            // Trigger explosion effect!
            if (EggGameManager.Instance != null)
            {
                EggGameManager.Instance.TriggerExplosion();
            }
            
            // Play explosion sound if audio source exists (optional)
            if (_audioSource != null)
            {
                // Try to play a simple beep sound or use existing audio
                _audioSource.pitch = 0.5f; // Lower pitch for explosion effect
                _audioSource.Play();
            }
            
            // Destroy the red egg
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Ground")) // Red egg hits ground (missed)
        {
            // Play miss sound if audio source exists (optional)
            if (_audioSource != null)
            {
                _audioSource.pitch = 1.0f; // Normal pitch
                _audioSource.Play();
            }
            
            // Destroy the red egg
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle slope collisions - red eggs should bounce/roll naturally
        if (collision.gameObject.CompareTag("Slope"))
        {
            // Let physics handle the slope interaction naturally
        }
    }
} 