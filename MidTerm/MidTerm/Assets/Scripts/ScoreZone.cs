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
        // Check if an egg entered the zone (but skip red eggs - they shouldn't trigger score penalty)
        if (other.GetComponent<Egg>() != null || 
            other.GetComponent<BlueEgg>() != null || other.GetComponent<OrangeEgg>() != null ||
            other.GetComponent<GreenEgg>() != null || other.GetComponent<PurpleEgg>() != null)
        {
            // Get the egg's rigidbody
            Rigidbody2D eggRb = other.GetComponent<Rigidbody2D>();
            
            if (eggRb != null)
            {
                // Apply crazy jump forces
                ApplyCrazyJump(eggRb);
                
                // Deduct 500 score points for missing an egg
                if (EggGameManager.Instance != null)
                {
                    EggGameManager.Instance.LoseBigPoints();
                }
                
                // Play jump sound
                if (_audioSource && _jumpClip)
                {
                    _audioSource.PlayOneShot(_jumpClip);
                }
            }
        }
        // Red eggs just pass through without penalty or jump effects
        else if (other.GetComponent<RedEgg>() != null)
        {
            // Red eggs are ignored - no score penalty, no jump effects
            Debug.Log("Red egg passed through score zone - no penalty applied");
        }
    }
    
    private void ApplyCrazyJump(Rigidbody2D eggRb)
    {
        // Calculate random jump force with variation
        float randomHorizontal = Random.Range(-_randomVariation, _randomVariation);
        float randomVertical = Random.Range(-_randomVariation/2, _randomVariation);
        
        Vector2 jumpDirection = new Vector2(
            _horizontalForce + randomHorizontal,
            _jumpForce + randomVertical
        );
        
        // Apply the force
        eggRb.AddForce(jumpDirection, ForceMode2D.Impulse);
        
        // Add some random spin for extra craziness
        float randomSpin = Random.Range(-10f, 10f);
        eggRb.angularVelocity = randomSpin;
        
        Debug.Log($"Applied crazy jump force: {jumpDirection} with spin: {randomSpin}");
    }
} 