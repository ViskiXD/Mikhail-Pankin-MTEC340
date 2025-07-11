using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 15.0f;
    [SerializeField] private float _horizontalForce = 8.0f;
    [SerializeField] private float _randomVariation = 3.0f; // Add randomness to make it "crazy"
    
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _jumpClip;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if an egg entered the zone
        if (other.GetComponent<Egg>() != null)
        {
            // Get the egg's rigidbody
            Rigidbody2D eggRb = other.GetComponent<Rigidbody2D>();
            
            if (eggRb != null)
            {
                // Apply crazy jump forces
                ApplyCrazyJump(eggRb);
                
                // Deduct score point
                if (EggGameManager.Instance != null)
                {
                    EggGameManager.Instance.LosePoint();
                }
                
                // Play jump sound
                if (_audioSource && _jumpClip)
                {
                    _audioSource.PlayOneShot(_jumpClip);
                }
            }
        }
    }
    
    private void ApplyCrazyJump(Rigidbody2D eggRb)
    {
        // Add random variation to make it "crazy"
        float randomJumpForce = _jumpForce + Random.Range(-_randomVariation, _randomVariation);
        float randomHorizontalForce = Random.Range(-_horizontalForce, _horizontalForce);
        
        // Apply upward and random horizontal force
        Vector2 jumpVector = new Vector2(randomHorizontalForce, randomJumpForce);
        eggRb.linearVelocity = Vector2.zero; // Reset velocity first
        eggRb.AddForce(jumpVector, ForceMode2D.Impulse);
        
        // Add some random angular velocity for extra craziness
        float randomSpin = Random.Range(-10f, 10f);
        eggRb.angularVelocity = randomSpin;
    }
} 