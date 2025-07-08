using UnityEngine;

public class Egg : MonoBehaviour
{
    [SerializeField] private float _fallSpeed = 2.0f;
    
    private Rigidbody2D _rb;
    private AudioSource _audioSource;
    
    [SerializeField] private AudioClip _catchClip;
    [SerializeField] private AudioClip _missClip;

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
